
namespace Shared
open FSharp.Core


module Conf =
    let isTestEnv = true
    // atm there is only one db, but of course we need different db with different rights
    // delete events should be allowed only in dev/test
    // At the countrary, in prod db the data should always be read only
    // the events must be read only and must never be deleted
    // we may at most need to delete the snapshots after deploying a new version of the root type (see readme)
    // assuming the substitution principle is respected
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
