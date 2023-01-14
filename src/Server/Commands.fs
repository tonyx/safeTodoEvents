namespace BackEnd

open Shared
open System

module MetaCommands =
    open Todos
    open MetaEvents
    open EventSourcing
    type Command =
        | AddTodo of Todo
        | RemoveTodo of Guid

        interface Executable<Aggregate.Aggregate, Event> with
            member this.Execute (x: Aggregate.Aggregate) =
                match this with
                | AddTodo t ->
                    match
                        x.AddTodo t with
                        | Ok _ -> [Event.TodoAdded t] |> Ok
                        | Error x -> x |> Error
                | RemoveTodo g ->
                    match
                        x.RemoveTodo g with
                        | Ok _ -> [Event.TodoRemoved g] |> Ok
                        | Error x -> x |> Error

