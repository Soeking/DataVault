module DataVault.front.FileDownload

open System.IO
open DataVault.db.InfluxDB
open DataVault.front.LayoutPage
open Giraffe
open Giraffe.ViewEngine
open Microsoft.AspNetCore.Http

let filePage =
    [ p [] [ a [ _href "/file/sample"; _download "sample.txt" ] [ str "file download" ] ] ]
    |> masterPage "File"

let dynamicDownloadHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let reportContent = $"This report was generated at {System.DateTime.Now}"
            let tempFilePath = Path.GetTempFileName()
            do! File.WriteAllTextAsync(tempFilePath, reportContent)
            let stream = File.Open(tempFilePath, FileMode.Open)

            return! ctx.WriteFileStreamAsync(false, tempFilePath, None, None)
        // streamData false stream None None
        }

let generateCSVText seriesName =
    task{
        let! dailyDatas = readTable seriesName
        dailyDatas |> Seq.map (fun x -> $"{x.Value},{x.Date}")
    }
