module Program

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.FSharp.Core
open Route

let exitCode = 0

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    builder.Services.AddControllers() |> ignore

    let app = builder.Build()
    app.UseAuthorization() |> ignore
    routing app
    app.Run()

    exitCode
