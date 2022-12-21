namespace BackEnd

open System
open FSharp.Data.Sql
open FSharpPlus
open Shared
open Shared.Todos
open System.Runtime.CompilerServices
open Shared.Commands
open Shared.EventSourcing
open Newtonsoft.Json

module App =
    open Repository

    let getAllTodos() =
        ceResult {
            let! (_, state) = getState<Todos, Event> (Todos.Zero)
            let todos = state.GetTodos()
            return todos
        }

    [<MethodImpl(MethodImplOptions.Synchronized)>]
    let doAtomicAction (action: Command ) =
        match action with
        | Command.AddTodo todo ->
            todo |> Command.AddTodo |> (runCommand<Todos, Event> Todos.Zero)
        | Command.RemoveTodo id ->
            id |> Command.RemoveTodo |> (runCommand<Todos, Event> Todos.Zero)

    let addTodo todo =
        ceResult {
            let! result = doAtomicAction(Command.AddTodo todo)
            let _ = mksnapshotIfInterval<Todos, Event> Todos.Zero
            return result
        }

    let removeTodo id =
        ceResult {
            let! result = doAtomicAction(Command.RemoveTodo id)
            let _ = mksnapshotIfInterval<Todos, Event> Todos.Zero
            return result
        }

