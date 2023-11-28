Imports System
Imports System.Net.Http

Module Program
    Sub Main(args As String())
        Console.WriteLine("Hello World!")
        Dim task = New Task(AddressOf TestRequest)
        task.Start()
        task.Wait()
        Console.ReadLine()
    End Sub


    Private Async Sub TestRequest()
        Dim client = New HttpClient()
        Dim request = New HttpRequestMessage(HttpMethod.Post, "https://backend-qa-api.facturafacil.com.pa/api/pac/reception_fe/detailed/")
        request.Headers.Add("X-FF-Company", "[UUID DE FF]")
        request.Headers.Add("X-FF-API-Key", "[KEY DE FF]")
        request.Headers.Add("X-FF-Branch", "[BRANCH DE FF]")
        request.Headers.Add("Accept", "application/json")

        'Para ejemplos de generación de documentos JSON recomendamos la libreria http://www.newtonsoft.com/json/help/html/SerializingJSON.htm
        Dim content = New StringContent("{""header"":{""id"":1,""environment"":""2""},""document"":{""fd_number"":""12"",""receptor"":{""type"":""02"",""name"":""Nombre Cliente"",""ruc_type"":""1"",""address"":""Dirección Cliente"",""email"":""cliente@correo.com"",""ruc"":""123123123""},""items"":[{""line"":1,""price"":0.5,""mu"":""und"",""quantity"":1,""description"":""Producto de prueba"",""taxes"":[{""type"":""01"",""amount"":0.035,""code"":""01""}],""discount"":0,""internal_code"":""123123""}],""payments"":[{""type"":""99"",""amount"":""0.54"",""description"":""Medio de pago de prueba""}],""type"":""01"",""info"":""<string>""}}", Nothing, "application/json")
        request.Content = content
        Dim response = Await client.SendAsync(request)
        Console.WriteLine(Await response.Content.ReadAsStringAsync())
    End Sub

End Module
