import com.mashape.unirest.http.*;
import java.io.*;
public class main {
  public static void main(String []args) throws Exception{
    Unirest.setTimeouts(0, 0);
    HttpResponse<String> response = Unirest.post("https://backend-qa-api.facturafacil.com.pa/api/pac/reception_fe/detailed/")
      .header("X-FF-Company", "[UUID DE FF]")
      .header("X-FF-API-Key", "[KEY DE FF]")
      .header("Content-Type", "application/json")
      .header("Accept", "application/json")
      .body("{\n  \"header\": {\n    \"id\": 1,\n    \"environment\": \"2\"\n  },\n  \"document\": {\n    \"fd_number\": \"12\",\n    \"receptor\": {\n      \"type\": \"02\",\n      \"name\": \"Nombre Cliente\",\n      \"ruc_type\": \"1\",\n      \"address\": \"Dirección Cliente\",\n      \"email\": \"cliente@correo.com\",\n      \"ruc\": \"123123123\"\n    },\n    \"items\": [\n      {\n        \"line\": 1,\n        \"price\": 0.50,\n        \"mu\": \"und\",\n        \"quantity\": 1,\n        \"description\": \"Producto de prueba\",\n        \"taxes\": [\n          {\n            \"type\": \"01\",\n            \"amount\":0.035,\n            \"code\": \"01\"\n          }\n        ],\n        \"discount\": 0.0,\n        \"internal_code\": \"123123\"\n      }\n    ],\n    \"payments\": [\n      {\n        \"type\": \"99\",\n        \"amount\": \"0.54\",\n        \"description\": \"Medio de pago de prueba\"\n      }\n    ],\n    \"type\": \"01\",\n    \"info\": \"<string>\"\n  }\n}")
      .asString();
    
    System.out.println(response.getBody());
  }
}

