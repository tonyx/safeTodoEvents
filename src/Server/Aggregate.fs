namespace BackEnd

open Shared
open FSharpPlus
open System
open Todos
open Shared.Utils

module Aggregate =
    type Aggregate =
        {
            todos: ITodo
            projection: ITodo
        }
        with
            static member Zero =
                {
                    todos = Todos.Zero
                    projection = Projection.Zero
                }
            member this.AddTodo (t: Todo) =
                ceResult
                    {
                        let! todos = this.todos.AddTodo t
                        let! projection = this.projection.AddTodo t
                        return
                            {
                                this with
                                    todos = todos
                                    projection = projection
                            }
                    }
            member this.RemoveTodo (id: Guid) =
                ceResult
                    {
                        let! todos = this.todos.RemoveTodo id
                        let! projection = this.projection.RemoveTodo id
                        return
                            {
                                this with
                                    todos = todos
                                    projection = projection
                            }
                    }
