namespace app.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open app
open System.Net
open Microsoft.AspNetCore.Authorization

[<ApiController>]
[<Route("[controller]")>]
type WeatherForecastController (logger : ILogger<WeatherForecastController>) =
    inherit ControllerBase()

    let summaries =
        [|
            "Freezing"
            "Bracing"
            "Chilly"
            "Cool"
            "Mild"
            "Warm"
            "Balmy"
            "Hot"
            "Sweltering"
            "Scorching"
        |]

    [<HttpGet>]
    member _.Get() =
        let rng = System.Random()
        [|
            for index in 0..4 ->
                { Date = DateTime.Now.AddDays(float index)
                  TemperatureC = rng.Next(-20,55)
                  Summary = summaries.[rng.Next(summaries.Length)] }
        |]
    [<HttpGet>]
    [<Route("Who")>]
    [<Authorize(Policy="ACL")>]
    member this.Who() =
        this.Ok(
            {|
                isAuthenticated = this.User.Identity.IsAuthenticated
                claims =
                    match this.User.Identity with
                        | :? System.Security.Claims.ClaimsIdentity as id ->
                            id.Claims |> Seq.map(fun c -> c.Type, c.Value)
                        | _ -> null
            |})
