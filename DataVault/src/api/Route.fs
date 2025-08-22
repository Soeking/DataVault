module DataVault.api.Route

open System
open System.Threading.Tasks
open DataVault.api.PostFunc
open DataVault.front.Index
open DataVault.front.Login
open DataVault.front.Register
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.FSharp.Core

let routing (app: WebApplication) =
    app.MapGet("/", Func<String>(fun () -> "welcome")) |> ignore

    // 株情報を登録
    app.MapPost(
        "/stock/insert",
        Func<HttpContext, Task<Unit>>(fun ctx ->
            let startDate = ctx.Request.Headers.["start"].ToString()
            let endDate = ctx.Request.Headers.["end"].ToString()
            insertStockData startDate endDate)
    )
    |> ignore

let mustBeLoggedIn: HttpHandler = requiresAuthentication (redirectTo false "/login")

let webRouting: HttpFunc -> HttpContext -> HttpFuncResult =
    choose
        [ GET
          >=> choose
                  [ route "/" >=> mustBeLoggedIn >=> htmlView indexPage
                    route "/register" >=> htmlView registerPage
                    route "/login" >=> htmlView (loginPage false)
                    route "/logout" >=> mustBeLoggedIn >=> logoutHandler ]
          POST
          >=> choose
                  [ route "/register" >=> registerHandler
                    route "/login" >=> loginHandler
                    route "/stock/insert" >=> mustBeLoggedIn >=> insertStockHandler ]
          setStatusCode 404 >=> text "Not Found" ]
