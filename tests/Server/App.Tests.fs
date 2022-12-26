
module Server.AppTests
open System

open Expecto
open BackEnd

open Shared
open Server

let appTests =
    testSequenced <|
        testList "App Tests" [
            testCase "add a todo" <| fun _ ->
                Db.deleteAllevents()
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
                Db.deleteAllevents()
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

            testCase "after adding the first event then a snapshot will be created immediately" <| fun _ ->
                Db.deleteAllevents()
                let initSnapshot = Db.tryGetLastSnapshot()
                Expect.isNone initSnapshot "should be none"
                let id = Guid.NewGuid()
                let todo =
                        {
                            Id = id
                            Description = "write tests"
                        }
                let added = App.addTodo todo
                Expect.isOk added "should be ok"
                let (_, state) = Repository.getState<Todos.Todos, Todos.Event> Todos.Todos.Zero |> Result.get
                let (_, snapshot) = (Db.tryGetLastSnapshot().Value)
                let snapshotState = snapshot |> Utils.deserialize<Todos.Todos> |> Result.get
                Expect.equal state snapshotState "should be equal"

            testCase "add few events and then the last snapshots will be unaligned from the current state" <| fun _ ->
                Db.deleteAllevents()
                let initSnapshot = Db.tryGetLastSnapshot()
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
                let (_, state) = Repository.getState<Todos.Todos, Todos.Event> Todos.Todos.Zero |> Result.get
                let (_, snapshot) = (Db.tryGetLastSnapshot().Value)
                let snapshotState = snapshot |> Utils.deserialize<Todos.Todos> |> Result.get
                Expect.notEqual state snapshotState "should be equal"
        ]

