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

