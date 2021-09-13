module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn
open Shared
open LiteDB.FSharp
open LiteDB
open LiteDB.FSharp.Linq

type Storage () =
    let database =
        let mapper = FSharpBsonMapper()
        let connStr = "Filename=Boxes.db;mode=Exclusive"
        new LiteDatabase (connStr, mapper)

    let boxes = database.GetCollection<Box> "boxes"
    let allBoxStatuses = database.GetCollection<BoxStatuses> "boxStatuses"
    // let todos = database.GetCollection<Todo> "todos"

    /// Retrieves all boxes
    member _.GetBoxes () =
        let retrievedboxes = boxes.FindAll () |> List.ofSeq
        printfn "Caller is retrieving boxes: %A" retrievedboxes
        retrievedboxes

    /// Adds a nest box
    member _.AddBox (box:Box) =
        if Box.isValid box then
            boxes.Insert box |> ignore
            Ok ()
        else
            Error "Invalid box"

    member __.AddBoxStatus(boxStatuses: BoxStatuses) =
        allBoxStatuses.Insert boxStatuses |> ignore
        Ok()

    member __.GetAllBoxStatuses() =
        printfn "Length of BoxStatus collection %A" (allBoxStatuses.LongCount())
        allBoxStatuses.FindAll () |> List.ofSeq

    /// Retrieve Box Statuses
    member __.GetBoxStatuses(id:int) =
        let retrievedBoxStatuses = allBoxStatuses.FindById(BsonValue(id))
        printfn "Caller is retrieving box statuses: %A" retrievedBoxStatuses
        retrievedBoxStatuses

    // /// Retrieves all todo items.
    // member _.GetTodos () =
    //     todos.FindAll () |> List.ofSeq

    // /// Tries to add a todo item to the collection.
    // member _.AddTodo (todo:Todo) =
    //     if Todo.isValid todo.Description then
    //         todos.Insert todo |> ignore
    //         Ok ()
    //     else
    //         Error "Invalid todo"

// type Storage() =
//     let todos = ResizeArray<_>()

    // member __.GetTodos() = List.ofSeq todos

    // member __.AddTodo(todo: Todo) =
    //     if Todo.isValid todo.Description then
    //         todos.Add todo
    //         Ok()
    //     else
    //         Error "Invalid todo"

let storage = Storage()

if storage.GetBoxes() |> Seq.isEmpty then
    printfn "GetBoxes returns empty so better make some"
    storage.AddBox(Box.create 1 {Lat = -41.3492; Lng = 174.7729}) |> ignore
    storage.AddBox(Box.create 2 {Lat = -41.3495; Lng = 174.7725}) |> ignore
    storage.AddBox(Box.create 3 {Lat = -41.3495; Lng = 174.7732}) |> ignore
    storage.AddBox(Box.create 4 {Lat = -41.3493; Lng = 174.7738}) |> ignore
    storage.AddBox(Box.create 5 {Lat = -41.3491; Lng = 174.7739}) |> ignore

if storage.GetAllBoxStatuses() |> Seq.isEmpty then
    printfn "GetBoxStatuses returns empty so better make some"
    storage.AddBoxStatus(BoxStatuses.create 1 ) |> ignore
    storage.AddBoxStatus(BoxStatuses.create 2 ) |> ignore
    storage.AddBoxStatus(BoxStatuses.create 3 ) |> ignore
    storage.AddBoxStatus(BoxStatuses.create 4 ) |> ignore
    storage.AddBoxStatus(BoxStatuses.create 5 ) |> ignore

// if storage.GetTodos() |> Seq.isEmpty then
//     storage.AddTodo(Todo.create "Create new SAFE project") |> ignore
//     storage.AddTodo(Todo.create "Write your app") |> ignore
//     storage.AddTodo(Todo.create "Ship it !!!") |> ignore

// storage.AddTodo(Todo.create "Create new SAFE project")
// |> ignore

// storage.AddTodo(Todo.create "Write your app")
// |> ignore

// storage.AddTodo(Todo.create "Ship it !!!")
// |> ignore

let boxesApi =
    { getBoxes = fun() -> async { return storage.GetBoxes() }
      addBox =
        fun box ->
            async {
                match storage.AddBox box with
                | Ok () -> return box
                | Error e -> return failwith e
            }
      getAllBoxStatuses =
        fun () -> async { return storage.GetAllBoxStatuses() }
      getBoxStatuses =
        fun (i:int) -> async { return storage.GetBoxStatuses(i) }
    //   getBoxStatus =
    //     fun n -> async { return storage.GetBoxStatus(n) }
      addBoxStatus =
        fun boxStatus ->
            async {
                match storage.AddBoxStatus boxStatus with
                | Ok () -> return boxStatus
                | Error e -> return failwith e
            }
    }

// let todosApi =
//     { getTodos = fun () -> async { return storage.GetTodos() }
//       addTodo =
//           fun todo ->
//               async {
//                   match storage.AddTodo todo with
//                   | Ok () -> return todo
//                   | Error e -> return failwith e
//               }
//     }

let webApp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue boxesApi
    // |> Remoting.fromValue todosApi
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
