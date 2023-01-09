namespace BackEnd

open Shared
open Shared.Todos
open Shared.Commands
open Shared.TodoEvents
open FSharpPlus
open FSharpPlus.Operators
open System.Runtime.CompilerServices
open System

module Cache' =
    open EventSourcing
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
module Todos' =
    open Shared
    open Utils
    let ceResult = CeResultBuilder()
    type Projection =
        abstract member AddTodo: Todo -> Projection
        abstract member RemoveTodo: Guid -> Projection

    // [<Newtonsoft.Json.JsonConverter(typedefof<Repository.ConcreteConverter<Projection1>>)>]
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
                    // let projection  = (this.projection1:>Projection).AddTodo t
                    // let projection  = this.projection1.AddTodo t
                    let result =
                        {
                            this with
                                todos = t::this.todos
                                // projection1 = projection
                        }
                    return result
                }
            member this.RemoveTodo (id: Guid) =
                ceResult {
                    let! mustExist =
                        this.todos
                        |> List.exists (fun x -> x.Id = id)
                        |> boolToResult
                    // let projection = this.projection1.RemoveTodo id
                    let result =
                        {
                            this with
                                todos = this.todos |> List.filter (fun x -> x.Id <> id)
                                // projection1 = projection
                        }
                    return result
                }
            member this.GetTodos() = this.todos

module TodoEvents' =
    open Todos'
    open Shared.EventSourcing
    type Event =
        | TodoAdded of Todo
        | TodoRemoved of Guid
            interface Processable<Todos> with
                member this.Process (x: Todos ) =
                    match this with
                    | TodoAdded (t: Todo) ->
                        Cache'.memoize (fun (x: Todos) -> x.AddTodo t) (x, [this])
                    | TodoRemoved (g: Guid) ->
                        Cache'.memoize (fun (x: Todos) -> x.RemoveTodo g) (x, [this])

module Commands' =
    open Todos'
    open TodoEvents'
    open EventSourcing
    type Command =
        | AddTodo of Todo
        | RemoveTodo of Guid

        interface Executable<Todos, Event> with
            member this.Execute (x: Todos) =
                match this with
                | AddTodo t ->
                    match
                        Cache'.memoize (fun x -> x.AddTodo t) (x, [Event.TodoAdded t]) with
                        | Ok _ -> [Event.TodoAdded t] |> Ok
                        | Error x -> x |> Error
                | RemoveTodo g ->
                    match
                        Cache'.memoize (fun x -> x.RemoveTodo g) (x, [Event.TodoRemoved g]) with
                        | Ok _ -> [Event.TodoRemoved g] |> Ok
                        | Error x -> x |> Error

        static member Executes (l: List<Executable<Todos, Event>>) (t: Todos) =
            executes l t
module App =
    open Repository
    open Todos'
    open TodoEvents'
    open Commands'

    let getAllTodos() =
        ceResult {
            let! (_, state) = getState<Todos, Event> (Todos.Zero)
            let todos = state.GetTodos()
            return todos
        }

    let AverageTime() =
        ceResult {
            let! (_, state) = getState<Todos, Event> (Todos.Zero)
            let averageTime = state.projection1.AverageTodoTime()
            return (int) averageTime
        }

    let addTodo todo =
        printf "service add todo 0\n"
        ceResult {
            printf "service add todo 1\n"
            let! _ =
                todo |> Command.AddTodo |> (runCommand<Todos, Event> Todos.Zero)
            printf "service add todo 2\n"
            return! (mksnapshotIfInterval<Todos, Event> Todos.Zero)
        }

    let removeTodo id =
        ceResult {
            let! _ =
                id |> Command.RemoveTodo |> (runCommand<Todos, Event> Todos.Zero)
            return! (mksnapshotIfInterval<Todos, Event> Todos.Zero)
        }

