<?php
$client = new Client();
$headers = [
  'X-FF-Company' => '[UUID DE FF]',
  'X-FF-API-Key' => '[KEY DE FF]',
  'X-FF-Branch' => '[BRANCH DE FF]',
  'Content-Type' => 'application/json',
  'Accept' => 'application/json'
];
$body = '{
  "header": {
    "id": 1,
    "environment": "2"
  },
  "document": {
    "fd_number": "12",
    "receptor": {
      "type": "02",
      "name": "Nombre Cliente",
      "ruc_type": "1",
      "address": "Dirección Cliente",
      "email": "cliente@correo.com",
      "ruc": "123123123"
    },
    "items": [
      {
        "line": 1,
        "price": 0.5,
        "mu": "und",
        "quantity": 1,
        "description": "Producto de prueba",
        "taxes": [
          {
            "type": "01",
            "amount": 0.035,
            "code": "01"
          }
        ],
        "discount": 0,
        "internal_code": "123123"
      }
    ],
    "payments": [
      {
        "type": "99",
        "amount": "0.54",
        "description": "Medio de pago de prueba"
      }
    ],
    "type": "01",
    "info": "<string>"
  }
}';
$request = new Request('POST', 'https://backend-qa-api.facturafacil.com.pa/api/pac/reception_fe/detailed/', $headers, $body);
$res = $client->sendAsync($request)->wait();
echo $res->getBody();

