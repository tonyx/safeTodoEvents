namespace Shared
open System
open System.Runtime.CompilerServices

open FSharp.Core

open EventSourcing

open FSharpPlus
open FSharpPlus.Data

module rec Todos =
    open Shared
    open Utils

    module Cache =
        let dic = Collections.Generic.Dictionary<'H * Processable<'H>, Result<'H, string>>()
        let queue = Collections.Generic.Queue<'H * Processable<'H>>()
        [<MethodImpl(MethodImplOptions.Synchronized)>]
        let addToDictionary (arg, res) =
            dic.Add(arg, res)
            queue.Enqueue arg
            if (queue.Count > 13) then
                let removed = queue.Dequeue()
                dic.Remove removed |> ignore
            ()

        let memoize (f: 'H -> Result<'H, string>) (arg: 'H * Processable<'H>) =
            if (dic.ContainsKey arg) then
                dic.[arg]
            else
                let res = arg |> fst |> f
                addToDictionary(arg, res)
                res
    type TodosNextVer =
        {
            bla: string
        }

    let todonextInst =
        {
            bla = ""
        }

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
                    | TodoAdded (t: Todo) ->
                        Cache.memoize (fun x -> x.AddTodo t) (x, this)
                    | TodoRemoved (g: Guid) ->
                        Cache.memoize (fun x -> x.RemoveTodo g) (x, this)


