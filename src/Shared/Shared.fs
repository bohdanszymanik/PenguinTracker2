namespace Shared

open System

type Point = { Lat: float; Lng: float }

type Box = { Number : int; Location : Point}

type Status = {
    Adults: int
    Eggs: int
    Chicks: int
 }

module Status =
    let adultsValid (adults: int) = (
        match adults with
        | x when x > 0 && x < 3 -> true
        | _ -> false
    )

    let create (adults:int) (eggs:int) (chicks:int) = {
        Adults = adults
        Eggs = eggs
        Chicks = chicks
    }

type BoxStatus = {PenguinBox: Box; PenguinStatus: Status}

[<CLIMutable>]
type Todo = { Id: Guid; Description: string }

module Todo =
    let isValid (description: string) =
        String.IsNullOrWhiteSpace description |> not

    let create (description: string) =
        { Id = Guid.NewGuid()
          Description = description }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type ITodosApi =
    { getTodos: unit -> Async<Todo list>
      addTodo: Todo -> Async<Todo> }
