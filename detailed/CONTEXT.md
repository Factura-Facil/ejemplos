# Contexto: API de emisión `/detailed/`

Referencia completa de la API de emisión de documentos electrónicos de Factura Fácil (Panamá).
Aquí están **todos los campos** que acepta el body, sus catálogos y las reglas de validación.
Para ejemplos listos para usar, ver [`ejemplos/`](ejemplos/) y el [README](README.md).

---

## 1. Endpoint

| | |
|---|---|
| Método | `POST` |
| Ruta | `/api/pac/reception_fe/detailed/` |
| QA (pruebas) | `https://backend-qa-api.facturafacil.com.pa/api/pac/reception_fe/detailed/` |
| Producción | `https://api.facturafacil.com.pa/api/pac/reception_fe/detailed/` |
| Content-Type | `application/json` |

### Cabeceras HTTP

| Cabecera | Requerida | Descripción |
|---|---|---|
| `X-FF-Company` | Sí | UUID de la empresa emisora. |
| `X-FF-API-Key` | Sí | API Key entregada por Factura Fácil. |
| `X-FF-Branch` | Condicional | UUID de la sucursal emisora. Ver "¿Cómo se determina la sucursal?". |

### ¿Cómo se determina la sucursal emisora?

En este orden de prioridad:

1. `header.branch` en el body (objeto con `code` de 4 dígitos).
2. La cabecera HTTP `X-FF-Branch`.
3. La sucursal del API Key, si ese API Key tiene **una sola** sucursal asociada.

Si ninguna aplica, la API responde `400` con *"Se debe informar la sucursal que emite la factura"*.

---

## 2. Estructura general

```json
{
  "header":   { ...datos de control del envío... },
  "document": { ...contenido fiscal del documento... }
}
```

Convenciones:

- Los montos se pueden enviar como **string** (`"10.50"`) o número (`10.5`). Se recomienda string para
  evitar errores de punto flotante.
- Precios y cantidades aceptan hasta **6 decimales**; los totales se redondean a 2 decimales (half-up).
- Fechas: `YYYY-MM-DD`. Fecha/hora de emisión: `YYYY-MM-DD HH:MM:SS` (hora local de Panamá).
- Los campos que la API no reconoce se **ignoran en silencio** (no producen error). Revise bien los
  nombres: un typo como `"refered"` en vez de `"referred"` hace que el campo se pierda.

---

## 3. `header`

| Campo | Tipo | Req. | Descripción |
|---|---|---|---|
| `id` | string | Sí | Identificador de control del envío, para uso interno del cliente (p.ej. ID de la venta en su sistema). Se recomienda que sea único. |
| `environment` | string | Sí | `"2"` = pruebas (QA), `"1"` = producción. **Debe coincidir con el servidor**: enviar `"1"` a QA (o `"2"` a producción) devuelve 400. |
| `pos` | string(3) | No | Punto de facturación / caja, `000`–`999`. Por defecto `001`. Cada caja tiene su propia numeración. |
| `issue_date` | string | No | Fecha/hora de emisión `YYYY-MM-DD HH:MM:SS`, hora de Panamá. Por defecto: ahora. No puede ser más de 2 días en el futuro. |
| `branch` | objeto | No | Sucursal emisora (ver abajo). |

### `header.branch`

| Campo | Tipo | Req. | Descripción |
|---|---|---|---|
| `code` | string(4) | Sí | Código de sucursal, p.ej. `"0001"`. |
| `address` | string(≤100) | Cond. | Dirección. |
| `coordinates` | string | Cond. | `"latitud,longitud"`, p.ej. `"+8.9824,-79.5199"`. |
| `phone` | string | Cond. | `999-9999` o `9999-9999` (también se aceptan 7 u 8 dígitos sin guion). Formatos no reconocibles se descartan. |
| `location` | string | Cond. | Código de ubicación `provincia-distrito-corregimiento`, p.ej. `"8-8-7"` (catálogo DGI). |
| `email` | string | No | Correo de la sucursal. |

- Si la sucursal **ya existe** en Factura Fácil, solo `code` es necesario; los demás campos que no se
  envíen se completan con los datos registrados.
- Si la sucursal **no existe**, `address`, `coordinates`, `phone` y `location` son obligatorios.

---

## 4. `document`

| Campo | Tipo | Req. | Descripción |
|---|---|---|---|
| `fd_number` | entero | Sí | Número fiscal, `0`–`9999999999`. **`0` = la API asigna el siguiente número** de la caja (recomendado). Si envía su propio número, no puede repetirse ni reiniciarse. |
| `type` | string | No | Tipo de documento (ver catálogo). Por defecto `01`. |
| `sale_type` | string | No | Tipo de venta: `1` giro del negocio (defecto), `2` activo fijo, `3` bienes raíces, `4` prestación de servicios. |
| `info` | string(≤5000) | No | Información de interés que se imprime en el documento (CAFE). |
| `receptor` | objeto | Sí | Cliente. Ver §5. |
| `referred` | objeto | Cond. | Documento referenciado. **Obligatorio para tipos `04` y `05`.** Ver §6. |
| `items` | lista | Sí | 1 a 1000 ítems. Ver §7. |
| `payments` | lista | Sí | 1 a 10 formas de pago. Ver §9. |
| `discounts` | lista | No | Descuentos globales, máximo 5. Ver §8. |
| `payment_term` | entero | No | `1` contado (defecto), `2` crédito/plazo, `3` mixto. |
| `installments` | lista | Cond. | Cuotas. **Obligatorio si `payment_term` = 2** (1 a 99 cuotas). Ver §10. |
| `withholding` | objeto | No | Retención de ITBMS. Ver §11. |
| `dest_country` | string(2) | No | País de destino ISO-3166 alfa-2. Por defecto `PA`. Distinto de `PA` ⇒ exportación. |
| `export_info` | objeto | Cond. | **Obligatorio si `dest_country` ≠ `PA`**; prohibido si es `PA`. Ver §12. |
| `order_info` | objeto | No | Datos del pedido/orden de compra. Ver §13. |
| `total` | decimal | No | Total del documento. Si se omite, se calcula. Úselo solo si su sistema necesita forzar el total. |
| `trim_amounts` | booleano | No | Elimina ceros a la derecha de los montos en el XML. Útil con precios/cantidades de 3+ decimales. |
| `issue_type` | string | No | Tipo de emisión: `01` normal (defecto). Otros valores (`02`–`04`, contingencia/uso posterior) solo si Factura Fácil se lo indica. |

### Catálogo `document.type`

| Código | Documento | Notas |
|---|---|---|
| `01` | Factura de operación interna | La más común. `dest_country` debe ser `PA`. Alias: `FAC`. |
| `02` | Factura de importación | |
| `03` | Factura de exportación | Receptor tipo `04` (extranjero) y `dest_country` ≠ `PA` + `export_info`. |
| `04` | Nota de crédito referente a una FE | Requiere `referred`. Alias: `NTC`. |
| `05` | Nota de débito referente a una FE | Requiere `referred`. Alias: `NTD`. |
| `06` | Nota de crédito genérica | Sin documento referenciado. |
| `07` | Nota de débito genérica | Sin documento referenciado. |
| `08` | Factura de zona franca | Emitida por empresas de zona franca / Zona Libre de Colón. Venta local (`dest_country` `PA`) o reexportación (`dest_country` ≠ `PA` + `export_info`). No debe referenciar otra FE. |
| `09` | Reembolso | |
| `10` | Factura de operación extranjera | Receptor tipo `04` (extranjero). |

> Los tipos `02` y `09` tienen reglas adicionales de la DGI según la actividad del emisor;
> pruébelos primero en QA y consulte con soporte si tiene dudas.

---

## 5. `document.receptor`

| Campo | Tipo | Req. | Descripción |
|---|---|---|---|
| `type` | string | Sí | `01` contribuyente, `02` consumidor final, `03` gobierno, `04` extranjero. |
| `name` | string(≤100) | Sí | Nombre o razón social. Para consumidor final, si está vacío o tiene menos de 2 caracteres se usa `CONSUMIDOR FINAL`. |
| `ruc` | string(≤20) | Cond. | RUC, cédula o, para extranjeros, pasaporte / identificación tributaria extranjera. Los espacios se eliminan. |
| `ruc_type` | string | Cond. | `1` persona natural, `2` persona jurídica. |
| `dv` | string(≤2) | Cond. | Dígito verificador del RUC. |
| `address` | string(≤100) | Cond. | Dirección. |
| `location` | string | Cond. | Ubicación `provincia-distrito-corregimiento` (p.ej. `8-8-7`). Debe existir en el catálogo. |
| `email` | string | No | Correo al que se envía el documento autorizado. Si el formato no es válido se descarta (no da error). |
| `country` | string(2) | Cond. | País del documento del receptor extranjero (ISO alfa-2). |

### Requisitos por tipo de receptor

| Campo | `01` Contribuyente | `02` Consumidor final | `03` Gobierno | `04` Extranjero |
|---|---|---|---|---|
| `name` | ✔ | ✔ (o `CONSUMIDOR FINAL`) | ✔ | ✔ |
| `ruc` | ✔ (validado con la DGI) | opcional (cédula) | ✔ (validado con la DGI) | ✔ pasaporte / ID fiscal |
| `ruc_type` | ✔ | opcional (`1` si envía `ruc`) | opcional (se asume `2`) | — |
| `dv` | ✔ debe coincidir con la DGI | — | ✔ | — |
| `address` | ✔ | opcional | ✔ | recomendado |
| `location` | ✔ | opcional | ✔ | — |
| `country` | — | — | — | recomendado |
| `items[].gns` | — | — | ✔ **en todos los ítems** | — |

Reglas adicionales:

- **Consumidor final** con `ruc`: si no es una cédula con formato válido se descarta en silencio; un RUC
  de persona **jurídica** no está permitido para consumidor final (use tipo `01`).
- Para contribuyente y gobierno, la API consulta el RUC/DV en la DGI: un `dv` incorrecto o un RUC
  inexistente devuelve 400.

---

## 6. `document.referred` (notas de crédito/débito)

| Campo | Tipo | Req. | Descripción |
|---|---|---|---|
| `fd_number` | string | Sí | **CUFE** (66 caracteres) del documento original, o su número fiscal (hasta 10 dígitos). |
| `fd_date` | string | Sí | Fecha de emisión del documento original, `YYYY-MM-DD`. |

- Se recomienda enviar el **CUFE**: es inequívoco.
- Si envía el número fiscal, la API busca el documento emitido en la **misma sucursal, misma caja (`pos`)
  y en la fecha `fd_date`** y lo reemplaza por su CUFE. Si la nota se emite desde otra caja/sucursal,
  use el CUFE.

---

## 7. `document.items[]`

| Campo | Tipo | Req. | Descripción |
|---|---|---|---|
| `line` | entero ≥1 | Sí | Número secuencial de la línea. |
| `description` | string(2–500) | Sí | Descripción del bien o servicio. |
| `quantity` | decimal (6 dec.) | Sí | Cantidad. |
| `price` | decimal ≥0 (6 dec.) | Sí | Precio **unitario sin impuestos**. `0.00` para ítems sin valor (regalos/bonificaciones). |
| `discount` | decimal ≥0 (6 dec.) | Sí | Descuento **por unidad, en balboas** (no porcentaje). Debe ser menor que `price`. Envíe `0` si no aplica. |
| `taxes` | lista | Sí | Impuestos del ítem. Ver §7.1. |
| `mu` | string(1–20) | No | Unidad de medida (catálogo DGI), p.ej. `und`, `kg`, `lb`, `gal`, `Caja`, `Hora`, `Mes`. |
| `internal_code` | string(≤20) | No | Código interno / SKU. |
| `gns` | string(4) | Cond. | Código CPBS abreviado (Codificación Panameña de Bienes y Servicios), p.ej. `4321` equipo informático, `8111` servicios informáticos. **Obligatorio en todos los ítems si el receptor es gobierno.** |
| `expiration_date` | string | No | Fecha de caducidad `YYYY-MM-DD`. |
| `additional_info` | string(≤5000) | No | Información adicional del ítem. |

### Cálculo por ítem

```
subtotal_item = (price - discount) × quantity
total_item    = subtotal_item + Σ taxes[].amount
```

### 7.1 `items[].taxes[]`

| Campo | Tipo | Req. | Descripción |
|---|---|---|---|
| `type` | string | Sí | `01` ITBMS, `02` OTI (otras tasas/impuestos), `03` ISC (selectivo al consumo). |
| `code` | string(2) | Cond. | Requerido para ITBMS y OTI (ver catálogos). No aplica a ISC. |
| `amount` | decimal | Sí | Monto del impuesto **de toda la línea** (no por unidad). |
| `rate` | decimal | Cond. | Solo ISC, **obligatorio**: tasa como fracción (`0.07` = 7%). |

Se recomienda incluir **siempre** el impuesto ITBMS en cada ítem, aunque sea exento (`code` `00`, `amount` `0.00`).

Catálogo ITBMS (`type` `01`):

| `code` | Tasa | Uso típico |
|---|---|---|
| `00` | Exento (0%) | Alimentos básicos, medicinas, servicios médicos, exportaciones. |
| `01` | 7% | Tarifa general. |
| `02` | 10% | Bebidas alcohólicas, hospedaje. |
| `03` | 15% | Productos derivados del tabaco. |

```
ISC   = subtotal_item × rate
ITBMS = (subtotal_item + ISC) × tasa      → redondeado a 2 decimales
```

La base del ITBMS incluye el ISC cuando el producto tiene ambos impuestos; confirme el tratamiento
de su producto con su asesor fiscal.

Catálogo OTI (`type` `02`):

| `code` | Concepto |
|---|---|
| `00` | Cero |
| `01` | SUME 911 |
| `02` | Portabilidad numérica |
| `03` | Impuesto sobre seguro |
| `04` | Impuesto sobre seguro de autos |
| `05` | Tasa de salida aeropuerto (FZ) |
| `06` | Cargo incentivo desarrollo aeropuerto (F3) |
| `07` | Cargo de seguridad aeropuerto (AH) |
| `08` | Otros cargos (XT) |
| `09` | Combustible (YQ) |
| `10` | FECI |
| `11` | Intereses |

No repita el mismo `code` de OTI dentro de un mismo ítem.

---

## 8. Descuentos

Hay dos niveles y se pueden combinar:

| Nivel | Dónde | Cómo se expresa | Efecto |
|---|---|---|---|
| Por ítem | `items[].discount` | Monto **por unidad** en balboas | Reduce la base imponible del ítem: el ITBMS se calcula **después** del descuento. |
| Global | `document.discounts[]` | Monto total en balboas | Se resta del total del documento. **No** recalcula el ITBMS de los ítems. |

`document.discounts[]` (máximo 5):

| Campo | Tipo | Req. | Descripción |
|---|---|---|---|
| `amount` | decimal ≥0.01 (2 dec.) | Sí | Monto del descuento. |
| `description` | string(1–500) | No | Por defecto `Descuento`. Ej.: `Pronto pago`, `Cupón X`. |

Descuento porcentual por ítem: convierta el porcentaje a monto unitario.
Ejemplo: 20% sobre `price` `25.00` ⇒ `discount` `5.00`.

Para un ítem regalado, use `price` `0.00` (y `discount` `0`), no un descuento del 100%.

### Total del documento

```
total = Σ total_item − Σ discounts[].amount
```

---

## 9. `document.payments[]`

| Campo | Tipo | Req. | Descripción |
|---|---|---|---|
| `type` | string | Sí | Forma de pago (catálogo). |
| `amount` | decimal ≥0 | Sí | Monto pagado con esa forma. |
| `description` | string(10–100) | Cond. | **Obligatorio si `type` = `99`.** Si lo envía, mínimo 10 caracteres. |

| `type` | Forma de pago |
|---|---|
| `01` | Crédito (venta a crédito / por cobrar) |
| `02` | Efectivo |
| `03` | Tarjeta de crédito |
| `04` | Tarjeta de débito |
| `05` | Tarjeta de fidelización |
| `06` | Vale |
| `07` | Tarjeta de regalo |
| `08` | Transferencia / depósito bancario |
| `09` | Cheque |
| `10` | Punto de pago |
| `99` | Otro (describir en `description`) |

Reglas:

- `Σ payments[].amount` debe ser **≥ total**. Si es mayor, la diferencia se registra como **vuelto**.
  Si es menor, la API responde 400 (*"vuelto negativo"*).
- Ojo: `01` es **crédito**, no efectivo. Para efectivo use `02`.

---

## 10. Crédito y cuotas: `payment_term` + `installments[]`

| `payment_term` | Significado | `installments` |
|---|---|---|
| `1` | Contado (defecto) | No aplica |
| `2` | Crédito / plazo | **Obligatorio** (1–99) |
| `3` | Mixto (parte contado, parte crédito) | Opcional, recomendado |

`installments[]`:

| Campo | Tipo | Req. | Descripción |
|---|---|---|---|
| `expiration_date` | string | Sí | Vencimiento `YYYY-MM-DD`. |
| `value` | decimal ≥0 (2 dec.) | Sí | Monto de la cuota. |
| `info` | string(≤1000) | No | Información adicional. |

Buena práctica: la suma de las cuotas = monto pagado con `type` `01` (crédito).

---

## 11. `document.withholding` (retención de ITBMS)

| Campo | Tipo | Req. | Descripción |
|---|---|---|---|
| `code` | string | Sí | Tipo de retención (catálogo). |
| `amount` | decimal ≥0 | Sí\* | Monto retenido. \*Envíelo siempre. |

| `code` | Retención |
|---|---|
| `1` | Pago por servicio profesional al Estado (100%) |
| `2` | Pago por venta de bienes/servicios al Estado (50%) |
| `3` | Pago o acreditación a no domiciliado o empresa constituida en el exterior (100%) |
| `4` | Pago o acreditación por compra de bienes/servicios (50%) |
| `7` | Pago a comercio afiliado a sistema de TC/TD (50%) |
| `8` | Otros (disminución de la retención) |

El monto es el porcentaje indicado aplicado al ITBMS total. Ejemplo: ITBMS 140.00 con código `2` ⇒ `amount` 70.00.
La retención se informa en el documento, pero **no** cambia el total ni los pagos.

---

## 12. Exportación: `dest_country` + `export_info`

| Campo | Tipo | Req. | Descripción |
|---|---|---|---|
| `incoterms` | string(3) | Sí | `EXW`, `FCA`, `CPT`, `CIP`, `DAT`, `DAP`, `DDP`, `FAS`, `FOB`, `CFR`, `CIF`. |
| `currency` | string(3) | No | Moneda ISO-4217 (p.ej. `USD`, `EUR`, `COP`). |
| `exchange_rate` | decimal (4 dec.) | Cond. | **Obligatorio si `currency` ≠ `USD`.** |
| `value` | decimal | Cond. | Valor total en la moneda extranjera. **Obligatorio si `currency` ≠ `USD`.** |
| `currency_desc` | string | Cond. | Obligatorio si `currency` = `ZZZ` (otra moneda). |
| `port_of_shipment` | string(5–50) | No | Puerto de embarque. |

Reglas:

- `type` `03` ⇒ `dest_country` ≠ `PA`, `export_info` obligatorio, receptor tipo `04`.
- `dest_country` ≠ `PA` ⇒ `export_info` obligatorio.
- `dest_country` = `PA` con `export_info` ⇒ 400.
- Los montos del documento siempre van en **USD/balboas**; `value` es el equivalente en la moneda extranjera.
- Las exportaciones normalmente son exentas de ITBMS (`code` `00`).

---

## 13. `document.order_info` (pedido / orden de compra)

| Campo | Tipo | Req. | Descripción |
|---|---|---|---|
| `number` | string(1–12) | Sí | Número del pedido u orden de compra. |
| `info` | string(≤4000) | No | Información del pedido. |
| `acceptance_numbers` | lista de string(1–2) | No | Números de aceptación. |
| `receptor_code` | string(≤50) | No | Código del cliente en su sistema. |
| `system_code` | string(≤50) | No | Código del sistema emisor. |

---

## 14. Respuestas

Ejemplos completos en [`respuestas/`](respuestas/).

| HTTP | Caso | Cómo detectarlo |
|---|---|---|
| `201` | **Autorizado** | `rejected` = `false`. Trae `cufe`, `authorization_number`, `pdf_url`, `qr_code_data`, `document_number`, `process_date`, `document_uuid`. |
| `201` | **Rechazado por la DGI** | `rejected` = `true`; detalle en `messages[]` (`code`, `message`, `path`). Corrija y reenvíe. |
| `201` | **Duplicado** | `duplicated` = `true` (código `1002`); `original` trae los datos del documento ya autorizado con ese número. |
| `400` | Error de validación local | `rejected` = `true`; `messages[0].message` indica la ruta del campo, p.ej. `document->receptor->dv->...`. |
| `429` | Otra emisión en curso en la misma caja | Body `{detail, retry_after}` + cabecera `Retry-After`. El documento **no** se emitió: reintente. |
| `503` | Bloqueo temporal de la caja | Igual que 429: espere `Retry-After` segundos y reintente. |
| `401`/`403` | Credenciales inválidas | Revise `X-FF-Company` / `X-FF-API-Key`. |

Campos de `201`:

| Campo | Descripción |
|---|---|
| `rejected` | `false` = autorizado. **Es el campo a verificar**, no solo el HTTP 201. |
| `cufe` | Código Único de Factura Electrónica (66 caracteres). Guárdelo: se usa para notas de crédito/débito y consultas. |
| `authorization_number` | Número de protocolo de autorización de la DGI. |
| `document_number` | Número fiscal asignado (10 dígitos). |
| `document_type`, `branch`, `pos` | Tipo, sucursal y caja con que se emitió. |
| `process_date` | Fecha/hora de autorización. |
| `pdf_url` | URL de descarga del CAFE (representación impresa en PDF). |
| `qr_code_data` | URL del QR de consulta en la DGI. |
| `qr_image_b64` | Imagen del QR en base64 (`data:image/png;base64,...`). |
| `document_uuid` | ID del documento en Factura Fácil. |
| `messages[]` | Mensajes de la DGI (`type` `N` = notificación, `R` = rechazo). |
| `xml`, `service_response`, `digest_value`, `did` | Datos técnicos del envío (XML firmado, respuesta SOAP). |

Si el receptor tiene `email`, el documento autorizado se le envía automáticamente.

---

## 15. Reintentos y duplicados

- **Con `fd_number` = `0`** (numeración automática), cada request exitoso consume un número nuevo. Si su
  sistema reintenta tras un timeout de red, puede emitir el mismo documento dos veces con números distintos.
  Antes de reintentar, consulte si el documento ya se emitió (ver la documentación Swagger).
- **Con `fd_number` propio**, reenviar el mismo número en la misma sucursal/caja/tipo devuelve
  `duplicated` = `true` con los datos del `original` — los reintentos son seguros.
- `429` y `503` significan que el documento **no** se procesó: se puede reintentar directamente.

---

## 16. Campos que no tienen efecto hoy

Estos campos se aceptan sin error pero actualmente no se reflejan en el documento: `items[].medicine`,
`items[].manufacturing_date`, `items[].bar_code`, `items[].comments`. No los use para información que
deba aparecer en la factura; use `items[].additional_info` o `document.info`.

---

## 17. Documentación en línea

| | QA | Producción |
|---|---|---|
| Swagger | https://backend-qa-api.facturafacil.com.pa/swagger/ | https://api.facturafacil.com.pa/swagger/ |
| ReDoc | https://backend-qa-api.facturafacil.com.pa/redoc/ | https://api.facturafacil.com.pa/redoc/ |
| RapiDoc | https://backend-qa-api.facturafacil.com.pa/rapidoc/ | https://api.facturafacil.com.pa/rapidoc/ |
