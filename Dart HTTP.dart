import 'dart:convert';
import 'package:http/http.dart' as http;

void main() async {
  var headers = {
    'X-FF-Company': '[UUID DE FF]',
    'X-FF-API-Key': '[KEY DE FF]',
    'Content-Type': 'application/json',
    'Accept': 'application/json'
  };
  var request = http.Request('POST', Uri.parse('https://backend-qa-api.facturafacil.com.pa/api/pac/reception_fe/detailed/'));
  request.body = json.encode({
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
  request.headers.addAll(headers);
  
  http.StreamedResponse response = await request.send();
  
  if (response.statusCode == 200) {
    print(await response.stream.bytesToString());
  }
  else {
    print(response.reasonPhrase);
  }
  
}

