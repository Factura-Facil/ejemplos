"""Calcula los totales de un JSON de /detailed/ sin enviarlo (no requiere credenciales).

Replica el cálculo que hace la API antes de enviar el documento a la DGI:

    subtotal_item = (price - discount) * quantity
    total_item    = subtotal_item + suma(taxes[].amount)
    total         = suma(total_item) - suma(discounts[].amount)
    vuelto        = suma(payments[].amount) - total   (no puede ser negativo)

Además advierte cuando el ITBMS informado no coincide con la tasa del código
(00=0%, 01=7%, 02=10%, 03=15%) o el ISC no coincide con subtotal * rate.

Uso:
    python scripts/calcular_totales.py ejemplos/02-descuentos/*.json
"""
import json
import sys
from decimal import Decimal as D, ROUND_HALF_UP

ITBMS = {'00': D('0'), '01': D('0.07'), '02': D('0.10'), '03': D('0.15')}
TOLERANCIA = D('0.01')


def r2(value):
    return D(value).quantize(D('0.01'), rounding=ROUND_HALF_UP)


def analizar(path):
    with open(path, encoding='utf-8') as fh:
        document = json.load(fh)['document']

    avisos = []
    total_items = D('0')
    total_itbms = D('0')
    for item in document['items']:
        price, qty = D(str(item['price'])), D(str(item['quantity']))
        discount = D(str(item.get('discount', 0)))
        if discount and discount >= price:
            avisos.append(f"línea {item['line']}: el descuento debe ser menor que el precio unitario")
        subtotal = (price - discount) * qty
        isc = D('0')
        for tax in item['taxes']:
            if tax['type'] == '03':
                isc = D(str(tax['amount']))
                esperado = subtotal * D(str(tax['rate']))
                if abs(isc - esperado) > TOLERANCIA:
                    avisos.append(f"línea {item['line']}: ISC {isc} ≠ subtotal × rate = {r2(esperado)}")
        for tax in item['taxes']:
            if tax['type'] == '01':
                itbms = D(str(tax['amount']))
                total_itbms += itbms
                esperado = (subtotal + isc) * ITBMS.get(tax.get('code'), D('0'))
                if abs(itbms - esperado) > TOLERANCIA:
                    avisos.append(f"línea {item['line']}: ITBMS {itbms} ≠ {r2(esperado)} "
                                  f"(código {tax.get('code')})")
        total_items += subtotal + sum(D(str(t['amount'])) for t in item['taxes'])

    descuentos = sum((D(str(d['amount'])) for d in document.get('discounts', [])), D('0'))
    total = r2(document.get('total') or (total_items - descuentos))
    pagado = sum((r2(D(str(p['amount']))) for p in document['payments']), D('0'))
    vuelto = pagado - total
    if vuelto < 0:
        avisos.append(f'los pagos ({pagado}) no cubren el total ({total})')

    if document.get('installments'):
        cuotas = sum(D(str(c['value'])) for c in document['installments'])
        credito = sum((D(str(p['amount'])) for p in document['payments'] if p['type'] == '01'), D('0'))
        if credito and abs(cuotas - credito) > TOLERANCIA:
            avisos.append(f'las cuotas ({cuotas}) no suman lo pagado a crédito ({credito})')

    print(f'{path}')
    print(f'  ítems: {len(document["items"])}  ITBMS: {r2(total_itbms)}  '
          f'descuentos globales: {r2(descuentos)}')
    print(f'  TOTAL: {total}  pagado: {pagado}  vuelto: {r2(max(vuelto, D("0")))}')
    for aviso in avisos:
        print(f'  ⚠ {aviso}')
    return not avisos


if __name__ == '__main__':
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(2)
    resultados = [analizar(p) for p in sys.argv[1:]]
    sys.exit(0 if all(resultados) else 1)
