var myHeaders = new Headers();
myHeaders.append("X-FF-Company", "[UUID DE FF]");
myHeaders.append("X-FF-API-Key", "[KEY DE FF]");
myHeaders.append("X-FF-Branch", "[BRANCH DE FF]");
myHeaders.append("Content-Type", "application/json");
myHeaders.append("Accept", "application/json");

var raw = JSON.stringify({
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
});

var requestOptions = {
  method: 'POST',
  headers: myHeaders,
  body: raw,
  redirect: 'follow'
};

fetch("https://backend-qa-api.facturafacil.com.pa/api/pac/reception_fe/detailed/", requestOptions)
  .then(response => response.text())
  .then(result => console.log(result))
  .catch(error => console.log('error', error));
