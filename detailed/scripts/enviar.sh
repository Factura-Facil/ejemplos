#!/usr/bin/env bash
# Envía un JSON de ejemplo a la API /detailed/ de Factura Fácil.
#
# Uso:
#   export FF_COMPANY="<UUID de la empresa>"
#   export FF_API_KEY="<API key>"
#   export FF_BRANCH="<UUID de la sucursal>"   # opcional (ver README)
#   export FF_BASE_URL="https://qa-api.facturafacil.com.pa"  # opcional, QA por defecto
#   ./scripts/enviar.sh ejemplos/01-facturas/01-consumidor-final-minimo.json
set -euo pipefail

FILE="${1:?Indique el archivo JSON a enviar}"
: "${FF_COMPANY:?Defina FF_COMPANY}"
: "${FF_API_KEY:?Defina FF_API_KEY}"
BASE_URL="${FF_BASE_URL:-https://qa-api.facturafacil.com.pa}"

BRANCH_HEADER=()
if [[ -n "${FF_BRANCH:-}" ]]; then
  BRANCH_HEADER=(--header "X-FF-Branch: ${FF_BRANCH}")
fi

curl --silent --show-error --location \
  --request POST "${BASE_URL}/api/pac/reception_fe/detailed/" \
  --header "X-FF-Company: ${FF_COMPANY}" \
  --header "X-FF-API-Key: ${FF_API_KEY}" \
  ${BRANCH_HEADER[@]+"${BRANCH_HEADER[@]}"} \
  --header 'Content-Type: application/json' \
  --header 'Accept: application/json' \
  --write-out '\nHTTP %{http_code}\n' \
  --data-binary "@${FILE}"
