namespace BackEnd

open Saturn
open System.Data
open FSharp.Data.Sql
open Npgsql.FSharp
open FSharpPlus
open Shared.Utils

module Db =

    let TPConnectionString =
        "Server=127.0.0.1;"+
        "Database=todo;" +
        "User Id=todo;"+
        "Password=todo;"

    [<Literal>]
    let snapshotInterval = 10

    let ceResult = CeResultBuilder()

    let tryGetLastSnapshot()  =
        TPConnectionString
        |> Sql.connect
        |> Sql.query "SELECT event_id, snapshot FROM snapshots ORDER BY id DESC LIMIT 1"
        |> Sql.execute (fun read ->
            (
                read.int "event_id",
                read.text "snapshot"
            )
        )
        |> Seq.tryHead

    let getLastEventId() =
        TPConnectionString
        |> Sql.connect
        |> Sql.query "SELECT id from events ORDER BY id DESC LIMIT 1"
        |> Sql.execute  (fun read -> read.int "id")
        |> Seq.tryHead

    let getEvent id  =
        TPConnectionString
        |> Sql.connect
        |> Sql.query "SELECT * from events where id = @id"
        |> Sql.parameters ["id", Sql.int id]
        |> Sql.execute
            (
                fun read ->
                {|
                    Id = read.int "id"
                    Event = read.string "event"
                    Timestamp = read.dateTime "timestamp"
                |}
            ) |> Seq.tryHead

    let setSnapshot id snapshot =
        ceResult
            {
                let! event = getEvent id |> optionToResult
                let _ =
                    TPConnectionString
                    |> Sql.connect
                    |> Sql.executeTransaction
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
                return ()
            }

    let addEvents (events: List<string>)  =
        try
            TPConnectionString
            |> Sql.connect
            |> Sql.executeTransaction
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
                |> Ok
        with
            | _ as ex -> (ex.ToString()) |> Error

    let getEventsAfterId id =
        TPConnectionString
        |> Sql.connect
        |> Sql.query "SELECT id, event FROM events WHERE id > @id ORDER BY id"
        |> Sql.parameters ["id", Sql.int id]
        |> Sql.execute ( fun read ->
            (
                read.int "id",
                read.text "event"
            )
        ) |> Seq.toList
