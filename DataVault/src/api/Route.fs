module DataVault.api.Route

open System
open System.Threading.Tasks
open DataVault.db.InfluxDB
open DataVault.front.Index
open DataVault.front.Login
open DataVault.front.Register
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.FSharp.Core
open DataVault.external.StockData

let insertStockData (startDateText: String) (endDateText: String) =
    task {
        let mutable startDateTime = Unchecked.defaultof<DateTime>
        let mutable endDateTime = Unchecked.defaultof<DateTime>
        let startParse = DateTime.TryParse(startDateText, &startDateTime)
        let endParse = DateTime.TryParse(endDateText, &endDateTime)

        if startParse && endParse then
            getNikkei225Data startDateTime endDateTime
            |> Async.RunSynchronously
            |> (fun result ->
                match result with
                | Ok list -> writeStockDataList list "Nikkei225"
                | Error er -> printfn $"%A{er}")

            getSP500Data startDateTime endDateTime
            |> Async.RunSynchronously
            |> (fun result ->
                match result with
                | Ok list -> writeStockDataList list "SP500"
                | Error er -> printfn $"%A{er}")
        else
            printfn $"datetime parse error start:%s{startDateText} end:%s{endDateText}"
    }

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
          >=> choose [ route "/register" >=> registerHandler; route "/login" >=> loginHandler ]
          setStatusCode 404 >=> text "Not Found" ]
