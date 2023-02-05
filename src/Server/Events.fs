namespace BackEnd

open Shared
open System
open EventSourcing
open BackEnd.Aggregate
module Events =
    open Cache
    type Event =
        | TodoAdded of Todo
        | TodoRemoved of Guid
            interface Processable<Aggregate> with
            // interface Processable<Aggregate.Aggregate> with
                // member this.Process (x: Aggregate.Aggregate ) =
                member this.Process (x: Aggregate ) =
                    match this with
                    | TodoAdded (t: Todo) ->
                        // EventCache<Aggregate.Aggregate>.Instance.Memoize (fun () -> x.AddTodo t) (x, [TodoAdded t])
                        EventCache<Aggregate>.Instance.Memoize (fun () -> x.AddTodo t) (x, [TodoAdded t])
                    | TodoRemoved (g: Guid) ->
                        // EventCache<Aggregate.Aggregate>.Instance.Memoize (fun () -> x.RemoveTodo g) (x, [TodoRemoved g])
                        EventCache<Aggregate>.Instance.Memoize (fun () -> x.RemoveTodo g) (x, [TodoRemoved g])

