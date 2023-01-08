namespace Shared
open System
open System.Runtime.CompilerServices

open FSharp.Core

open EventSourcing

open FSharpPlus
open FSharpPlus.Data

module Cache =
    let dic = Collections.Generic.Dictionary<'H * List<Processable<'H>>, Result<'H, string>>()
    let queue = Collections.Generic.Queue<'H * List<Processable<'H>>>()
    [<MethodImpl(MethodImplOptions.Synchronized)>]
    let tryAddToDictionary (arg, res) =
        try
            dic.Add(arg, res)
            queue.Enqueue arg
            if (queue.Count > Conf.cacheSize) then
                let removed = queue.Dequeue()
                dic.Remove removed |> ignore
            ()
        with :? _ as e -> printf "warning: cache is doing something wrong %A\n" e

    let memoize (f: 'H -> Result<'H, string>) (arg: 'H * List<Processable<'H>>) =
        if (dic.ContainsKey arg) then
            dic.[arg]
        else
            let res = arg |> fst |> f
            tryAddToDictionary(arg, res)
            res

module Todos =
    open Shared
    open Utils

    type Projection1 =
        {
            timeAdded: Map<Guid, DateTime>
            timeRemoved: Map<Guid, DateTime>
        }
        with
            member this.AddTodo(todo) =
                {
                    this with
                        timeAdded = this.timeAdded.Add(todo.Id, DateTime.Now)
                }
            member this.RemoveTodo(id) =
                {
                    this with
                        timeRemoved = this.timeRemoved.Add(id, DateTime.Now)
                }
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
                let average = (double)total /(double)(addedAndFinished.Count)
                average

    type Todos =
        {
            todos: List<Todo>
            projection1: Projection1
        }
        with
            static member Zero =
                {
                    todos = []
                    projection1 =
                        {
                            timeAdded = [] |> Map.ofList
                            timeRemoved = [] |> Map.ofList
                        }
                }
            member this.AddTodo (t: Todo) =
                ceResult {
                    let! mustNotExist =
                        this.todos
                        |> List.exists (fun x -> x.Description = t.Description)
                        |> not
                        |> boolToResult
                    let projection  = this.projection1.AddTodo t
                    let result =
                        {
                            this with
                                todos = t::this.todos
                                projection1 = projection
                        }
                    return result
                }
            member this.RemoveTodo (id: Guid) =
                ceResult {
                    let! mustExist =
                        this.todos
                        |> List.exists (fun x -> x.Id = id)
                        |> boolToResult
                    let projection = this.projection1.RemoveTodo id
                    let result =
                        {
                            this with
                                todos = this.todos |> List.filter (fun x -> x.Id <> id)
                                projection1 = projection
                        }
                    return result
                }
            member this.GetTodos() = this.todos

module TodoEvents =
    open Todos
    type Event =
        | TodoAdded of Todo
        | TodoRemoved of Guid
            interface Processable<Todos> with
                member this.Process (x: Todos ) =
                    match this with
                    | TodoAdded (t: Todo) ->
                        Cache.memoize (fun (x: Todos) -> x.AddTodo t) (x, [this])
                    | TodoRemoved (g: Guid) ->
                        Cache.memoize (fun (x: Todos) -> x.RemoveTodo g) (x, [this])
