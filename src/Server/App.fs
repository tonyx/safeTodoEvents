namespace BackEnd

open Shared
open Shared.Todos
open Shared.Commands
open Shared.TodoEvents

module App =
    open Repository

    let getAllTodos() =
        ceResult {
            let! (_, state) = getState<Todos, Event> (Todos.Zero)
            let todos = state.GetTodos()
            return todos
        }

    let addTodo todo =
        ceResult {
            let! _ =
                todo |> Command.AddTodo |> (runCommand<Todos, Event> Todos.Zero)
            return!  (mksnapshotIfInterval<Todos, Event> Todos.Zero)
        }

    let removeTodo id =
        ceResult {
            let! _ =
                id |> Command.RemoveTodo |> (runCommand<Todos, Event> Todos.Zero)
            return!  (mksnapshotIfInterval<Todos, Event> Todos.Zero)
        }

