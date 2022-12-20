namespace Shared
open System
open FSharp.Core
open FSharpPlus
open FSharpPlus.Data
open Newtonsoft.Json

module Utils =
    let deserialize<'H> (json: string): Result<'H, string> =
        try
            json |> JsonConvert.DeserializeObject<'H> |> Ok
        with
        | ex  -> Error (ex.ToString())
    type CeResultBuilder()  =
        member this.Bind(x, f) =
            match x with
            | Error x1 -> Error x1
            | Ok x1 -> f x1

        member this.MergeSources (x, y) =
            match x, y with
                | Error x1, _ -> Error x1
                | _, Error y1 -> Error y1
                | Ok x1, Ok y2 -> (x1, y2) |> Ok

        member this.Return(x) =
            x |> Ok

        member this.ReturnFrom(x) =
            x

    let catchErrors f l =
        let (okList, errors) =
            l
            |> List.map f
            |> Result.partition
        if (errors.Length > 0) then
            Result.Error (errors.Head)
        else
            okList |> Result.Ok

    let optionToResult x =
        match x with
        | Some x -> x |> Ok
        | _ -> Error "is None"

    let boolToResult x =
        match x with
        | true -> x |> Ok
        | false -> Error "false"