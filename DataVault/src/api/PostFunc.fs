module DataVault.api.PostFunc

open System
open DataVault.db.InfluxDB
open DataVault.external.StockData
open Giraffe
open Microsoft.AspNetCore.Http


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
    
let insertStockHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let startDate = ctx.Request.Headers.["start"].ToString()
            let endDate = ctx.Request.Headers.["end"].ToString()
            insertStockData startDate endDate |> ignore
             
            return! Successful.NO_CONTENT next ctx
        }