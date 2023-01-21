
namespace BackEnd

open Shared
open FSharpPlus
open System

module Todos =
    open Utils
    type Todos =
        {
            todos: List<Todo>
        }
        with
            static member Zero =
                {
                    todos = []
                }
            interface ITodo with
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
