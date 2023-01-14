namespace BackEnd

open Shared
open System
open Aggregate

module MetaEvents =
    open Todos
    open Shared.EventSourcing
    type Event =
        | TodoAdded of Todo
        | TodoRemoved of Guid
            interface Processable<Aggregate.Aggregate> with
                member this.Process (x: Aggregate.Aggregate ) =
                    match this with
                    | TodoAdded (t: Todo) -> x.AddTodo t
                    | TodoRemoved (g: Guid) ->x.RemoveTodo g

