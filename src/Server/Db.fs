namespace BackEnd

open FSharp.Data.Sql
open Npgsql.FSharp
open FSharpPlus
open Shared
open Shared.Utils

module Db =

    let TPConnectionString = Conf.connectionString

    [<Literal>]
    let snapshotInterval = 10

    let ceResult = CeResultBuilder()

    let deleteAllevents () =
        if (Conf.isTestEnv) then
            let _ =
                TPConnectionString
                |> Sql.connect
                |> Sql.query "DELETE from snapshots"
                |> Sql.executeNonQuery
            let _ =
                TPConnectionString
                |> Sql.connect
                |> Sql.query "DELETE from events"
                |> Sql.executeNonQuery
            ()
        else
            failwith "operation allowed only in test db"

    let tryGetLastSnapshot()  =
        TPConnectionString
        |> Sql.connect
        |> Sql.query "SELECT event_id, snapshot FROM snapshots ORDER BY id DESC LIMIT 1"
        |> Sql.executeAsync (fun read ->
            (
                read.int "event_id",
                read.text "snapshot"
            )
        )
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> Seq.tryHead

    let getLastEventId() =
        TPConnectionString
        |> Sql.connect
        |> Sql.query "SELECT id from events ORDER BY id DESC LIMIT 1"
        |> Sql.executeAsync  (fun read -> read.int "id")
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> Seq.tryHead

    let getEvent id  =
        TPConnectionString
        |> Sql.connect
        |> Sql.query "SELECT * from events where id = @id"
        |> Sql.parameters ["id", Sql.int id]
        |> Sql.executeAsync
            (
                fun read ->
                {|
                    Id = read.int "id"
                    Event = read.string "event"
                    Timestamp = read.dateTime "timestamp"
                |}
            )
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> Seq.tryHead

    let setSnapshot id snapshot =
        ceResult
            {
                let! event = getEvent id |> optionToResult
                let _ =
                    TPConnectionString
                    |> Sql.connect
                    |> Sql.executeTransactionAsync
                        [
                            "INSERT INTO snapshots (event_id, snapshot, timestamp) VALUES (@event_id, @snapshot, @timestamp)",
                                [
                                    [
                                        ("@event_id", Sql.int event.Id);
                                        ("snapshot",  Sql.jsonb snapshot);
                                        ("timestamp", Sql.timestamp event.Timestamp)
                                    ]
                                ]
                        ]
                    |> Async.AwaitTask
                    |> Async.RunSynchronously
                return ()
            }

    let addEvents (events: List<string>)  =
        try
            TPConnectionString
            |> Sql.connect
            |> Sql.executeTransactionAsync
                [
                    "INSERT INTO events (event, timestamp) VALUES (@event, @timestamp)",
                    events
                    |> List.map
                        (
                            fun x ->
                                [
                                    ("@event", Sql.jsonb x);
                                    ("timestamp", Sql.timestamp (System.DateTime.Now))
                                ]
                        )
                ]
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> Ok
        with
            | _ as ex -> (ex.ToString()) |> Error

    let getEventsAfterId id =
        TPConnectionString
        |> Sql.connect
        |> Sql.query "SELECT id, event FROM events WHERE id > @id ORDER BY id"
        |> Sql.parameters ["id", Sql.int id]
        |> Sql.executeAsync ( fun read ->
            (
                read.int "id",
                read.text "event"
            )
        )
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> Seq.toList
