# Event sourcing version of SAFE template

I reused the official SAFE Template to make it work in the "event sourcing" style.

The domain [Todos.fs](./src/Shared/Todos.fs) is unaware the persistency logic. I needed to add just some boilerplate code (events, commands, dispatching).

Should I work on a different domain in a similar way I would replace the Todos.fs with my domain, write  events, commands in [Commands.fs](./src/Shared/Commands.fs), and [App.fs](./src/Server/App.fs).
I would reuse The following files:
* [Db.fs](./src/Server/Db.fs): connect to the database for writing and reading events and snapshots
* [Repository.fs](./src/Server/Repository.fs): rely on the previous Db.fs file to:
* 1) get the current state.
* 2) run commands, generating the corresponding events and storing them.
* 3) store and read snapshots
* 4) [EventSourcing.fs](./src/Shared/EventSourcing.fs): defines abstractions based on interfaces, generics, constraints.

Some small details:
* 1.  The domain must implement the "Root interface", defining the Evolve member which must call the _evolve_ defined in EventSourcing.fs.
* 2.  events must be discriminated Unions implementing the _Processable_ interface, to wrap the members defined in the aggregate ([Todos.fs](./src/Shared/Todos.fs).
* 3.  _Commands_ implement the _Executable_ interface and returns events or error, as in the [Commands.fs](./src/Shared/Commands.fs).
* 4.  In [App.fs](./src/Server/App.fs) I have another wrapper which exposes the logic of the commands in an a "atomic" (i.e. transactional) way to ensure integrity of the stored events (they should never return error).

Other "dispatching" logic work in the same way as they original are part of the way the original SAFE example. (i.e. ITodosApi and  interface and implementation)

## Installation
The ordinary SAFE requirement (as follows) plus a postgres database using the Schema.sql script to create user, database, and tables

## Notes:
My approach is quite unconventional at the moment: Events can return Error, and commands just pre-apply events to the state and see if the result is not an error, and only in that case they return the events. This can change to comply to the standard approach, so that events will never return errors.

# SAFE Template


This template can be used to generate a full-stack web application using the [SAFE Stack](https://safe-stack.github.io/). It was created using the dotnet [SAFE Template](https://safe-stack.github.io/docs/template-overview/). If you want to learn more about the template why not start with the [quick start](https://safe-stack.github.io/docs/quickstart/) guide?

## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications

* [.NET Core SDK](https://www.microsoft.com/net/download) 6.0 or higher
* [Node 16](https://nodejs.org/en/download/)

## Starting the application

Before you run the project **for the first time only** you must install dotnet "local tools" with this command:

```bash
dotnet tool restore
```

To concurrently run the server and the client components in watch mode use the following command:

```bash
dotnet run
```

Then open `http://localhost:8080` in your browser.

The build project in root directory contains a couple of different build targets. You can specify them after `--` (target name is case-insensitive).

To run concurrently server and client tests in watch mode (you can run this command in parallel to the previous one in new terminal):

```bash
dotnet run -- RunTests
```

Client tests are available under `http://localhost:8081` in your browser and server tests are running in watch mode in console.

Finally, there are `Bundle` and `Azure` targets that you can use to package your app and deploy to Azure, respectively:

```bash
dotnet run -- Bundle
dotnet run -- Azure
```

## SAFE Stack Documentation

If you want to know more about the full Azure Stack and all of it's components (including Azure) visit the official [SAFE documentation](https://safe-stack.github.io/docs/).

You will find more documentation about the used F# components at the following places:

* [Saturn](https://saturnframework.org/)
* [Fable](https://fable.io/docs/)
* [Elmish](https://elmish.github.io/elmish/)
