module DataVault.db.InfluxDB

open System.Linq
open System.Threading
open System.Threading.Tasks
open InfluxDB3.Client
open InfluxDB3.Client.Write
open System
open DataVault.external.StockData

type DatabaseConfig =
    { Host: string
      Database: string
      Token: string }

let createClient (config: DatabaseConfig) =
    new InfluxDBClient(host = config.Host, database = config.Database, token = config.Token)

let writeStockData
    (client: InfluxDBClient)
    tableName
    (fields: Map<String, Decimal>)
    (tags: Map<String, String>)
    (date: DateTime)
    =
    async {
        let point = PointData.Measurement(tableName).SetTimestamp(date)

        // フィールドの追加
        for KeyValue(key, value) in fields do
            point.SetField(key, value) |> ignore

        // タグの追加
        for KeyValue(key, value) in tags do
            point.SetTag(key, value) |> ignore

        client.WritePointAsync(
            point,
            cancellationToken = (new CancellationTokenSource(TimeSpan.FromSeconds(30.0))).Token
        )
        |> ignore
    }

let queryData (client: InfluxDBClient) query =
    task {
        let enumerable = client.QueryPoints(query)
        return enumerable
    }

let config =
    { Host =
        Environment.GetEnvironmentVariable("INFLUXDB3_HOST")
        + ":"
        + Environment.GetEnvironmentVariable("INFLUXDB3_PORT")
      Database = "DataVault"
      Token = Environment.GetEnvironmentVariable("INFLUXDB3_AUTH_TOKEN") }

let client = createClient config

let writeStockDataList (stockList: StockDailyData list) seriesName =
    stockList
    |> List.iter (fun st ->
        let fields = Map [ "value", st.Value ]
        let tags = Map [ "series", seriesName ]

        writeStockData client "stock" fields tags st.Date |> Async.RunSynchronously)

let readTable seriesName =
    task {
        let list = ResizeArray<StockDailyData>()

        let query =
            $"SELECT value FROM stock WHERE series = %s{seriesName} ORDER BY time ASC"

        let! result = queryData client query

        let enum =
            result.GetAsyncEnumerator((new CancellationTokenSource(TimeSpan.FromSeconds(20.0))).Token)

        while! enum.MoveNextAsync() do
            let timestamp =
                enum.Current.GetTimestamp()
                |> Nullable.op_Explicit
                |> int64
                |> fun x -> x / 1_000_000L |> DateTimeOffset.FromUnixTimeMilliseconds

            let value = enum.Current.GetDoubleField("value") |> Nullable.op_Explicit |> decimal

            list.Add({ Date = timestamp.Date; Value = value })

        return list.ToArray() |> List.ofArray
    }
