
namespace Shared
open FSharp.Core


module Conf =
    let isTestEnv = true
    // atm there is only one db, but here I point out that we need different rights in
    // dev/test and in prod respectively.
    // For instance in dev/test we want to give rights to delete events and snapshots
    // At the opposite: in prod db we need to ensure append only mode.
    // We may at most need to delete the snapshots after deploying a new version of the root type (see readme)
    // as long as we respect th3 substitution principle between versions
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
