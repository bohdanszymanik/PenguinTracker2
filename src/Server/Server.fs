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
    let allBoxStatuses = database.GetCollection<BoxStatuses> "allBoxStatuses" // seems like the string name needs to match the left hand side?

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

    member __.AddBoxStatuses(boxStatuses: BoxStatuses) =
        printfn "In Storage.AddBoxStatuses - about to insert boxStatuses %A to allBoxStatuses %A" boxStatuses allBoxStatuses
        // weird thing here - litedb upsert returns bool here but in litedb source it returns int count of updates
        allBoxStatuses.Upsert boxStatuses |> ignore
        Ok()

    member __.UpdateBoxStatuses(boxStatuses: BoxStatuses) =
        allBoxStatuses.Update(boxStatuses) |> ignore
        Ok()

    member __.GetAllBoxStatuses() =
        printfn "In GetAllBoxStatuses()"
        let t = allBoxStatuses.FindAll()
        t |> List.ofSeq

    /// Retrieve Box Statuses
    member __.GetBoxStatuses(id:int) =
        let retrievedBoxStatuses = allBoxStatuses.FindById(BsonValue(id))
        printfn "Caller is retrieving box statuses: %A" retrievedBoxStatuses
        retrievedBoxStatuses

let storage = Storage()
printfn "On starting up Boxes count %A" (storage.GetBoxes().Length)
printfn "On starting up AllBoxStatuses count %A" (storage.GetAllBoxStatuses().Length)

if storage.GetBoxes() |> Seq.isEmpty then
    printfn "GetBoxes returns empty so better make some"
    storage.AddBox(Box.create 1 {Lat = -41.3492; Lng = 174.7729}) |> ignore
    storage.AddBox(Box.create 2 {Lat = -41.3495; Lng = 174.7725}) |> ignore
    storage.AddBox(Box.create 3 {Lat = -41.3495; Lng = 174.7732}) |> ignore
    storage.AddBox(Box.create 4 {Lat = -41.3493; Lng = 174.7738}) |> ignore
    storage.AddBox(Box.create 5 {Lat = -41.3491; Lng = 174.7739}) |> ignore

if storage.GetAllBoxStatuses() |> Seq.isEmpty then
    printfn "GetBoxStatuses returns empty so better make some"
    storage.AddBoxStatuses(BoxStatuses.create 1 ) |> ignore
    storage.AddBoxStatuses(BoxStatuses.create 2 ) |> ignore
    storage.AddBoxStatuses(BoxStatuses.create 3 ) |> ignore
    storage.AddBoxStatuses(BoxStatuses.create 4 ) |> ignore
    storage.AddBoxStatuses(BoxStatuses.create 5 ) |> ignore

let boxesApi = {
    getBoxes = fun() -> async { return storage.GetBoxes() }

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

    addBoxStatuses =
        fun boxStatuses ->
            printfn "In API addBoxStatuses with boxStatuses %A" boxStatuses
            async {
                match storage.AddBoxStatuses boxStatuses with
                | Ok () -> printfn "Ok path"; return boxStatuses
                | Error e -> printfn "Error path"; return failwith e
            }
    updateBoxStatuses =
        fun boxStatuses ->
            async {
                match storage.UpdateBoxStatuses boxStatuses with
                | Ok () -> return boxStatuses
                | Error e -> return failwith e
            }
    dummy =
        fun () -> async { return "dummy"}
    }

let webApp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue boxesApi
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
