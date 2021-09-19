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


[<CLIMutable>]
type Status = {
    Adults: int
    Eggs: int
    Chicks: int
    Date: DateTime
 }
module Status =
    let adultsValid (adults: int) = (
        match adults with
        | x when x > 0 && x < 3 -> true
        | _ -> false
    )

    let create (adults:int) (eggs:int) (chicks:int) (date:DateTime) = {
        Adults = adults
        Eggs = eggs
        Chicks = chicks
        Date = date
    }

// we'll model the box status recordings as
// a list of status records against a box id
[<CLIMutable>]
type BoxStatuses = {Id:int; StatusList: Status list}
module BoxStatuses =
    let create (id:int) = {
        Id = id
        StatusList = []
    }

    let add (status: Status) (priorBoxStatuses: BoxStatuses) = {
        Id = priorBoxStatuses.Id
        StatusList = {
            Adults = status.Adults
            Eggs = status.Eggs
            Chicks = status.Chicks
            Date = status.Date
        } :: priorBoxStatuses.StatusList
    }


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
    {
        getBoxes: unit -> Async<Box list>
        addBox: Box -> Async<Box>
        getAllBoxStatuses: unit -> Async<BoxStatuses list>
        getBoxStatuses: int -> Async<BoxStatuses>
        addBoxStatuses: BoxStatuses -> Async<BoxStatuses>
        updateBoxStatuses: BoxStatuses -> Async<BoxStatuses>
        dummy: unit -> Async<string>
      }
