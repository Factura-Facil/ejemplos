"""Envía un JSON de ejemplo a la API /detailed/ de Factura Fácil e interpreta la respuesta.

Uso:
    pip install requests
    export FF_COMPANY="<UUID de la empresa>"
    export FF_API_KEY="<API key>"
    export FF_BRANCH="<UUID de la sucursal>"   # opcional (ver README)
    python scripts/enviar.py ejemplos/01-facturas/01-consumidor-final-minimo.json

Variables opcionales:
    FF_BASE_URL   URL base (por defecto QA: https://qa-api.facturafacil.com.pa)
"""
import json
import os
import sys
import time

import requests

BASE_URL = os.environ.get('FF_BASE_URL', 'https://qa-api.facturafacil.com.pa')
MAX_RETRIES = 3


def main(path):
    with open(path, encoding='utf-8') as fh:
        payload = json.load(fh)

    headers = {
        'X-FF-Company': os.environ['FF_COMPANY'],
        'X-FF-API-Key': os.environ['FF_API_KEY'],
        'Content-Type': 'application/json',
        'Accept': 'application/json',
    }
    if os.environ.get('FF_BRANCH'):
        headers['X-FF-Branch'] = os.environ['FF_BRANCH']

    url = f'{BASE_URL}/api/pac/reception_fe/detailed/'
    for attempt in range(1, MAX_RETRIES + 1):
        response = requests.post(url, json=payload, headers=headers, timeout=60)
        if response.status_code in (429, 503):
            # La caja tiene otra emisión en curso; el documento NO se emitió, se puede reintentar
            wait = int(response.headers.get('Retry-After', '2'))
            print(f'HTTP {response.status_code}: reintento {attempt}/{MAX_RETRIES} en {wait}s')
            time.sleep(wait)
            continue
        break

    print(f'HTTP {response.status_code}')
    try:
        data = response.json()
    except ValueError:
        print(response.text)
        return 1

    if response.status_code == 201 and not data.get('rejected'):
        print('AUTORIZADO')
        for key in ('document_type', 'branch', 'pos', 'document_number', 'cufe',
                    'authorization_number', 'process_date', 'pdf_url'):
            print(f'  {key}: {data.get(key)}')
        return 0

    if data.get('duplicated') and data.get('original'):
        print('DUPLICADO: el documento ya existía, datos del original:')
        print(json.dumps(data['original'], indent=2, ensure_ascii=False))
        return 0

    print('RECHAZADO' if response.status_code == 201 else 'ERROR DE VALIDACIÓN')
    for msg in data.get('messages', []) or []:
        print(f"  [{msg.get('code')}] {msg.get('message')} {msg.get('path') or ''}".rstrip())
    if 'detail' in data:
        print(f"  {data['detail']}")
    return 1


if __name__ == '__main__':
    if len(sys.argv) != 2:
        print(__doc__)
        sys.exit(2)
    sys.exit(main(sys.argv[1]))
