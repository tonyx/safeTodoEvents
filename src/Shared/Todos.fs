namespace Shared
open System
open FSharp.Core

open EventSourcing

open FSharpPlus
open FSharpPlus.Data

module rec Todos =
    open Shared
    open Utils

    type Todos =
        {
            todos: List<Todo>
        }
        with
            interface Root<Todos, Todos.Event> with
                member this.Evolve (events: list<Todos.Event>) =
                    events |> evolve this

            static member Zero =
                {
                    todos = []
                }
            member this.AddTodo (t: Todo) =
                ceResult {
                    let! mustNotExist =
                        this.todos
                        |> List.exists (fun x -> x.Description = t.Description)
                        |> not
                        |> boolToResult
                    let result =
                        {
                            this with
                                todos = t::this.todos
                        }
                    return result
                }

            member this.RemoveTodo (id: Guid) =
                ceResult {
                    let! mustExist =
                        this.todos
                        |> List.exists (fun x -> x.Id = id)
                        |> boolToResult
                    let result =
                        {
                            this with
                                todos = this.todos |> List.filter (fun x -> x.Id <> id)
                        }
                    return result
                }
            member this.GetTodos() = this.todos

    type Event =
        | TodoAdded of Todo
        | TodoRemoved of Guid
            interface Processable<Todos> with
                member this.Process (x: Todos) =
                    match this with
                    | TodoAdded (t: Todo) -> x.AddTodo t
                    | TodoRemoved (g: Guid) -> x.RemoveTodo g


