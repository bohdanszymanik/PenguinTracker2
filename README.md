# Penguin Tracker example built using the SAFE Template

This application is a full stack web app using F# server and client based on the [SAFE Stack](https://safe-stack.github.io/) template and technologies.

## What does it do?
Penguin tracker is used to record nest box observations being the number of adults,
the number of eggs, and the number of chicks.

The trick here is to provide a form embedded in a map so that it's possible
to drill down to a small area to find the nest box, then to enter data by
clicking on the nest box pin.

**NOTE: None of the pins included in this version of the code point to real nest box locations**. This is just a demonstration version!

## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications

* [.NET Core SDK](https://www.microsoft.com/net/download) 5.0 or higher
* [Node LTS](https://nodejs.org/en/download/)

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
## Testing the API
I use Huachao Mao's [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) for VS Code and you'll find a tests.http with sample queries in the root of the solution.

## SAFE Stack Documentation

If you want to know more about the full Azure Stack and all of it's components (including Azure) visit the official [SAFE documentation](https://safe-stack.github.io/docs/).

You will find more documentation about the used F# components at the following places:

* [Saturn](https://saturnframework.org/)
* [Fable](https://fable.io/docs/)
* [Elmish](https://elmish.github.io/elmish/)
