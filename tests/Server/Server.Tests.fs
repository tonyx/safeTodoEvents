module Server.Tests

open Expecto

let all =
    testList "All"
        [
            AppTests.appTests
            Shared.Tests.shared
            Shared.Tests.domain
        ]

[<EntryPoint>]
let main _ = runTestsWithCLIArgs [] [||] all