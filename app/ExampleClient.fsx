// Run this script within FSI to hit the server

#r "nuget: Azure.Identity"
#r "nuget: Microsoft.Identity.Client"
#r "System.Net.Http"

open System
open Microsoft.Identity.Client
open System.Security.Cryptography.X509Certificates
open Azure.Identity
open System.Net.Http

// Note that this client itself is not multi-tenant, and it lives in 4510468d-3790-4a1a-8209-84281b2d1596,
// but it talks to a multi-tenant app in tenant 88ac0449-3fae-4113-a1ba-fb4f2d041702
let clientId = "083d3ba2-ed4e-4e11-b7ef-d8cc46ffe346" // HelloWeatherFromForeignTenant.
let clientTenantId = "4510468d-3790-4a1a-8209-84281b2d1596" // since this is a cross-tenant scenario, use common authority
let clientSecret = "<redacted>" // see: https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/Credentials/appId/41590763-ec7a-4376-906f-cee138711384/isMSAApp/

// No admin consent or manifest changes or API permissions are needed, but the service does need to add this app to the ACL.
// Otherwise, you'll be able to get a token for the API, but you won't be authorized to use any special protected resources.

let http = new HttpClient()
let showResponse (msg: HttpResponseMessage) =
    printfn "Response: %A" msg
    msg.Content.ReadAsStringAsync().Result |> printfn "%s"

let cred = new ClientCertificateCredential(clientTenantId, clientId, clientSecret)
let client =
    let options = ConfidentialClientApplicationOptions(TenantId=clientTenantId, ClientId=clientId, ClientSecret=clientSecret)
    ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(options)
        .Build()
let token = client.AcquireTokenForClient(["https://wilsonsoft.onmicrosoft.com/HelloWeather/.default"]).ExecuteAsync().Result.AccessToken

http.DefaultRequestHeaders.Authorization <- Headers.AuthenticationHeaderValue("bearer", token)
http.GetAsync("https://localhost:44336/weatherforecast/who").Result |> showResponse

