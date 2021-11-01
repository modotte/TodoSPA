module Main

open System
open Browser.Dom
open Browser.WebStorage
open Elmish
open Feliz
open Feliz.Router
open Feliz.UseElmish
open Feliz.Bulma
open Fable.Core.JsInterop
open Thoth.Json

importSideEffects "./styles/global.scss"

let [<Literal>] ALL_TAB_NAME = "All"
let [<Literal>] ACTIVE_TAB_NAME = "Active"
let [<Literal>] ARCHIVED_TAB_NAME = "Archived"
let [<Literal>] ALL_LINK = "#/"
let [<Literal>] ACTIVE_LINK = "#/active"
let [<Literal>] ARCHIVED_LINK = "#/archived"

type TodoId = TodoId of Guid

type TodoEntry = {
    Id: TodoId
    Description: string
    IsCompleted: bool
}

type Model = {
    Entries: TodoEntry array
    NewEntryDescription: string
    CurrentUrls: string list
}

type Message =
| Failure of string
| EntryChanged of string
| AddedEntry
| MarkedEntry of TodoId * bool
| RemovedEntry of TodoId
| UrlsChanged of string list

let init = function
    | Some oldModel -> (oldModel, Cmd.none)
    | _ -> ({Entries = [||]; NewEntryDescription = ""; CurrentUrls = Router.currentUrl() }, Cmd.none)

let withFailure error model =
    printfn "%A" error

    (model, Cmd.none)    

let withEntryChanged description model =
    ({ model with NewEntryDescription = description }, Cmd.none)

let withAddedEntry model =
    let newEntry =  {
        Id = TodoId (Guid.NewGuid())
        Description = model.NewEntryDescription
        IsCompleted = false
    }

    let resultEntries =
        match String.IsNullOrEmpty model.NewEntryDescription with
        | true -> model.Entries
        | _ -> Array.append [|newEntry|] model.Entries

    ({ model with Entries = resultEntries; NewEntryDescription = "" }, Cmd.none)

let withMarkedEntry id isCompleted model =
    let updateEntry entry =
        if entry.Id = id then
            { entry with IsCompleted = not isCompleted }
        else
            entry

    ({ model with Entries = Array.map updateEntry model.Entries }, Cmd.none)

let withRemovedEntry id model =
    ({ model with
        Entries = Array.filter (fun entry -> entry.Id <> id) model.Entries},
     Cmd.none)

let withUrlChanged segments model = ({ model with CurrentUrls = segments }, Cmd.none)

let update message model =
    match message with
    | Failure error -> withFailure error model
    | EntryChanged description -> withEntryChanged description model
    | AddedEntry -> withAddedEntry model
    | MarkedEntry (id, isCompleted) -> withMarkedEntry id isCompleted model
    | RemovedEntry id -> withRemovedEntry id model
    | UrlsChanged segments -> withUrlChanged segments model

module Storage =
    let private key = "modotte-todo-spa-elmish"
    let private decoder = Decode.Auto.generateDecoder<Model>()
    let load () = 
        localStorage.getItem(key)
        |> unbox
        |> Option.bind (
            Decode.fromString decoder 
            >> function
               | Ok data -> Some data
               | _ -> None)

    let save (model: Model) =
        localStorage.setItem(key, Encode.Auto.toString(1, model))

    let updateStorage (message: Message) (model: Model) = 
        let setStorage (model: Model) =
            Cmd.OfFunc.attempt save model (string >> Failure)
        
        match message with
        | Failure _ -> (model, Cmd.none)
        | _ ->
            let (newModel, commands) = update message model
            (newModel, Cmd.batch [ setStorage newModel; commands ] )

let makeDeleteButton dispatch entry =
    Bulma.button.button [
        color.isDanger
        prop.style [ style.marginRight 5]
        prop.text "Delete"
        prop.onClick (fun _ -> dispatch (RemovedEntry entry.Id))
    ]

let makeEntryButtons dispatch entry =
    let checkboxId = Guid.NewGuid()
    Html.tr [
        Html.td [
            Checkradio.checkbox [
                color.isPrimary
                prop.id (string checkboxId)
                checkradio.isLarge
                checkradio.isCircle
                prop.isChecked entry.IsCompleted
                prop.onCheckedChange (fun _ -> dispatch (MarkedEntry (entry.Id, entry.IsCompleted)))
            ]
            
            Html.label [ 
                prop.htmlFor (string checkboxId)
                prop.text entry.Description 
            ]
        ]

        Html.td (makeDeleteButton dispatch entry)
    ]

let makeEntryInputArea dispatch model =
    Bulma.field.div [
        field.isGrouped
        field.isGroupedCentered

        prop.children [        
            Bulma.input.text [
                prop.required true
                prop.placeholder "Add a task"
                prop.valueOrDefault model.NewEntryDescription
                prop.onTextChange (EntryChanged >> dispatch)
            ]
            
            Bulma.button.button [
                color.isSuccess
                prop.onClick (fun _ -> dispatch AddedEntry)
                prop.text "+"
            ]
        ]
    ]

let makeTodosStateTabs model =
    let makeTab isActive (name: string) link =
        Bulma.tab [
            match isActive with
            | true -> tab.isActive
            | false -> ()
            prop.children [
                Html.a [
                    prop.text name
                    prop.href link
                ]
            ]
        ]
    
    Bulma.tabs [
        tabs.isCentered
        prop.children [
            match model.CurrentUrls with
            | [] ->
                Html.ul [
                    makeTab true ALL_TAB_NAME ALL_LINK
                    makeTab false ACTIVE_TAB_NAME ACTIVE_LINK
                    makeTab false ARCHIVED_TAB_NAME ARCHIVED_LINK
                ]
            | [ "active" ] -> 
                Html.ul [ 
                    makeTab false ALL_TAB_NAME ALL_LINK
                    makeTab true ACTIVE_TAB_NAME ACTIVE_LINK
                    makeTab false ARCHIVED_TAB_NAME ARCHIVED_LINK
                ]
            | [ "archived" ] -> 
                Html.ul [ 
                    makeTab false ALL_TAB_NAME ALL_LINK
                    makeTab false ACTIVE_TAB_NAME ACTIVE_LINK
                    makeTab true ARCHIVED_TAB_NAME ARCHIVED_LINK
                ]
            | _ -> 
                Html.h1 [ 
                    prop.style [ style.textAlign.center ]
                    prop.text "Tabs broken"
                ]
        ]
    ]


let rootContainer children = 
    Bulma.container [
        container.isFluid

        prop.children [ children ]
    ]

let headerComponent dispatch model = 
    Html.div [
        Bulma.title [
            prop.style [ style.textAlign.center ]
            title.is2
            prop.text "TodoSPA Demo"
        ]

        makeEntryInputArea dispatch model
        makeTodosStateTabs model
    ]

let showEntries dispatch entries =
    Bulma.tableContainer [
        Bulma.table [
            table.isStriped
            table.isHoverable
            table.isFullWidth
            prop.children [
                Html.tbody (
                    entries
                    |> Array.map (fun entry -> makeEntryButtons dispatch entry)
                    |> Array.toList
                )
            ]
        ]
    ]


let AllView dispatch model =
    Bulma.box [
        headerComponent dispatch model
        showEntries dispatch model.Entries
    ] |> rootContainer

let ActiveView dispatch model =
    Bulma.box [
        headerComponent dispatch model
        showEntries dispatch (
            model.Entries
            // TODO: Clarify reverse boolean for activeView?
            |> Array.filter (fun entry -> not entry.IsCompleted)
        )
    ] |> rootContainer

let ArchivedView dispatch model =
    Bulma.box [
        headerComponent dispatch model
        showEntries dispatch (
            model.Entries
            |> Array.filter (fun entry -> entry.IsCompleted)
        )
    ] |> rootContainer

[<ReactComponent>]
let Router () =
    let (model, dispatch) = React.useElmish(Storage.load >> init, Storage.updateStorage, [||])
    React.router [
        router.onUrlChanged (UrlsChanged >> dispatch)
        router.children [
            match model.CurrentUrls with
            | [] -> AllView dispatch model
            | [ "active" ] -> ActiveView dispatch model
            | [ "archived" ] -> ArchivedView dispatch model
            | _ -> Html.h1 "Not found"
        ]
    ]

ReactDOM.render(
    Router(),
    document.getElementById "feliz-app"
)