module Server.AppTests

open System

open Expecto
open BackEnd

open Shared
open Server
open TodoEvents

let db = Repository.storage

let appTests =

    let third (_, _, c) = c

    testSequenced
    <| testList
        "App Tests" [
            testCase "add a todo"
                <| fun _ ->
                    db.DeleteAllEvents()
                    let todo =
                        {
                            Id = Guid.NewGuid()
                            Description = "write app tests"
                        }

                    let added = App.addTodo todo
                    Expect.isOk added "should be ok"
                    let retrieved = App.getAllTodos () |> Result.get
                    Expect.contains retrieved todo "should contain the added element"

            testCase "add and then remove a todo" <| fun _ ->
                db.DeleteAllEvents()
                let id = Guid.NewGuid()
                let todo = { Id = id; Description = "write tests" }
                let added = App.addTodo todo
                Expect.isOk added "should be ok"
                let retrieved = App.getAllTodos () |> Result.get
                Expect.contains retrieved todo "should contain the added element"
                let removed = App.removeTodo id
                Expect.isOk removed "should be ok"
                let retrieved' = App.getAllTodos () |> Result.get
                Expect.isEmpty retrieved' "should be empty"

            testCase "delete all events so that the current state is the init/zero state"
            <| fun _ ->
                db.DeleteAllEvents()

                let (_, state) =
                    Repository.getState<Todos.Todos, TodoEvents.Event> Todos.Todos.Zero
                    |> Result.get

                Expect.equal state Todos.Todos.Zero "shold be equal"

            testCase "after adding an event, the state is not zero"
            <| fun _ ->
                db.DeleteAllEvents()

                let todo =
                    {
                        Id = Guid.NewGuid()
                        Description = "add"
                    }

                let command = todo |> Commands.Command.AddTodo

                let _ =
                    command
                    |> (Repository.runCommand<Todos.Todos, Event> Todos.Todos.Zero)

                let (_, state) =
                    Repository.getState<Todos.Todos, TodoEvents.Event> Todos.Todos.Zero
                    |> Result.get

                Expect.notEqual state Todos.Todos.Zero "shold be equal"

            testCase "after adding an event, there is a snapshot in the db and it is not zero"
            <| fun _ ->
                db.DeleteAllEvents()

                let todo =
                    {
                        Id = Guid.NewGuid()
                        Description = "add"
                    }

                let appAddCommand = App.addTodo todo
                Expect.isOk appAddCommand "shouold be ok"

                let snap = db.TryGetLastSnapshot()
                Expect.isSome snap "should be some"

                let snapValue =
                    snap.Value
                    |> third
                    |> Utils.deserialize<Todos.Todos>

                Expect.isOk snapValue "should be ok"
                let expected = { Todos.Todos.Zero with todos = [ todo ] }
                Expect.equal (snapValue |> Result.get) expected "should be equal"

            testCase "after adding and addtodo event, then state is the zero plus the todo just added"
            <| fun _ ->
                db.DeleteAllEvents()

                let todo =
                    {
                        Id = Guid.NewGuid()
                        Description = "add"
                    }

                let appAddCommand = App.addTodo todo
                Expect.isOk appAddCommand "should be ok"

                let (_, state) =
                    BackEnd.Repository.getState<Todos.Todos, Event> (Todos.Todos.Zero)
                    |> Result.get

                let expected = { Todos.Todos.Zero with todos = [ todo ] }
                Expect.equal state expected "should be equal"

            testCase "add two addTodos events and then check that the state includes them"
            <| fun _ ->
                db.DeleteAllEvents()

                let todo =
                    {
                        Id = Guid.NewGuid()
                        Description = "add"
                    }

                let appAddCommand = App.addTodo todo
                Expect.isOk appAddCommand "shouold be ok"

                let todo2 =
                    {
                        Id = Guid.NewGuid()
                        Description = "add 2"
                    }

                let appAddCommand = App.addTodo todo2
                Expect.isOk appAddCommand "shouold be ok"

                let (_, state) =
                    BackEnd.Repository.getState<Todos.Todos, Event> (Todos.Todos.Zero)
                    |> Result.get

                let state' = { state with todos = ((state.todos) |> List.sort) }

                let expected = { Todos.Todos.Zero with todos = ([ todo2; todo ] |> List.sort) }
                Expect.equal state' expected "should be equal"

            testCase "add two addTodo events. The snapshot will by updated only to the first todo"
            <| fun _ ->
                db.DeleteAllEvents()

                let todo =
                    {
                        Id = Guid.NewGuid()
                        Description = "add"
                    }

                let appAddCommand = App.addTodo todo
                Expect.isOk appAddCommand "shouold be ok"

                let todo2 =
                    {
                        Id = Guid.NewGuid()
                        Description = "add2"
                    }

                let appAddCommand = App.addTodo todo2
                Expect.isOk appAddCommand "shouold be ok"

                let snap = db.TryGetLastSnapshot()
                Expect.isSome snap "should be some"

                let snapValue =
                    snap.Value
                    |> third
                    |> Utils.deserialize<Todos.Todos>

                Expect.isOk snapValue "should be ok"
                let expected = { Todos.Todos.Zero with todos = [ todo ] }
                Expect.equal (snapValue |> Result.get) expected "should be equal"

            testCase "after adding the first todo a snapshot will be created"
            <| fun _ ->
                db.DeleteAllEvents()
                let initSnapshot = db.TryGetLastSnapshot()
                Expect.isNone initSnapshot "should be none"
                let id = Guid.NewGuid()
                let todo = { Id = id; Description = "write tests" }
                let added = App.addTodo todo
                Expect.isOk added "should be ok"

                let (_, state) =
                    Repository.getState<Todos.Todos, TodoEvents.Event> Todos.Todos.Zero
                    |> Result.get

                let (_, _, snapshot) = (db.TryGetLastSnapshot().Value)

                let snapshotState =
                    snapshot
                    |> Utils.deserialize<Todos.Todos>
                    |> Result.get

                Expect.equal state snapshotState "should be equal"

            testCase "add a todo and then get state"
            <| fun _ ->
                db.DeleteAllEvents()
                let initSnapshot = db.TryGetLastSnapshot()
                Expect.isNone initSnapshot "should be none"

                let todo =
                    {
                        Id = Guid.NewGuid()
                        Description = "write tests again"
                    }

                let added = App.addTodo todo
                Expect.isOk added "should be ok"
                let st = Repository.getState<Todos.Todos, TodoEvents.Event> (Todos.Todos.Zero)
                Expect.isOk st "should be ok"

                let todo' =
                        {
                            Id = Guid.NewGuid()
                            Description = "write more tests"
                        }
                let added' = App.addTodo todo'
                Expect.isOk added' "should be ok"
          ]