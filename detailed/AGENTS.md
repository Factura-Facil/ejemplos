# AGENTS.md — Guía para asistentes de IA

Instrucciones para agentes de IA (Claude Code, Copilot, Cursor, etc.) que ayuden a integrar la API de
emisión `/detailed/` de Factura Fácil (facturación electrónica de Panamá, DGI).

## Fuentes de verdad

1. [`CONTEXT.md`](CONTEXT.md): todos los campos, catálogos y reglas. **Léalo antes de generar un payload.**
2. [`ejemplos/`](ejemplos/): payloads válidos; parta del más parecido al caso del usuario en vez de
   escribir uno desde cero.
3. [`respuestas/`](respuestas/): formato de las respuestas.
4. Swagger del ambiente (`/swagger/`) para endpoints distintos a la emisión.

No invente campos: la API **ignora en silencio** los campos desconocidos, así que un nombre mal
escrito no da error pero el dato se pierde.

## Endpoint

```
POST https://backend-qa-api.facturafacil.com.pa/api/pac/reception_fe/detailed/   (QA)
POST https://api.facturafacil.com.pa/api/pac/reception_fe/detailed/              (producción)

X-FF-Company: <UUID empresa>
X-FF-API-Key: <API key>
X-FF-Branch:  <UUID sucursal>     (opcional, ver CONTEXT.md §1)
Content-Type: application/json
```

Nunca escriba credenciales reales en código fuente; use variables de entorno o un gestor de secretos.

## Cómo construir un documento

1. **Tipo** (`document.type`): `01` factura, `04` NC, `05` ND, `06`/`07` NC/ND genérica, `03` exportación,
   `08` zona franca, `10` operación extranjera.
2. **Receptor** (`document.receptor.type`):
   - `02` consumidor final → basta `name` (o `CONSUMIDOR FINAL`).
   - `01` contribuyente → `ruc`, `ruc_type`, `dv`, `address`, `location` (todos obligatorios).
   - `03` gobierno → igual que `01` **y** `gns` en todos los ítems.
   - `04` extranjero → `ruc` (pasaporte / ID fiscal) y `country`.
3. **Ítems**: `price` = precio unitario **sin impuestos**; `discount` = descuento **por unidad en
   balboas**; `taxes[].amount` = impuesto **de toda la línea**.
4. **Calcule** (en `Decimal`, nunca `float`):
   ```
   subtotal = (price - discount) * quantity
   isc      = subtotal * rate                      # solo si hay ISC
   itbms    = round_half_up((subtotal + isc) * tasa, 2)   # 00=0, 01=0.07, 02=0.10, 03=0.15
   total_item = subtotal + itbms + isc + Σ oti
   total    = Σ total_item - Σ document.discounts[].amount
   ```
5. **Pagos**: `Σ payments[].amount ≥ total`. El excedente es vuelto.
6. **`fd_number: 0`** salvo que el sistema del usuario lleve su propia numeración fiscal.
7. **`environment`**: `"2"` QA, `"1"` producción.
8. Valide con `python scripts/calcular_totales.py archivo.json`.

## Reglas que más se olvidan

- `payments[].type` `01` es **Crédito**, no efectivo. Efectivo = `02`.
- `payments[].type` `99` exige `description` de 10 a 100 caracteres.
- `payment_term` `2` (crédito) exige `installments[]`.
- `04`/`05` exigen `referred` con `fd_number` (preferir el **CUFE** de 66 caracteres) y `fd_date`.
- Exportación `03`: receptor `04`, `dest_country` ≠ `PA`, `export_info.incoterms`; si `currency` ≠ `USD`,
  también `exchange_rate` y `value`.
- `export_info` con `dest_country` = `PA` es un error.
- ISC (`taxes[].type` `03`) requiere `rate` como fracción (`0.07`), no porcentaje (`7`).
- Máximos: 1000 ítems, 10 pagos, 5 descuentos globales, 99 cuotas.
- `header.issue_date` en formato `YYYY-MM-DD HH:MM:SS`, hora de Panamá, no más de 2 días en el futuro.
- Consumidor final no puede llevar RUC de persona jurídica.

## Manejo de la respuesta (código de integración)

```text
HTTP 201 y rejected == false     → AUTORIZADO. Persistir cufe, document_number, authorization_number, pdf_url.
HTTP 201 y duplicated == true    → ya existía; usar los datos de `original`.
HTTP 201 y rejected == true      → RECHAZADO por la DGI; mostrar messages[].code/message/path. No reintentar sin corregir.
HTTP 400                         → error de validación; messages[0].message trae la ruta del campo (document->items->[0]->...).
HTTP 429 / 503                   → no se emitió; esperar Retry-After y reintentar.
Timeout / error de red           → NO reintentar a ciegas si fd_number = 0 (puede duplicar la venta);
                                   verificar primero si el documento se emitió.
```

El código generado debe comprobar `rejected`, no solo el status HTTP.

## Al modificar este repositorio

- Cada ejemplo debe ser un JSON válido con `header` y `document`, montos como string con 2 decimales
  (más decimales solo si el caso lo requiere), y pasar `scripts/calcular_totales.py` sin avisos.
- Use datos ficticios: nunca RUC, cédulas, correos o credenciales reales de clientes.
- Nombre los archivos `NN-descripcion-en-kebab-case.json` dentro de la carpeta temática y agréguelos
  al índice del [README](README.md).
- Si un campo o regla cambia en la API, actualice primero `CONTEXT.md`.
