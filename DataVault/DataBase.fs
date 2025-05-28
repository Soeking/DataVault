module DataBase

open System.Threading
open InfluxDB3.Client
open InfluxDB3.Client.Write
open System

type DatabaseConfig =
    { Host: string
      Database: string
      Token: string }

let createClient (config: DatabaseConfig) =
    new InfluxDBClient(host = config.Host, database = config.Database, token = config.Token)

let writePoint (client: InfluxDBClient) tableName (fields: Map<string, obj>) (tags: Map<string, string>) =
    task {
        let point = PointData.Measurement(tableName).SetTimestamp(DateTime.UtcNow)

        // フィールドの追加
        for KeyValue(key, value) in fields do
            match value with
            | :? int as v -> point.SetField(key, v) |> ignore
            | :? float as v -> point.SetField(key, v) |> ignore
            | :? string as v -> point.SetField(key, v) |> ignore
            | :? bool as v -> point.SetField(key, v) |> ignore
            | _ -> ()

        // タグの追加
        for KeyValue(key, value) in tags do
            point.SetTag(key, value) |> ignore

        do!
            client.WritePointAsync(
                point,
                cancellationToken = (new CancellationTokenSource(TimeSpan.FromSeconds(4.0))).Token
            )
    }

let queryData (client: InfluxDBClient) query =
    task {
        let enumerable = client.QueryPoints(query)
        return enumerable
    }

let config =
    { Host = Environment.GetEnvironmentVariable("INFLUXDB3_PORT")
      Database = "DataVault"
      Token = Environment.GetEnvironmentVariable("INFLUXDB3_AUTH_TOKEN") }

let client = createClient config

let writeStockData (timeStamp: DateTime) =
    task {
        let fields = Map [ "s", 36.5 :> obj; "e", 45 :> obj ]
        let tags = Map [ "region", ""; "com", "" ]

        writePoint client "Stock" fields tags
        |> Async.AwaitTask
        |> Async.RunSynchronously
    }
