namespace Shared
open System
open FSharp.Core

open FSharpPlus
open FSharpPlus.Data
open Utils

module EventSourcing =
    let ceResult = CeResultBuilder()

    type Processable<'H> =
        abstract member Process: 'H -> Result<'H, string>
    type MigrationRoot<'H, 'E when 'E :> Processable<'H>> =
        abstract member Evolve:  List<'E> -> Result<'H, string>
    type Root<'H, 'E when 'E :> Processable<'H>> =
        abstract member Evolve:  List<'E> -> Result<'H, string>

    type Executable<'H, 'E when 'E :> Processable<'H>> =
        abstract member Execute: 'H -> Result<List<'E>, string>

    let inline evolve<'H, 'E when 'E :> Processable<'H>> (h: 'H) (events: List<'E>) =
        events
        |> List.fold
            (fun (acc: Result<'H, string>) (e: 'E) ->
                match acc with
                    | Error err -> Error err
                    | Ok h -> h |> e.Process
            ) (h |> Ok)

    let inline executes (l: List<Executable<'H, 'E>>) (h: 'H) =
        let res =
            l |> catchErrors (fun c -> h |> c.Execute)
        match res with
            | Error x -> Error x
            | Ok x -> x |> List.fold (@) [] |> Ok