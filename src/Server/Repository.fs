
namespace BackEnd

open FSharp.Data.Sql
open FSharpPlus
open Shared.Utils
open Shared.EventSourcing
open Newtonsoft.Json

module Repository =
    open BackEnd
    let ceError = CeErrorBuilder()

    let inline getLastSnapshot<'H> (zero: 'H) =
        ceError {
            let! result =
                match Db.tryGetLastSnapshot()  with
                | Some (id, json) ->
                    let state = json |> deserialize<'H>
                    match state with
                    | Error e -> Error e
                    | _ -> (id, state |> Result.get) |> Ok
                | None -> (0, zero) |> Ok
            return result
        }

    let getState<'H, 'E when 'E :> Processable<'H>> (zero: 'H) =
        ceError {
            let! (id, state) = getLastSnapshot<'H> (zero)
            let events = Db.getEventsAfterId id
            let lastId =
                match events.Length with
                | x when x > 0 -> events |> List.last |> fst
                | _ -> id
            let! events' =
                events |>> snd |> catchErrors deserialize<'E>
            let! result =
                events' |> evolve<'H, 'E> state
            return (lastId, result)
        }

    let runCommand<'H, 'E when 'E :> Processable<'H>> (zero: 'H) (command: Executable<'H, 'E>)  =
        ceError {
            let! (_, state) = getState<'H, 'E> (zero)
            let! events =
                state
                |> command.Execute
            let! eventsAdded =
                Db.addEvents (events |>> JsonConvert.SerializeObject)
            return eventsAdded
        }

    let mksnapshot<'H, 'E when 'E :> Processable<'H>> (zero: 'H) =
        ceError
            {
                let! (id, state) = getState<'H, 'E> (zero: 'H)
                let snapshot = state |> JsonConvert.SerializeObject
                let! result = Db.setSnapshot id snapshot
                return result
            }

    let mksnapshotIfInterval<'H, 'E when 'E :> Processable<'H>> (zero: 'H) =
        ceError
            {
                let! lastEventId = Db.getLastEventId() |> optionToResult
                let! lastSnapshot = getLastSnapshot<'H> (zero)
                let snapId = lastSnapshot |> fst

                let! result =
                    if ((lastEventId - snapId) > Db.snapshotInterval) then
                        mksnapshot<'H, 'E> (zero)
                    else () |> Ok

                return result
            }

