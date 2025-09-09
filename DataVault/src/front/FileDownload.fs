module DataVault.front.FileDownload

open System.IO
open DataVault.db.InfluxDB
open DataVault.front.LayoutPage
open Giraffe
open Giraffe.ViewEngine
open Microsoft.AspNetCore.Http

let filePage =
    [ p [] [ a [ _href "/file/Nikkei225"; _download "Nikkei225.csv" ] [ str "Nikkei225 file download" ] ]
      p [] [ a [ _href "/file/SP500"; _download "SP500.csv" ] [ str "SP500 file download" ] ] ]
    |> masterPage "File"

let generateCSVStream seriesName =
    task {
        let tempFilePath = Path.GetTempFileName()
        use writer = new StreamWriter(tempFilePath)
        let! dailyDatas = readTable seriesName

        dailyDatas
        |> Seq.map (fun x -> $"{x.Value},{x.Date}")
        |> Seq.iter writer.WriteLine
        writer.Dispose()

        // return writer.BaseStream
        return File.Open(tempFilePath, FileMode.Open) |> Stream.Synchronized
    }

let dynamicDownloadHandler seriesName : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! stream = generateCSVStream seriesName
            return! ctx.WriteStreamAsync(false, stream, None, None)
        }
