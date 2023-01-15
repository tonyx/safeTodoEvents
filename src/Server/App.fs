namespace BackEnd

module App =
    open Repository
    open Todos
    open MetaEvents
    open MetaCommands

    let getAllTodos() =
        ceResult {
            let! (_, state) = getState<Aggregate.Aggregate, Event> Aggregate.Aggregate.Zero
            let todos = (state.todos :?> Todos).GetTodos()
            return todos
        }
    let AverageTime() =
        ceResult {
            let! (_, state) = getState<Aggregate.Aggregate, Event> Aggregate.Aggregate.Zero
            let averageTime = (state.projection :?> Stat).AverageTodoTime()
            return (int) averageTime
        }
    let addTodo todo =
        ceResult {
            let! _ =
                todo |> Command.AddTodo |> (runCommand<Aggregate.Aggregate, Event> (Aggregate.Aggregate.Zero))
            return! (mksnapshotIfInterval<Aggregate.Aggregate, Event> Aggregate.Aggregate.Zero)
        }
    let removeTodo id =
        ceResult {
            let! _ =
                id |> Command.RemoveTodo |> (runCommand<Aggregate.Aggregate, Event> Aggregate.Aggregate.Zero)
            return! (mksnapshotIfInterval<Aggregate.Aggregate, Event> Aggregate.Aggregate.Zero)
        }