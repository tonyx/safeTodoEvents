namespace Shared
open System
open FSharp.Core
open FSharpPlus
open FSharpPlus.Data

open Utils
open EventSourcing
open Todos

module Commands =
    type Command =
        | AddTodo of Todo
        | RemoveTodo of Guid

        interface Executable<Todos,Event> with
            member this.Execute (x: Todos) =
                match this with
                | AddTodo t ->
                    match x.AddTodo t with
                        | Ok _ -> [Event.TodoAdded t] |> Ok
                        | Error x -> x |> Error
                | RemoveTodo g ->
                    match x.RemoveTodo g with
                        | Ok _ -> [Event.TodoRemoved g] |> Ok
                        | Error x -> x |> Error

        static member Executes (l: List<Executable<Todos, Event>>) (t: Todos) =
            executes l t

