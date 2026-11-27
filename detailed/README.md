# Ejemplos de la API `/detailed/` — Factura Fácil

Colección de ejemplos **listos para enviar** a la API de emisión de documentos electrónicos de
Factura Fácil (Panamá): facturas, notas de crédito y débito, exportaciones, descuentos, formas de pago,
crédito en cuotas, impuestos (ITBMS, ISC, OTI) y retenciones.

Todos los JSON tienen montos consistentes (impuestos, totales y pagos cuadran) y fueron validados
contra las reglas de la API.

| Archivo | Para qué sirve |
|---|---|
| [`CONTEXT.md`](CONTEXT.md) | Referencia completa: todos los campos, catálogos y reglas de validación. |
| [`AGENTS.md`](AGENTS.md) | Guía para asistentes de IA (Claude, Copilot, Cursor…) que generen integraciones. |
| [`ejemplos/`](ejemplos/) | Bodies JSON por caso de uso. |
| [`respuestas/`](respuestas/) | Respuestas de ejemplo: autorizado, rechazado, duplicado, errores. |
| [`scripts/`](scripts/) | Enviar un ejemplo (`curl` / Python) y calcular totales sin enviar. |

---

## Inicio rápido

### 1. Credenciales

Solicite al equipo de Factura Fácil:

- **UUID de la empresa** → cabecera `X-FF-Company`
- **API Key** → cabecera `X-FF-API-Key`
- **UUID de la sucursal** → cabecera `X-FF-Branch` (no es necesario si su API Key pertenece a una sola
  sucursal o si envía `header.branch` en el body)

### 2. Enviar un ejemplo al ambiente de pruebas

```bash
export FF_COMPANY="<UUID de la empresa>"
export FF_API_KEY="<API key>"
export FF_BRANCH="<UUID de la sucursal>"   # opcional

# con curl
./scripts/enviar.sh ejemplos/01-facturas/01-consumidor-final-minimo.json

# o con Python (interpreta la respuesta y reintenta si la caja está ocupada)
pip install requests
python scripts/enviar.py ejemplos/01-facturas/01-consumidor-final-minimo.json
```

O directamente:

```bash
curl -X POST 'https://backend-qa-api.facturafacil.com.pa/api/pac/reception_fe/detailed/' \
  -H "X-FF-Company: $FF_COMPANY" \
  -H "X-FF-API-Key: $FF_API_KEY" \
  -H "X-FF-Branch: $FF_BRANCH" \
  -H 'Content-Type: application/json' \
  --data-binary @ejemplos/01-facturas/01-consumidor-final-minimo.json
```

### 3. Verificar la respuesta

Un HTTP `201` **no significa autorizado**: revise el campo `rejected`.

```jsonc
{
  "rejected": false,                 // false = autorizado por la DGI
  "cufe": "FE0120000155555555-...",  // guárdelo: se usa en notas de crédito/débito
  "document_number": "0000000001",
  "authorization_number": "...",
  "pdf_url": "https://.../download_cafe/?cufe=...",
  "qr_code_data": "https://dgi-fep.mef.gob.pa/Consultas/FacturasPorQR?..."
}
```

Ver [`respuestas/`](respuestas/) y la sección 14 de [`CONTEXT.md`](CONTEXT.md).

### 4. Calcular totales sin enviar

```bash
python scripts/calcular_totales.py mi-factura.json
```

Muestra total, ITBMS y vuelto, y advierte si un ITBMS/ISC no corresponde a su tasa o si los pagos no
cubren el total.

---

## Antes de usar un ejemplo

- **`environment`**: `"2"` en QA, `"1"` en producción. Los ejemplos usan `"2"`.
- **`fd_number: 0`**: la API asigna el número fiscal siguiente de la caja. Es lo recomendado.
- **RUC y DV**: los ejemplos de contribuyente y gobierno usan RUC ficticios (`155555555-2-2020`,
  `dv` `00`). Para contribuyente (`01`) y gobierno (`03`) la API valida el RUC y el DV contra la DGI:
  **reemplácelos por los de un cliente real**.
- **Notas de crédito/débito**: `referred` debe apuntar a un documento que exista. Reemplace el CUFE /
  número y la fecha por los de una factura emitida en su ambiente.
- **`header.issue_date`** (solo en `09-sucursal-pos-y-fecha-en-body.json`): ajústela a la fecha actual;
  no puede ser más de 2 días en el futuro.
- **`header.branch`**: los ejemplos que la incluyen usan códigos `0002`/`0003`; use los de sus
  sucursales.

---

## Índice de ejemplos

### `01-facturas/` — Factura de operación interna (`type` `01`) por tipo de receptor

| Archivo | Caso |
|---|---|
| [`01-consumidor-final-minimo.json`](ejemplos/01-facturas/01-consumidor-final-minimo.json) | El mínimo indispensable: consumidor final anónimo, un ítem, efectivo. |
| [`02-consumidor-final-con-cedula.json`](ejemplos/01-facturas/02-consumidor-final-con-cedula.json) | Consumidor final que pide factura con su cédula y correo. Tarjeta de crédito. |
| [`03-contribuyente-persona-natural.json`](ejemplos/01-facturas/03-contribuyente-persona-natural.json) | Contribuyente persona natural (`ruc_type` 1) con DV, dirección y ubicación. Venta de servicios (`sale_type` 4). |
| [`04-contribuyente-persona-juridica.json`](ejemplos/01-facturas/04-contribuyente-persona-juridica.json) | Empresa (`ruc_type` 2), varios ítems, transferencia. |
| [`05-gobierno-con-cpbs.json`](ejemplos/01-facturas/05-gobierno-con-cpbs.json) | Entidad de gobierno (`type` 03): `gns` (CPBS) obligatorio en todos los ítems. A crédito. |
| [`06-receptor-extranjero-turista.json`](ejemplos/01-facturas/06-receptor-extranjero-turista.json) | Extranjero (`type` 04) que compra en Panamá: pasaporte + país. |
| [`07-multiples-tasas-itbms.json`](ejemplos/01-facturas/07-multiples-tasas-itbms.json) | Un ítem por cada tasa de ITBMS: exento, 7%, 10% y 15%. |
| [`08-numero-fiscal-propio.json`](ejemplos/01-facturas/08-numero-fiscal-propio.json) | El sistema del cliente controla la numeración (`fd_number` propio) en la caja `002`. |
| [`09-sucursal-pos-y-fecha-en-body.json`](ejemplos/01-facturas/09-sucursal-pos-y-fecha-en-body.json) | Sucursal, caja y fecha/hora de emisión informadas en `header`. |
| [`10-campos-opcionales-de-item.json`](ejemplos/01-facturas/10-campos-opcionales-de-item.json) | Fecha de caducidad, información adicional del ítem, cantidades fraccionadas y `info` del documento. |

### `02-descuentos/`

| Archivo | Caso |
|---|---|
| [`01-descuento-por-item.json`](ejemplos/02-descuentos/01-descuento-por-item.json) | Descuento fijo por unidad en un ítem (el ITBMS se calcula sobre el precio con descuento). |
| [`02-descuento-porcentual-por-item.json`](ejemplos/02-descuentos/02-descuento-porcentual-por-item.json) | 20% de descuento convertido a monto por unidad. |
| [`03-descuento-global.json`](ejemplos/02-descuentos/03-descuento-global.json) | Descuento sobre el total de la factura (`document.discounts`). |
| [`04-descuento-item-y-global.json`](ejemplos/02-descuentos/04-descuento-item-y-global.json) | Combinación: descuento por ítem + dos descuentos globales. |
| [`05-item-gratis-bonificacion.json`](ejemplos/02-descuentos/05-item-gratis-bonificacion.json) | Producto regalado (`price` 0.00). |

### `03-pagos/`

| Archivo | Caso |
|---|---|
| [`01-pago-mixto.json`](ejemplos/03-pagos/01-pago-mixto.json) | Parte en efectivo, parte con tarjeta. |
| [`02-efectivo-con-vuelto.json`](ejemplos/03-pagos/02-efectivo-con-vuelto.json) | El cliente paga más que el total: se registra el vuelto. |
| [`03-otro-medio-de-pago-99.json`](ejemplos/03-pagos/03-otro-medio-de-pago-99.json) | Forma de pago `99` con descripción obligatoria (p.ej. billetera digital). |
| [`04-credito-a-plazo-en-cuotas.json`](ejemplos/03-pagos/04-credito-a-plazo-en-cuotas.json) | Venta a crédito (`payment_term` 2) en 3 cuotas. |
| [`05-pago-mixto-contado-y-credito.json`](ejemplos/03-pagos/05-pago-mixto-contado-y-credito.json) | Abono inicial + saldo a crédito en 2 cuotas (`payment_term` 3). |
| [`06-todos-los-medios-de-pago.json`](ejemplos/03-pagos/06-todos-los-medios-de-pago.json) | Referencia: los 10 medios de pago en un solo documento. |

### `04-impuestos/`

| Archivo | Caso |
|---|---|
| [`01-isc-impuesto-selectivo-al-consumo.json`](ejemplos/04-impuestos/01-isc-impuesto-selectivo-al-consumo.json) | ISC (`type` 03, con `rate`) + ITBMS en el mismo ítem. |
| [`02-oti-otros-impuestos.json`](ejemplos/04-impuestos/02-oti-otros-impuestos.json) | OTI: SUME 911 y portabilidad numérica en un plan de telefonía. |
| [`03-exento-total.json`](ejemplos/04-impuestos/03-exento-total.json) | Documento totalmente exento de ITBMS. |
| [`04-retencion-itbms.json`](ejemplos/04-impuestos/04-retencion-itbms.json) | Venta al Estado con retención del 50% del ITBMS (`withholding`). |

### `05-notas-credito-debito/`

| Archivo | Caso |
|---|---|
| [`01-nota-credito-por-numero.json`](ejemplos/05-notas-credito-debito/01-nota-credito-por-numero.json) | Devolución parcial (`type` 04) referenciando la factura por su número fiscal. |
| [`02-nota-credito-por-cufe.json`](ejemplos/05-notas-credito-debito/02-nota-credito-por-cufe.json) | Ajuste de precio referenciando la factura por su CUFE (recomendado). |
| [`03-nota-debito.json`](ejemplos/05-notas-credito-debito/03-nota-debito.json) | Cargo adicional (`type` 05) sobre una factura. |
| [`04-nota-credito-generica.json`](ejemplos/05-notas-credito-debito/04-nota-credito-generica.json) | Nota de crédito genérica (`type` 06), sin factura referenciada. |
| [`05-nota-debito-generica.json`](ejemplos/05-notas-credito-debito/05-nota-debito-generica.json) | Nota de débito genérica (`type` 07): intereses por mora. |
| [`06-alias-ntc.json`](ejemplos/05-notas-credito-debito/06-alias-ntc.json) | Uso del alias `NTC` (equivale a `04`). |

### `06-exportacion/`

| Archivo | Caso |
|---|---|
| [`01-exportacion-usd.json`](ejemplos/06-exportacion/01-exportacion-usd.json) | Exportación (`type` 03) a Colombia en USD, incoterm FOB. |
| [`02-exportacion-otra-moneda.json`](ejemplos/06-exportacion/02-exportacion-otra-moneda.json) | Exportación a España facturada en EUR: `exchange_rate` y `value` obligatorios. |
| [`03-operacion-extranjera-tipo-10.json`](ejemplos/06-exportacion/03-operacion-extranjera-tipo-10.json) | Factura de operación extranjera (`type` 10). |

### `07-avanzado/`

| Archivo | Caso |
|---|---|
| [`01-orden-de-compra.json`](ejemplos/07-avanzado/01-orden-de-compra.json) | Datos de pedido / orden de compra del cliente (`order_info`). |
| [`02-total-explicito-y-trim-amounts.json`](ejemplos/07-avanzado/02-total-explicito-y-trim-amounts.json) | Precio y cantidad con 3 decimales (combustible), `total` explícito y `trim_amounts`. |
| [`03-muchos-items.json`](ejemplos/07-avanzado/03-muchos-items.json) | 25 ítems con tasas mezcladas (ticket de supermercado). |
| [`04-documento-completo.json`](ejemplos/07-avanzado/04-documento-completo.json) | Combina casi todo: sucursal en body, CPBS, descuentos por ítem y global, pago mixto con cuota, orden de compra. |

### `08-zona-franca/` — Factura de zona franca (`type` `08`)

Para empresas establecidas en una zona franca o en la Zona Libre de Colón.

| Archivo | Caso |
|---|---|
| [`01-venta-local-zona-franca.json`](ejemplos/08-zona-franca/01-venta-local-zona-franca.json) | Venta a una empresa en Panamá, ítems exentos de ITBMS, pago por transferencia. |
| [`02-reexportacion-zona-franca.json`](ejemplos/08-zona-franca/02-reexportacion-zona-franca.json) | Reexportación a Costa Rica: receptor extranjero, `dest_country` + `export_info`, a crédito a 45 días. |

---

## Combinaciones válidas de un vistazo

| `type` | Receptor permitido | `referred` | `dest_country` / `export_info` |
|---|---|---|---|
| `01` Factura | `01`, `02`, `03`, `04` | — | `PA`, sin `export_info` |
| `03` Exportación | `04` | — | ≠ `PA` + `export_info` |
| `04` Nota de crédito | igual que la factura original | **obligatorio** | igual que la original |
| `05` Nota de débito | igual que la factura original | **obligatorio** | igual que la original |
| `06` / `07` NC/ND genérica | `01`, `02`, `03`, `04` | — | `PA` |
| `08` Zona franca | `01`, `02`, `03`, `04` | — (no debe referenciar otra FE) | `PA`, o ≠ `PA` + `export_info` (reexportación) |
| `10` Operación extranjera | `04` | — | — |

## Errores frecuentes

| Síntoma | Causa | Solución |
|---|---|---|
| `document->receptor->dv->El dígito verificador es requerido…` | Contribuyente/gobierno sin `dv` | Agregue `dv`, `address` y `location`. |
| `El ambiente de destino producción no es válido…` | `environment` no corresponde al servidor | `"2"` en QA, `"1"` en producción. |
| `No se puede generar documento con vuelto negativo` | Los pagos no cubren el total | Ajuste `payments[].amount` (use `scripts/calcular_totales.py`). |
| `La descripción es obligatoria cuando el tipo de pago es 99` | Pago `99` sin `description` | Agregue `description` (10 a 100 caracteres). |
| `El código de bien o servicio (item.gns) es obligatorio…` | Receptor gobierno | Agregue `gns` (CPBS de 4 dígitos) en **todos** los ítems. |
| `El documento referenciado es obligatorio…` | Nota `04`/`05` sin `referred` | Agregue `referred.fd_number` (CUFE) y `referred.fd_date`. |
| `rejected: true` con código `1002` | Número fiscal ya usado | Ver `original` en la respuesta, o envíe `fd_number: 0`. |
| HTTP 429 / 503 | Otra emisión en curso en la misma caja | Espere `Retry-After` segundos y reintente. |
| Se paga en efectivo pero sale "Crédito" | `payments[].type` = `01` | Efectivo es `02`; `01` es crédito. |

---

## Regenerar o validar los ejemplos

Los JSON tienen montos calculados con la misma fórmula que usa la API. Si edita uno, verifique:

```bash
python scripts/calcular_totales.py ejemplos/*/*.json
```
