import java.io.*;
import okhttp3.*;
public class main {
  public static void main(String []args) throws IOException{
    OkHttpClient client = new OkHttpClient().newBuilder()
      .build();
    MediaType mediaType = MediaType.parse("application/json");
    RequestBody body = RequestBody.create(mediaType, "{\n  \"header\": {\n    \"id\": 1,\n    \"environment\": \"2\"\n  },\n  \"document\": {\n    \"fd_number\": \"12\",\n    \"receptor\": {\n      \"type\": \"02\",\n      \"name\": \"Nombre Cliente\",\n      \"ruc_type\": \"1\",\n      \"address\": \"Dirección Cliente\",\n      \"email\": \"cliente@correo.com\",\n      \"ruc\": \"123123123\"\n    },\n    \"items\": [\n      {\n        \"line\": 1,\n        \"price\": 0.50,\n        \"mu\": \"und\",\n        \"quantity\": 1,\n        \"description\": \"Producto de prueba\",\n        \"taxes\": [\n          {\n            \"type\": \"01\",\n            \"amount\":0.035,\n            \"code\": \"01\"\n          }\n        ],\n        \"discount\": 0.0,\n        \"internal_code\": \"123123\"\n      }\n    ],\n    \"payments\": [\n      {\n        \"type\": \"99\",\n        \"amount\": \"0.54\",\n        \"description\": \"Medio de pago de prueba\"\n      }\n    ],\n    \"type\": \"01\",\n    \"info\": \"<string>\"\n  }\n}");
    Request request = new Request.Builder()
      .url("https://backend-qa-api.facturafacil.com.pa/api/pac/reception_fe/detailed/")
      .method("POST", body)
      .addHeader("X-FF-Company", "[UUID DE FF]")
      .addHeader("X-FF-API-Key", "[KEY DE FF]")
      .addHeader("X-FF-Branch", "[BRANCH DE FF]")
      .addHeader("Content-Type", "application/json")
      .addHeader("Accept", "application/json")
      .build();
    Response response = client.newCall(request).execute();
    System.out.println(response.body().string());
  }
}

