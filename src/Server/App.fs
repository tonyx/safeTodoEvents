namespace BackEnd

open Shared
open Shared.Todos
open System.Runtime.CompilerServices
open Shared.Commands

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
        action |> (runCommand<Todos, Event> Todos.Zero)

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

