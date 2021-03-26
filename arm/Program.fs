// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open Farmer
open Farmer.Builders

let helloWorld = functions {
    name "helloWorld"
    storage_account_name "helloWorldData"    
}

// Define a function to construct a message to print
let from whom =
    sprintf "from %s" whom

[<EntryPoint>]
let main argv =
    let message = from "F#" // Call the function
    printfn "Hello world %s" message
    0 // return an integer exit code