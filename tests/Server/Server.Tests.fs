module Server.Tests

open Expecto

open Shared
open Server

let server = testList "Server" [
    testCase "empty" <| fun _ ->
        Expect.isTrue true "true"
]

let all =
    testList "All"
        [
            server
        ]

[<EntryPoint>]
let main _ = runTestsWithCLIArgs [] [||] all