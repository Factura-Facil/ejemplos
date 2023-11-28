using System;
using RestSharp;
using System.Threading;
using System.Threading.Tasks;
namespace HelloWorldApplication {
  class HelloWorld {
    static async Task Main(string[] args) {
      var options = new RestClientOptions("https://backend-qa-api.facturafacil.com.pa/api/pac/reception_fe/detailed/")
      {
        MaxTimeout = -1,
      };
      var client = new RestClient(options);
      var request = new RestRequest("", Method.Post);
      request.AddHeader("X-FF-Company", "[UUID DE FF]");
      request.AddHeader("X-FF-API-Key", "[KEY DE FF]");
      request.AddHeader("X-FF-Branch", "[UUID BRANCH DE FF]");
      request.AddHeader("Content-Type", "application/json");
      request.AddHeader("Accept", "application/json");
      var body = @"{" + "\n" +
      @"  ""header"": {" + "\n" +
      @"    ""id"": 1," + "\n" +
      @"    ""environment"": ""2""" + "\n" +
      @"  }," + "\n" +
      @"  ""document"": {" + "\n" +
      @"    ""fd_number"": ""12""," + "\n" +
      @"    ""receptor"": {" + "\n" +
      @"      ""type"": ""02""," + "\n" +
      @"      ""name"": ""Nombre Cliente""," + "\n" +
      @"      ""ruc_type"": ""1""," + "\n" +
      @"      ""address"": ""Dirección Cliente""," + "\n" +
      @"      ""email"": ""cliente@correo.com""," + "\n" +
      @"      ""ruc"": ""123123123""" + "\n" +
      @"    }," + "\n" +
      @"    ""items"": [" + "\n" +
      @"      {" + "\n" +
      @"        ""line"": 1," + "\n" +
      @"        ""price"": 0.50," + "\n" +
      @"        ""mu"": ""und""," + "\n" +
      @"        ""quantity"": 1," + "\n" +
      @"        ""description"": ""Producto de prueba""," + "\n" +
      @"        ""taxes"": [" + "\n" +
      @"          {" + "\n" +
      @"            ""type"": ""01""," + "\n" +
      @"            ""amount"":0.035," + "\n" +
      @"            ""code"": ""01""" + "\n" +
      @"          }" + "\n" +
      @"        ]," + "\n" +
      @"        ""discount"": 0.0," + "\n" +
      @"        ""internal_code"": ""123123""" + "\n" +
      @"      }" + "\n" +
      @"    ]," + "\n" +
      @"    ""payments"": [" + "\n" +
      @"      {" + "\n" +
      @"        ""type"": ""99""," + "\n" +
      @"        ""amount"": ""0.54""," + "\n" +
      @"        ""description"": ""Medio de pago de prueba""" + "\n" +
      @"      }" + "\n" +
      @"    ]," + "\n" +
      @"    ""type"": ""01""," + "\n" +
      @"    ""info"": ""<string>""" + "\n" +
      @"  }" + "\n" +
      @"}";
      request.AddStringBody(body, DataFormat.Json);
      RestResponse response = await client.ExecuteAsync(request);
      Console.WriteLine(response.Content);
    }
  }
}

