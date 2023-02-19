module Shared.Tests
open System

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open Shared

let domain = testList "Empty test list" [
    testCase "empty test" <| fun _ ->
        Expect.isTrue true "true"
]
