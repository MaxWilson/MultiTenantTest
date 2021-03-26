// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open Farmer
open Farmer.Builders
open Farmer.WebApp

let svc = webApp {
    name "helloFarmer"
    service_plan_name "helloFarmerPlan"
    setting "myKey" "possibly unneeded?"
    sku WebApp.Sku.B1
    enable_cors WebApp.AllOrigins
    zip_deploy @".\publish"
}

let deployment = arm {
    location Location.WestUS2
    add_resource svc
}

[<EntryPoint>]
let main argv =
    //deployment |> Writer.quickWrite "weather"
    deployment |> Deploy.execute "helloFarmerRG" Deploy.NoParameters |> ignore
    0 // return an integer exit code