
namespace Shared
open System
open FSharp.Core

open FSharpPlus
open FSharpPlus.Data
open Utils

module Conf =
    let isTestEnv = true
    let connectionString =
        if isTestEnv then
            "Server=127.0.0.1;"+
            "Database=todo;" +
            "User Id=todo;"+
            "Password=todo;"
        else
            "Server=127.0.0.1;"+
            "Database=todo;" +
            "User Id=todo;"+
            "Password=todo;"