namespace Shared
open System
open FSharp.Core
open EventSourcing
open Todos

module Commands =
    type Command =
        | AddTodo of Todo
        | RemoveTodo of Guid

        interface Executable<Todos, Event> with
            member this.Execute (x: Todos) =
                match this with
                | AddTodo t ->
                    match
                        Cache.memoize (fun x -> x.AddTodo t) (x, Event.TodoAdded t) with
                        | Ok _ -> [Event.TodoAdded t] |> Ok
                        | Error x -> x |> Error
                | RemoveTodo g ->
                    match
                        Cache.memoize (fun x -> x.RemoveTodo g) (x, Event.TodoRemoved g) with
                        | Ok _ -> [Event.TodoRemoved g] |> Ok
                        | Error x -> x |> Error

        static member Executes (l: List<Executable<Todos, Event>>) (t: Todos) =
            executes l t

