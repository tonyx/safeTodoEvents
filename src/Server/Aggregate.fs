namespace BackEnd

open Shared
open FSharpPlus
open System
open Todos

module Aggregate =
    type Aggregate =
        {
            todos: Todos
            projection: Projection1
        }
        with
            static member Zero =
                {
                    todos = Todos.Zero
                    projection = Projection1.Zero
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
                                todos = todos
                                projection = projection
                            }
                    }
