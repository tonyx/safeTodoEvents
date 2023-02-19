module Server.Tests

open Expecto

let all =
    testList "All"
        [
            AppTests.appTests
            ModelTests.modelTests
        ]

[<EntryPoint>]
let main _ = runTestsWithCLIArgs [] [||] all