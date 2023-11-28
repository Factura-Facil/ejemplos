<?php

$curl = curl_init();

curl_setopt_array($curl, array(
  CURLOPT_URL => 'https://backend-qa-api.facturafacil.com.pa/api/pac/reception_fe/detailed/',
  CURLOPT_RETURNTRANSFER => true,
  CURLOPT_ENCODING => '',
  CURLOPT_MAXREDIRS => 10,
  CURLOPT_TIMEOUT => 0,
  CURLOPT_FOLLOWLOCATION => true,
  CURLOPT_HTTP_VERSION => CURL_HTTP_VERSION_1_1,
  CURLOPT_CUSTOMREQUEST => 'POST',
  CURLOPT_POSTFIELDS =>'{
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
        "price": 0.50,
        "mu": "und",
        "quantity": 1,
        "description": "Producto de prueba",
        "taxes": [
          {
            "type": "01",
            "amount":0.035,
            "code": "01"
          }
        ],
        "discount": 0.0,
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
}',
  CURLOPT_HTTPHEADER => array(
    'X-FF-Company: [UUID DE FF]',
    'X-FF-API-Key: [KEY DE FF]',
    'X-FF-Branch: [BRANCH DE FF]',
    'Content-Type: application/json',
    'Accept: application/json'
  ),
));

$response = curl_exec($curl);

curl_close($curl);
echo $response;

