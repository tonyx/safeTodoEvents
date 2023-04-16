namespace BackEnd

open Shared
open System
open Todos

module Aggregate =
    type Aggregate =
        {
            model: Model
            projection: Model
        }
        static member Zero() =
            {
                model = Todos.Zero
                projection = Stat.Zero
            }
        member this.AddTodo (t: Todo) =
            ceResult
                {
                    let! model = this.model.AddTodo t
                    let! projection = this.projection.AddTodo t
                    return
                        {
                            this with
                                model = model
                                projection = projection
                        }
                }
        member this.RemoveTodo (id: Guid) =
            ceResult
                {
                    let! model = this.model.RemoveTodo id
                    let! projection = this.projection.RemoveTodo id
                    return
                        {
                            this with
                                model = model
                                projection = projection
                        }
                }
