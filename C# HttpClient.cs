// No more boilerplate needed with top level statements (https://docs.microsoft.com/en-us/dotnet/core/tutorials/top-level-templates)
var client = new HttpClient();
var request = new HttpRequestMessage(HttpMethod.Post, "https://backend-qa-api.facturafacil.com.pa/api/pac/reception_fe/detailed/");
request.Headers.Add("X-FF-Company", "[UUID EMISOR DE FF]");
request.Headers.Add("X-FF-API-Key", "[KEY DE FF]");
request.Headers.Add("X-FF-Branch", "[UUID BRANCH DE FF]");
request.Headers.Add("Accept", "application/json");
var content = new StringContent("{\n  \"header\": {\n    \"id\": 1,\n    \"environment\": \"2\"\n  },\n  \"document\": {\n    \"fd_number\": \"12\",\n    \"receptor\": {\n      \"type\": \"02\",\n      \"name\": \"Nombre Cliente\",\n      \"ruc_type\": \"1\",\n      \"address\": \"Dirección Cliente\",\n      \"email\": \"cliente@correo.com\",\n      \"ruc\": \"123123123\"\n    },\n    \"items\": [\n      {\n        \"line\": 1,\n        \"price\": 0.50,\n        \"mu\": \"und\",\n        \"quantity\": 1,\n        \"description\": \"Producto de prueba\",\n        \"taxes\": [\n          {\n            \"type\": \"01\",\n            \"amount\":0.035,\n            \"code\": \"01\"\n          }\n        ],\n        \"discount\": 0.0,\n        \"internal_code\": \"123123\"\n      }\n    ],\n    \"payments\": [\n      {\n        \"type\": \"99\",\n        \"amount\": \"0.54\",\n        \"description\": \"Medio de pago de prueba\"\n      }\n    ],\n    \"type\": \"01\",\n    \"info\": \"<string>\"\n  }\n}", null, "application/json");
request.Content = content;
var response = await client.SendAsync(request);
response.EnsureSuccessStatusCode();
Console.WriteLine(await response.Content.ReadAsStringAsync());

