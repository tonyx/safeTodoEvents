
namespace BackEnd

open System.Runtime.CompilerServices
open FSharp.Data.Sql
open FSharp.Core
open FSharpPlus
open FSharpPlus.Data
open Shared
open Shared.Utils
open Shared.EventSourcing
open BackEnd.Cache
open Newtonsoft.Json

module Repository =
    let storage: IStorage =
        match Conf.storageType with
            | Conf.StorageType.Memory -> MemoryStorage.MemoryStorage.Instance
            | Conf.StorageType.Postgres -> DbStorage.PgDb()

    let ceResult = CeResultBuilder()
    let inline getLastSnapshot<'H when ^H: (static member Zero: unit -> ^H)>  (zero: 'H) =
        ceResult {
            let! result =
                match storage.TryGetLastSnapshot()  with
                | Some (id, eventId, json) ->
                    let state = SnapCache<'H>.Instance.Memoize(fun () -> json |> deserialize<'H>) id
                    match state with
                    | Error e -> Error e
                    | _ -> (eventId, state |> Result.get) |> Ok
                | None -> (0, zero) |> Ok
            return result
        }

    let inline getState<'H, 'E
        when ^H: (static member Zero: unit -> ^H)
        and 'E :> Processable<'H>> (zero: 'H) =
        ceResult {
            let! (id, state) = getLastSnapshot<'H> (zero)
            let events = storage.GetEventsAfterId id
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

    [<MethodImpl(MethodImplOptions.Synchronized)>]
    let inline runCommand<'H, 'E
        when ^H: (static member Zero: unit -> ^H)
        and
        'E :> Processable<'H>> (zero: 'H) (command: Executable<'H, 'E>)  =
        ceResult {
            let! (_, state) = getState<'H, 'E> (zero)
            let! events =
                state
                |> command.Execute
            let! eventsAdded =
                storage.AddEvents (events |>> JsonConvert.SerializeObject)
            return eventsAdded
        }

    let inline mksnapshot<'H, 'E
        when ^H: (static member Zero: unit -> ^H)
        and 'E :> Processable<'H>> (zero: 'H) =
        ceResult
            {
                let! (id, state) = getState<'H, 'E> (zero: ^H)
                let snapshot = JsonConvert.SerializeObject(state, Utils.serSettings)
                let! result = storage.SetSnapshot (id, snapshot)
                return result
            }

    let inline mksnapshotIfInterval<'H, 'E
        when ^H: (static member Zero: unit -> ^H)
        and 'E :> Processable<'H>> (zero: 'H) =
        ceResult
            {
                let! lastEventId = storage.TryGetLastEventId() |> optionToResult
                let snapEventId = storage.TryGetLastSnapshotEventId() |> optionToDefault 0

                let! result =
                    if ((lastEventId - snapEventId) > Conf.intervalBetweenSnapshots || snapEventId = 0) then
                        mksnapshot<'H, 'E> (zero)
                    else
                        () |> Ok
                return result
            }

