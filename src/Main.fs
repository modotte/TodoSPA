module Main

open System
open Browser.Dom
open Browser.WebStorage
open Elmish
open Feliz
open Feliz.UseElmish
open Feliz.Bulma
open Fable.Core.JsInterop
open Thoth.Json

importSideEffects "./styles/global.scss"

type TodoId = TodoId of Guid

type TodoEntry = {
    Id: TodoId
    Description: string
    IsCompleted: bool
}

type Model = {
    Entries: TodoEntry array
    NewEntryDescription: string
}

type Message =
| Failure of string
| EntryChanged of string
| AddedEntry
| MarkedEntry of TodoId * bool
| RemovedEntry of TodoId

let init = function
    | Some oldModel -> (oldModel, Cmd.none)
    | _ -> ({Entries = [||]; NewEntryDescription = ""}, Cmd.none)

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

    ({ model with Entries = resultEntries  }, Cmd.none)

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

let update message model =
    match message with
    | Failure error -> printfn "%s" error; (model, Cmd.none)
    | EntryChanged description -> withEntryChanged description model
    | AddedEntry -> withAddedEntry model
    | MarkedEntry (id, isCompleted) -> withMarkedEntry id isCompleted model
    | RemovedEntry id -> withRemovedEntry id model

module Storage =
    let private key = "modotte-todo-spa-elmish"
    let private decoder = Decode.Auto.generateDecoder<Model>()
    let load () = 
        localStorage.getItem(key)
        |> unbox
        |> Option.bind (
            Decode.fromString decoder 
            >> function
               | Ok x -> Some x
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
    Html.li [
        Checkradio.checkbox [
            color.isPrimary
            prop.id (string checkboxId)
            checkradio.isLarge
            prop.isChecked entry.IsCompleted
            prop.onCheckedChange (fun _ -> dispatch (MarkedEntry (entry.Id, entry.IsCompleted)))
        ]
        
        Html.label [ 
            prop.htmlFor (string checkboxId)
            prop.text entry.Description ]

        makeDeleteButton dispatch entry
    ]

[<ReactComponent>]
let View () =
    let (model, dispatch) = React.useElmish(Storage.load >> init, Storage.updateStorage, [||])

    Bulma.container [
        Html.h2 [ prop.text "TodoSPA Demo" ]

        Html.div [
            Bulma.field.div [
                Bulma.input.text [
                    prop.required true
                    prop.placeholder model.NewEntryDescription
                    prop.onTextChange (EntryChanged >> dispatch)
                ]   
            ]

            Bulma.button.button [
                color.isSuccess
                prop.onClick (fun _ -> dispatch AddedEntry)
                prop.text "+"
            ]
        ]
        

        Html.div [
            Html.br []
            Html.ul (
                model.Entries
                |> Array.map (fun entry -> makeEntryButtons dispatch entry)
                |> Array.toList
            )
        ]
    ]

ReactDOM.render(
    View(),
    document.getElementById "feliz-app"
)