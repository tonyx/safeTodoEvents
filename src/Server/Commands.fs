namespace BackEnd

open Shared
open System
open EventSourcing
open BackEnd.Events
open BackEnd.Aggregate
open BackEnd.Cache

module Commands =
type Command =
    | AddTodo of Todo
    | RemoveTodo of Guid

    interface Executable<Aggregate, Event> with
        member this.Execute (x: Aggregate) =
            match this with
            | AddTodo t ->
                match
                    EventCache<Aggregate>.Instance.Memoize (fun () -> x.AddTodo t) (x, [TodoAdded t]) with
                    | Ok _ -> [TodoAdded t] |> Ok
                    | Error x -> x |> Error
            | RemoveTodo g ->
                match
                    EventCache<Aggregate>.Instance.Memoize (fun () -> x.RemoveTodo g) (x, [TodoRemoved g]) with
                    | Ok _ -> [TodoRemoved g] |> Ok
                    | Error x -> x |> Error

