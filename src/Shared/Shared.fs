namespace Shared

open System

type Point = { Lat: float; Lng: float }

[<CLIMutable>]
type Box = { Id : int; Location : Point}

module Box =
    let isValid (box: Box) =
        box.Id < 20
        // TODO and we should check location in NZ roughly

    let create (number: int) (location: Point) =
        { Id = number
          Location = location }


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

// [<CLIMutable>]
// type Todo = { Id: Guid; Description: string }

// module Todo =
//     let isValid (description: string) =
//         String.IsNullOrWhiteSpace description |> not

//     let create (description: string) =
//         { Id = Guid.NewGuid()
//           Description = description }

module Route =
    let builder typeName methodName =
        printfn "/api/%s/%s" typeName methodName
        sprintf "/api/%s/%s" typeName methodName

// type ITodosApi =
//     { getTodos: unit -> Async<Todo list>
//       addTodo: Todo -> Async<Todo> }

type IBoxesApi =
    { getBoxes: unit -> Async<Box list>
      addBox: Box -> Async<Box> }
