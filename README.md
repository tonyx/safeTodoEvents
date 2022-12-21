# Event sourcing version of SAFE template

In this project, I cloned the official SAFE Template and modified it to work in an "event sourcing" style.

Warning: this is not an official guide of the "event sourcing" topic. It is just my experiment.

It looks to me that the advantage is the possibility to work effectively on the domain logic in a way unaware of the persistency because the storage is uniform for any project (no other entity to store except events and snapshots). A little price to pay is having to add some boilerplate code (such as  events as a wrapper for members of the aggregate, and commands that returns events, dispatching logic).

The following files can be reused for any similar project:
* [Db.fs](./src/Server/Db.fs): connect to the database for writing and reading events and snapshots
* [Repository.fs](./src/Server/Repository.fs): rely on the previous Db.fs file to:
* 1) get the current state.
* 2) run commands, generating the corresponding events and storing them.
* 3) store and read snapshots

* [EventSourcing.fs](./src/Shared/EventSourcing.fs): defines abstractions based on interfaces, generics, constraints. Specifically, any aggregate (the [Todos.fs](./src/shared/Todos.fs) class in my case) - which is basically a coherent and consistent part of the domain - must implement the Root interface and must implement the related __Evolve__ member by calling the _evolve_ function defined in this module. You  also need to define events in the same module where you define your Root class.
The members of the aggregate must work in a functional style. For instance the _AddTodo_ member must return a copy of the Todos itself including the new todo.

* We need to add mechanically some pieces of code to define events, commands, dispatching. Particularly:
* 1.  events as discriminated Unions implementing the _Processable_ interface, to wrap the members defined in the aggregate (as in Todos.fs that includes both the aggregate and the events)
* 2.  defining _Commands_ that implements the _Executable_ interface and that returns events, as in the [Commands.fs](./src/Shared/Commands.fs).
* 3.  In [App.fs](./src/Server/App.fs) source file I have another wrapper (!) that exposes the logic of the commands in a way that they can be called in a "atomic" (i.e. transactional) way. In this way I think we ensure that the integrity of the  context of the execution of any command that returns events, is preserved: if the command returns events that will not end up in an error, then the event that are stored will not generate errors because the context cannot be changed because of the "syncronized" attribute to the function that actually execute the command and store the related events (if the result is Ok for both the commands and the events).
The Events, if stored, should never return error, but anyway I made them returning _Result_ just in case.

Finally, the app expose the function that actually create and run commands and eventually create also a snapshot.

Other "dispatching" logic work in the same way as they original are part of the way the original SAFE example. (i.e. ITodosApi and  interface and implementation)

Note that the aggregate needs a "zero" instance which is the initial state and this zero instance is passed around methods like "getSnapshot" just to make possible to return an initial snapshot if there is no one.
I guess that this can be improved by using features that are available only in .net 7.0 like abstract static methods in interfaces.

## Final thoughts:
It looks to me that by such kind of approach it could be possible to focus on conversations, modeling, testing on the aggregate without hitting the database or other parts that are typically slower.

## Installation
The ordinary SAFE requirement (as follows) plus a postgres database using the Schema.sql script to create user, database, and tables


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
