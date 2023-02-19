
module Server.ModelTests

open System

open Expecto
open BackEnd

open BackEnd.Todos
open BackEnd.Commands
open BackEnd.Events
open BackEnd.Aggregate
open Shared
open Server

let db = Repository.storage

let modelTests =
    testList
        "Model Tests" [
            testCase "add a new todotodo"
                <| fun _ ->
                    let todos = Todos.Zero
                    let todo = {
                        Id = Guid.NewGuid()
                        Description = "a new todo"
                    }
                    let result = (todos :> Model).AddTodo todo
                    Expect.isOk result "should be ok"

            testCase "add a todo with an already existing description - Error" <| fun _ ->
                let todos = Todos.Zero
                let todo = {
                    Id = Guid.NewGuid()
                    Description = "todo"
                }
                let todos' = (todos :> Model).AddTodo todo |> Result.get

                let anotherTodo = {
                    Id = Guid.NewGuid()
                    Description = "todo"
                }
                let result = todos'.AddTodo anotherTodo
                Expect.isError result "should be error"

            testCase "remove a todo - Ok" <| fun _ ->
                let guid = Guid.NewGuid()
                let todos =
                    {
                        Todos.Zero with
                            todos =
                                [
                                    {
                                        Id = guid
                                        Description = "todo"
                                    }
                                ]
                    }
                let result = (todos :> Model).RemoveTodo guid
                Expect.isOk result "should be ok"
                let result' = result |> Result.get
                Expect.equal result' Todos.Zero "should be equal"
        ]
