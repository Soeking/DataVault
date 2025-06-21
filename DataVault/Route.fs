module Route

open System
open System.Threading.Tasks
open DataBase
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.FSharp.Core
open StockData

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
