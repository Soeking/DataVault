module DataVault.external.StockData

open System
open System.Net.Http
open System.Text.Json

type StockDailyData = { Date: DateTime; Value: Decimal }

type ResponseError =
    | InvalidApiKey
    | NetworkError of String
    | ParseError of String

let getStockData (seriesId: string) (apiKey: string) (startDate: DateTime) (endDate: DateTime) =
    async {
        use client = new HttpClient()

        // FREDのSeries
        let url =
            sprintf
                "https://api.stlouisfed.org/fred/series/observations?series_id=%s&api_key=%s&file_type=json&observation_start=%s&observation_end=%s"
                seriesId
                apiKey
                (startDate.ToString("yyyy-MM-dd"))
                (endDate.ToString("yyyy-MM-dd"))

        try
            let! response = client.GetStringAsync(url) |> Async.AwaitTask
            let document = JsonDocument.Parse(response)

            return
                Ok(
                    document.RootElement.GetProperty("observations").EnumerateArray()
                    |> Seq.map (fun daily ->
                        let date = DateTime.Parse(daily.GetProperty("date").GetString())
                        let valueStr = daily.GetProperty("value").GetString()

                        match Decimal.TryParse(valueStr) with
                        | true, value -> Some { Date = date; Value = value }
                        | false, _ -> None)
                    |> Seq.choose id
                    |> Seq.toList
                )
        with
        | :? HttpRequestException as ex -> return Error(NetworkError ex.Message)
        | ex -> return Error(ParseError ex.Message)
    }

let apiKey = Environment.GetEnvironmentVariable("FRED_TOKEN")

let getNikkei225Data startDate endDate =
    let seriesId = "NIKKEI225"

    getStockData seriesId apiKey startDate endDate

let getSP500Data startDate endDate =
    let seriesId = "SP500"

    getStockData seriesId apiKey startDate endDate
