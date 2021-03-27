// Run this script within FSI to hit the server

#r "nuget: Azure.Identity"
#r "nuget: Microsoft.Identity.Client"
#r "System.Net.Http"

open System
open Microsoft.Identity.Client
open System.Security.Cryptography.X509Certificates
open Azure.Identity
open System.Net.Http

let clientId = "41590763-ec7a-4376-906f-cee138711384" // MultiTenantTestClient, but note that this client itself is not multi-tenant
let clientTenantId = "4510468d-3790-4a1a-8209-84281b2d1596" // wilson.max default directory. Note: different from the server tenant!
let clientSecret = "<redacted>"

let cred = new ClientCertificateCredential(clientTenantId, clientId, clientSecret)
let client =
    let options = ConfidentialClientApplicationOptions(TenantId=clientTenantId, ClientId=clientId, ClientSecret=clientSecret)
    ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(options)
        .Build()
let token = client.AcquireTokenForClient(["https://wilsonsoft.onmicrosoft.com/HelloWeather/.default"]).ExecuteAsync().Result.AccessToken

let http = new HttpClient()
http.DefaultRequestHeaders.Authorization <- Headers.AuthenticationHeaderValue("bearer", token)
let showResponse (msg: HttpResponseMessage) =
    printfn "Response: %A" msg
    msg.Content.ReadAsStringAsync().Result |> printfn "%s"
http.GetAsync("https://localhost:44336/weatherforecast/who").Result |> showResponse

// see https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Client-credential-flows
