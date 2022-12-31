
module Server.AppTests
open System

open Expecto
open BackEnd

open Shared
open Server
open TodoEvents

let db: EStorage =  Db'.pgDb()
let appTests =
    testSequenced <|
        testList "App Tests" [
            testCase "add a todo" <| fun _ ->
                db.DeleteAllEvents()
                let todo =
                        {
                            Id = Guid.NewGuid()
                            Description = "write app tests"
                        }
                let added = App.addTodo todo
                Expect.isOk added "should be ok"
                let retrieved = App.getAllTodos() |> Result.get
                Expect.contains retrieved todo "should contain the added element"

            testCase "add and then remove a todo" <| fun _ ->
                db.DeleteAllEvents()
                let id = Guid.NewGuid()
                let todo =
                        {
                            Id = id
                            Description = "write tests"
                        }
                let added = App.addTodo todo
                Expect.isOk added "should be ok"
                let retrieved = App.getAllTodos() |> Result.get
                Expect.contains retrieved todo "should contain the added element"
                let removed = App.removeTodo id
                Expect.isOk removed "should be ok"
                let retrieved' = App.getAllTodos() |> Result.get
                Expect.isEmpty retrieved' "should be empty"

            testCase "after adding the first todo a snapshot will be created" <| fun _ ->
                db.DeleteAllEvents()
                let initSnapshot = db.TryGetLastSnapshot()
                Expect.isNone initSnapshot "should be none"
                let id = Guid.NewGuid()
                let todo =
                        {
                            Id = id
                            Description = "write tests"
                        }
                let added = App.addTodo todo
                Expect.isOk added "should be ok"
                let (_, state) = Repository.getState<Todos.Todos, TodoEvents.Event> Todos.Todos.Zero |> Result.get
                let (_, snapshot) = (db.TryGetLastSnapshot().Value)
                let snapshotState = snapshot |> Utils.deserialize<Todos.Todos> |> Result.get
                Expect.equal state snapshotState "should be equal"

            testCase "add few todos and then the last snapshot will be unaligned respect of the current state" <| fun _ ->
                db.DeleteAllEvents()
                let initSnapshot = db.TryGetLastSnapshot()
                Expect.isNone initSnapshot "should be none"
                let todo =
                        {
                            Id = Guid.NewGuid()
                            Description = "write tests"
                        }
                let added = App.addTodo todo
                Expect.isOk added "should be ok"
                let todo' =
                        {
                            Id = Guid.NewGuid()
                            Description = "write more tests"
                        }
                let added' = App.addTodo todo'
                Expect.isOk added' "should be ok"
                let (_, state) = Repository.getState<Todos.Todos, TodoEvents.Event> Todos.Todos.Zero |> Result.get
                let (_, snapshot) = (db.TryGetLastSnapshot().Value)
                let snapshotState = snapshot |> Utils.deserialize<Todos.Todos> |> Result.get
                Expect.notEqual state snapshotState "should be equal"
        ]
