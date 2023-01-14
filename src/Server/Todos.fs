
namespace BackEnd

open Shared
open FSharpPlus
open System

module Todos =
    open Utils

    type ITodo =
        abstract member AddTodo: Todo -> ITodo
        abstract member RemoveTodo: Guid -> ITodo

    let ceResult = CeResultBuilder()
    type Projection1 =
        {
            timeAdded: Map<Guid, DateTime>
            timeRemoved: Map<Guid, DateTime>
        }
        static member Zero =
            {
                timeAdded = [] |> Map.ofList
                timeRemoved = [] |> Map.ofList
            }
        member this.AddTodo(todo) =
            {
                this with
                    timeAdded = this.timeAdded.Add(todo.Id, DateTime.Now)
            }
            |> Ok
        member this.RemoveTodo(id) =
            {
                this with
                    timeRemoved = this.timeRemoved.Add(id, DateTime.Now)
            }
            |> Ok
        member this.AverageTodoTime() =
            let addedAndFinished =
                this.timeAdded.Keys |> Set.ofSeq |> Set.intersect (this.timeRemoved.Keys |> Set.ofSeq)
            let times =
                addedAndFinished
                |>>
                (fun x ->
                    let startedAt = this.timeAdded.[x]
                    let finishedAt = this.timeRemoved.[x]
                    let interval = finishedAt.Subtract(startedAt)
                    interval.Milliseconds
                )
            let total =
                times
                |> Set.fold (fun x y -> x + y) 0
            let average = (double)total/(double)(addedAndFinished.Count)
            average

    type Todos =
        {
            todos: List<Todo>
        }
        with
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
