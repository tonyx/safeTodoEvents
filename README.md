# Event sourcing version of SAFE template

In this project I cloned the official SAFE Template and modified it to work in a "event sourcing" style.

Warning: this is not an official source fo the "event sourcing" topic.

The aim of this project is to investigate about a possible way to work on a domain logic in a way unaware of the persistency using event sourcing style.
Another goal is being able to make it a starting point for a library and a set of practices for event sourcing.

Particularly: the following files can be reused for any similar project:
* Db.fs: connect to the database writing and reading events and snapshots
* Repository.fs: using the previous Db.fs file is able to:
* 1) get the current state.
* 2) run a command, generating the corresponding event and storing them.
* 3) create snapshots

* EventSourcing.fs: defines abstractions based on interfaces, generics, constraints. Particularly, the aggregate (the Todos.fs class in my case) must implement the Root interface and must implement the evolve method calling the evolve function defined in this module. There is also need to define events in the same module where you define your Root class.

* Boilerplate code is essentially based on:
* 1.  defining events that Union based, that implements the Processable interface and are wrappers for methods on the Root (as in Todos.fs)
* 2.  defining Commands that implements the Executable<> interface and that are wrapper fot the events, as in the Commands.fs file
A more detailed explanation will come soon....
* 3. In App.fs file there is another wrapper (!) that exposes the logic of the commands in a way that they can be called in a "atomic" (i.e. transactional) way. There is also the logic for creating snapshots according to an interval policy.
Note that the aggregate defines a "zero" instance which is the initial state and this zero instance is passed around methods like "getSnapshot" just to make possible to return an initial snapshot if there is no one.
I guess that trick will not be needed anymore by using features like abstract static methods in interfaces.


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
