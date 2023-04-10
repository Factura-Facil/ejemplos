#include <stdio.h>
#include <string.h>
#include <curl/curl.h>
int main(int argc, char *argv[]){
  CURL *curl;
  CURLcode res;
  curl = curl_easy_init();
  if(curl) {
    curl_easy_setopt(curl, CURLOPT_CUSTOMREQUEST, "POST");
    curl_easy_setopt(curl, CURLOPT_URL, "https://backend-qa-api.facturafacil.com.pa/api/pac/reception_fe/detailed/");
    curl_easy_setopt(curl, CURLOPT_FOLLOWLOCATION, 1L);
    curl_easy_setopt(curl, CURLOPT_DEFAULT_PROTOCOL, "https");
    struct curl_slist *headers = NULL;
    headers = curl_slist_append(headers, "X-FF-Company: [UUID DE FF]");
    headers = curl_slist_append(headers, "X-FF-API-Key: [KEY DE FF]");
    headers = curl_slist_append(headers, "Content-Type: application/json");
    headers = curl_slist_append(headers, "Accept: application/json");
    curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
    const char *data = "{\n  \"header\": {\n    \"id\": 1,\n    \"environment\": \"2\"\n  },\n  \"document\": {\n    \"fd_number\": \"12\",\n    \"receptor\": {\n      \"type\": \"02\",\n      \"name\": \"Nombre Cliente\",\n      \"ruc_type\": \"1\",\n      \"address\": \"Dirección Cliente\",\n      \"email\": \"cliente@correo.com\",\n      \"ruc\": \"123123123\"\n    },\n    \"items\": [\n      {\n        \"line\": 1,\n        \"price\": 0.50,\n        \"mu\": \"und\",\n        \"quantity\": 1,\n        \"description\": \"Producto de prueba\",\n        \"taxes\": [\n          {\n            \"type\": \"01\",\n            \"amount\":0.035,\n            \"code\": \"01\"\n          }\n        ],\n        \"discount\": 0.0,\n        \"internal_code\": \"123123\"\n      }\n    ],\n    \"payments\": [\n      {\n        \"type\": \"99\",\n        \"amount\": \"0.54\",\n        \"description\": \"Medio de pago de prueba\"\n      }\n    ],\n    \"type\": \"01\",\n    \"info\": \"<string>\"\n  }\n}";
    curl_easy_setopt(curl, CURLOPT_POSTFIELDS, data);
    res = curl_easy_perform(curl);
  }
  curl_easy_cleanup(curl);
  return (int)res;
}
