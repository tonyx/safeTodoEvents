namespace BackEnd

open Shared
open System.Runtime.CompilerServices
open Todos
open System

module Cache =
    open EventSourcing
    let dic = Collections.Generic.Dictionary<Todos * List<Processable<'H>>, Result<'H, string>>()
    let queue = Collections.Generic.Queue<'H * List<Processable<'H>>>()
    [<MethodImpl(MethodImplOptions.Synchronized)>]
    let tryAddToDictionary (arg, res) =
        try
            dic.Add(arg, res)
            queue.Enqueue arg
            if (queue.Count > Conf.cacheSize) then
                let removed = queue.Dequeue()
                dic.Remove removed |> ignore
            ()
        with :? _ as e -> printf "warning: cache is doing something wrong %A\n" e

    let memoize (f: 'H -> Result<'H, string>) (arg: 'H * List<Processable<'H>>) =
        if (dic.ContainsKey arg) then
            dic.[arg]
        else
            let res = arg |> fst |> f
            tryAddToDictionary(arg, res)
            res