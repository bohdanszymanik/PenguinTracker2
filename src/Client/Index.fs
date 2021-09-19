module Index

open System
open Elmish
open Fable.Remoting.Client
open Feliz
open Feliz.Bulma
// open Fulma
open Leaflet
open Fable.Core.JsInterop
open Shared


type Model = {
    Boxes: Box list
    // Todos: Todo list;
    // Input: string;
    // AllBoxStatuses: BoxStatuses list
    AllBoxStatuses: Map<int, Status list>
    AdultsInput: int
    EggsInput: int
    ChicksInput: int
    DisplayPopover: bool
}

type Msg =
    // | GotTodos of Todo list
    | GotBoxes of Box list
    | GotBoxStatuses of BoxStatuses
    | GetAllBoxStatuses
    | GotAllBoxStatuses of BoxStatuses list
    // | SetInput of string
    // | AddTodo
    // | AddedTodo of Todo
    | BoxClick of int
    | SetAdultsInput of int
    | SetEggsInput of int
    | SetChicksInput of int
    | SetSubmit of int  // int marker for nest box

module RL = ReactLeaflet
importAll "../../node_modules/leaflet/dist/leaflet.css"
Leaflet.icon?Default?imagePath <- "//cdnjs.cloudflare.com/ajax/libs/leaflet/1.6.0/images/"
type RLMarker = { Info: string; Position: LatLngExpression}


// let todosApi =
//     Remoting.createApi ()
//     |> Remoting.withRouteBuilder Route.builder
//     |> Remoting.buildProxy<ITodosApi>

let boxesApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IBoxesApi>

let init () : Model * Cmd<Msg> =
    printfn "I'm in index.init"
    let model = {
        Boxes = []
        // Todos = [];
        // Input = ""
        AllBoxStatuses = Map.empty
        // BoxStatuses =
        AdultsInput = 0
        EggsInput = 0
        ChicksInput = 0
        DisplayPopover = false
    }

    let cmd = Cmd.OfAsync.perform boxesApi.getBoxes () GotBoxes
    printfn "Am i here - yes, model: %A" model
    printfn "Cmd at this point: %A" cmd
    // let cmd = Cmd.OfAsync.perform todosApi.getTodos () GotTodos
    model, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    printfn "In update with model: %A" model
    printfn "In upate with msg: %A" msg
    match msg with
    | GotBoxes boxes ->
        printfn "In GotBoxes, received boxes: %A" boxes
        // now for every box.. we want all the box statuses

        let cmd = Cmd.OfAsync.perform boxesApi.getAllBoxStatuses () GotAllBoxStatuses
        printfn "Am i here - yes, model: %A" model
        printfn "Cmd at this point: %A" cmd

        { model with Boxes = boxes }, cmd

    // // | GotTodos todos -> { model with Todos = todos }, Cmd.none
    // | SetInput value -> { model with Input = value }, Cmd.none
    // // | AddTodo ->
    // //     let todo = Todo.create model.Input

    //     let cmd =
    //         Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo

    //     { model with Input = "" }, cmd
    // | AddedTodo todo ->
    //     { model with
    //           Todos = model.Todos @ [ todo ] },
    //     Cmd.none

    | BoxClick i-> model, Cmd.none
    | SetAdultsInput i -> { model with AdultsInput = i }, Cmd.none
    | SetEggsInput i -> { model with EggsInput = i }, Cmd.none
    | SetChicksInput i -> { model with ChicksInput = i }, Cmd.none
    | SetSubmit i ->
        printfn "In SetSubmit with arg %A" i
        let status = Status.create model.AdultsInput model.EggsInput model.ChicksInput DateTime.Now
        printfn "In SetSubmit and created status %A" status
        printfn "Model AllBoxStatuses %A" model.AllBoxStatuses
        printfn "AllBoxStatuses length %A" model.AllBoxStatuses.Count
        printfn "Prepending status to %A" model.AllBoxStatuses.[i]
        let boxStatuses' = status :: model.AllBoxStatuses.[i]
        let boxStatuses = {Id = i; StatusList = boxStatuses'}
        printfn "SetSubmit created status %A" status
        let cmd =
            Cmd.OfAsync.perform boxesApi.addBoxStatuses boxStatuses (fun _ -> GetAllBoxStatuses)
        printfn "Just constructed cmd for SetSubmit"
        {model with AdultsInput = 0; EggsInput = 0; ChicksInput = 0}, cmd

    // | AddBoxStatus s ->

    | GetAllBoxStatuses ->
        model, Cmd.OfAsync.perform boxesApi.getAllBoxStatuses () GotAllBoxStatuses

    | GotAllBoxStatuses abs ->
        printfn "In GotAllBoxStatuses with list of BoxStatuses %A" abs
        // in the storage we have a list of BoxStatuses
        // but in the ui it's easier to deal with a Map<int, BoxStatuses list> so we need to transform
        let allBoxStatuses' = abs |> List.map (fun i ->  printfn "i %A" i; i.Id, i.StatusList)
        printfn "allBoxStatuses' %A" allBoxStatuses'
        let allBoxStatuses = allBoxStatuses' |> Map.ofList
        printfn "allBoxStatuses %A" allBoxStatuses
        {model with AllBoxStatuses = allBoxStatuses}, Cmd.none

    | GotBoxStatuses bs ->
        printfn "GotBoxStatuses has come in with %A" bs
        model, Cmd.none


let LINZBasemap x y z dpr =
    // sprintf "https://stamen-tiles.a.ssl.fastly.net/terrain/%A/%A/%A.png" z x y
    printfn "z:%A x:%A y:%A" z x y
    printfn "https://basemaps.linz.govt.nz/v1/tiles/aerial/EPSG:3857/%A/%A/%A.webp?api=%s" z x y "c01fdbeerst1kd0r05hkbwa2k6z"
    sprintf "https://basemaps.linz.govt.nz/v1/tiles/aerial/EPSG:3857/%A/%A/%A.webp?api=%s" z x y "c01fdbeerst1kd0r05hkbwa2k6z"

let appTitle =
  Html.p [
    prop.className "title"
    prop.text "Penguin nest box tracker"
  ]

let buildMarker (marker: RLMarker) (model: Model) (dispatch: Msg -> unit) : ReactElement =
    printfn "In buildMarker now with position %A and Info %A" marker.Position marker.Info
    RL.marker
      [
        RL.MarkerProps.Position marker.Position;
        RL.MarkerProps.Title marker.Info ]
      [
        Bulma.label marker.Info
        RL.popup
          [ RL.PopupProps.Key marker.Info; RL.PopupProps.MaxWidth 200.; RL.PopupProps.MinWidth 200.; RL.PopupProps.CloseButton true ]
          [ Bulma.field.div
              [ Html.p [ !!marker.Info ] ]
            Bulma.label (sprintf "Nest box %s" marker.Info)
            Html.form [
              Bulma.field.div [
                Bulma.label "Adults"
                Bulma.control.div [
                  Bulma.input.number [
                    prop.placeholder "0"
                    prop.valueOrDefault model.AdultsInput
                    prop.onTextChange (fun i -> (int)i |> (SetAdultsInput >> dispatch ))
                  ]
                ]
              ]
              Bulma.field.div [
                Bulma.label "Eggs"
                Bulma.control.div [
                  Bulma.input.number [
                    prop.placeholder "0"
                    prop.valueOrDefault model.EggsInput
                    prop.onTextChange (fun i -> (int)i |> (SetEggsInput >> dispatch ))
                  ]
                ]
              ]
              Bulma.field.div [
                Bulma.label "Chicks"
                Bulma.control.div [
                  Bulma.input.number [
                    prop.placeholder "0"
                    prop.valueOrDefault model.ChicksInput
                    prop.onTextChange (fun i -> (int)i |> (SetChicksInput >> dispatch ))
                  ]
                ]
              ]
              Bulma.button.button [
                prop.text "Submit"
                prop.href ""
                prop.onClick (fun e -> e.preventDefault(); dispatch (SetSubmit ((int)marker.Info)) )
              ]
            ]

         ] ]

let tile =
  RL.tileLayer
    [
      let url = sprintf "https://basemaps.linz.govt.nz/v1/tiles/aerial/EPSG:3857/{z}/{x}/{y}.webp?api=%s" "c01fc5hzfekvvz92x9z7yrwc5hr"
      RL.TileLayerProps.Url url
      RL.TileLayerProps.Attribution "&amp;<a href=&quot;https://www.linz.govt.nz/linz-copyright&quot;>LINZ CC BY 4.0</a> © <a href=&quot;https://www.linz.govt.nz/data/linz-data/linz-basemaps/data-attribution&quot;>Imagery Basemap contributors</a>"
      ]
    []

let mapBoxes (state: Model) (dispatch: Msg -> unit) =
    printfn "In mapBoxes now"
    let markers =
        state.Boxes
        |> List.map (fun b -> buildMarker { Info = (string)b.Id; Position = Fable.Core.U3.Case3(b.Location.Lat, b.Location.Lng) } state dispatch )
        // |> List.tail

    tile :: markers

let navBrand =
    Bulma.navbarBrand.div [
        Bulma.navbarItem.a [
            prop.href "https://safe-stack.github.io/"
            navbarItem.isActive
            prop.children [
                Html.img [
                    prop.src "/favicon.png"
                    prop.alt "Logo"
                ]
            ]
        ]
    ]

let containerBox (model: Model) (dispatch: Msg -> unit) =
    Bulma.box [
        // Bulma.content [
        //     Html.ol [
        //         for todo in model.Todos do
        //             Html.li [ prop.text todo.Description ]
        //     ]
        // ]
        Bulma.field.div [
            field.isGrouped
            prop.children [
            //     Bulma.control.p [
            //         prop.className  "is-expanded"
            //         prop.children [
            //             Bulma.input.text [
            //                 prop.value model.Input
            //                 prop.placeholder "What needs to be done?"
            //                 prop.onChange (SetInput >> dispatch)
            //             ]
            //         ]
            //     ]
            //     Bulma.control.p [
            //         Bulma.button.a [
            //             color.isPrimary
            //             prop.disabled (Todo.isValid model.Input |> not)
            //             prop.onClick (fun _ -> dispatch AddTodo)
            //             prop.text "Add"
            //         ]
            //     ]
            ]
        ]
    ]

let view (model: Model) (dispatch: Msg -> unit) =
    printfn "I'm in the view now"
    Bulma.hero [
        hero.isFullHeight
        // color.isPrimary
        prop.style [
            style.backgroundSize "cover"
            // style.backgroundImageUrl "https://unsplash.it/1200/900?random"
            style.backgroundPosition "no-repeat center center fixed"
        ]
        prop.children [
            Bulma.heroHead [
                Bulma.navbar [
                    Bulma.container [ navBrand ]
                ]
            ]
            Bulma.heroBody [
                Bulma.container [
                    Bulma.column [
                        column.isFull
                        // column.isOffset3
                        prop.children [
                            Bulma.title [
                                text.hasTextCentered
                                prop.text "Nest box data logger"
                            ]
                            // containerBox model dispatch

                            RL.map [
                                RL.MapProps.Animate false ;
                                RL.MapProps.Zoom 18.;
                                RL.MapProps.Tap false;
                                RL.MapProps.MaxZoom 22.;
                                RL.MapProps.Style [ Fable.React.Props.CSSProp.Height 500; Fable.React.Props.CSSProp.MinWidth 200; Fable.React.Props.CSSProp.Width Bulma.column.isFull ];
                                RL.MapProps.Center ( Fable.Core.U3.Case3 (-41.34929470266615, 174.77286007970827))  ]
                                (mapBoxes model dispatch)

                        ]
                    ]
                ]
            ]
        ]
    ]
