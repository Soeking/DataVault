module DataBase

open System.Threading
open InfluxDB3.Client
open InfluxDB3.Client.Write
open System
open StockData

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
    task {
        let point = PointData.Measurement(tableName).SetTimestamp(date)

        // フィールドの追加
        for KeyValue(key, value) in fields do
            point.SetField(key, value) |> ignore

        // タグの追加
        for KeyValue(key, value) in tags do
            point.SetTag(key, value) |> ignore

        do!
            client.WritePointAsync(
                point,
                cancellationToken = (new CancellationTokenSource(TimeSpan.FromSeconds(30.0))).Token
            )
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

        writeStockData client "stock" fields tags st.Date
        |> Async.AwaitTask
        |> fun t -> Async.RunSynchronously t)
