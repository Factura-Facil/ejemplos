<?php
require_once 'HTTP/Request2.php';
$request = new HTTP_Request2();
$request->setUrl('https://backend-qa-api.facturafacil.com.pa/api/pac/reception_fe/detailed/');
$request->setMethod(HTTP_Request2::METHOD_POST);
$request->setConfig(array(
  'follow_redirects' => TRUE
));
$request->setHeader(array(
  'X-FF-Company' => '[UUID DE FF]',
  'X-FF-API-Key' => '[KEY DE FF]',
  'X-FF-Branch' => '[BRANCH DE FF]',
  'Content-Type' => 'application/json',
  'Accept' => 'application/json'
));
$request->setBody('{\n  "header": {\n    "id": 1,\n    "environment": "2"\n  },\n  "document": {\n    "fd_number": "12",\n    "receptor": {\n      "type": "02",\n      "name": "Nombre Cliente",\n      "ruc_type": "1",\n      "address": "Dirección Cliente",\n      "email": "cliente@correo.com",\n      "ruc": "123123123"\n    },\n    "items": [\n      {\n        "line": 1,\n        "price": 0.50,\n        "mu": "und",\n        "quantity": 1,\n        "description": "Producto de prueba",\n        "taxes": [\n          {\n            "type": "01",\n            "amount":0.035,\n            "code": "01"\n          }\n        ],\n        "discount": 0.0,\n        "internal_code": "123123"\n      }\n    ],\n    "payments": [\n      {\n        "type": "99",\n        "amount": "0.54",\n        "description": "Medio de pago de prueba"\n      }\n    ],\n    "type": "01",\n    "info": "<string>"\n  }\n}');
try {
  $response = $request->send();
  if ($response->getStatus() == 200) {
    echo $response->getBody();
  }
  else {
    echo 'Unexpected HTTP status: ' . $response->getStatus() . ' ' .
    $response->getReasonPhrase();
  }
}
catch(HTTP_Request2_Exception $e) {
  echo 'Error: ' . $e->getMessage();
}
