module Server.Tests

open Expecto

open Shared
open Server
open Server.AppTests

let server = testList "Server" [
    testCase "empty" <| fun _ ->
        Expect.isTrue true "true"
]

let all =
    testList "All"
        [
            server
            Server.AppTests.appTests
            Shared.Tests.shared
            Shared.Tests.domain
        ]

[<EntryPoint>]
let main _ = runTestsWithCLIArgs [] [||] all