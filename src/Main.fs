module Main

open Browser.Dom
open Elmish
open Feliz
open Feliz.UseElmish
open Feliz.Bulma
open Fable.Core.JsInterop
open System

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
| EntryChanged of string
| AddedEntry
| MarkedEntry of TodoId * bool
| RemovedEntry of TodoId

let init () = ({Entries = [||]; NewEntryDescription = ""}, Cmd.none)

let withEntryChanged description model =
    ({ model with NewEntryDescription = description }, Cmd.none)

let withAddedEntry model =
    let newEntry = {
        Id = TodoId (Guid.NewGuid())
        Description = model.NewEntryDescription
        IsCompleted = false
    }

    ({ model with Entries = Array.append [|newEntry|] model.Entries }, Cmd.none)

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
    | EntryChanged description -> withEntryChanged description model
    | AddedEntry -> withAddedEntry model
    | MarkedEntry (id, isCompleted) -> withMarkedEntry id isCompleted model
    | RemovedEntry id -> withRemovedEntry id model

[<ReactComponent>]
let View () =
    let (model, dispatch) = React.useElmish(init, update, [||])
    Bulma.container [
        Html.h2 [ prop.text "TodoSPA Demo" ]

        Html.div [
            Bulma.field.div [
                Bulma.input.text [
                    prop.placeholder model.NewEntryDescription
                    prop.onTextChange (fun text -> dispatch (EntryChanged text))
                ]

                Bulma.button.button [
                    color.isSuccess
                    prop.onClick (fun _ -> dispatch AddedEntry)
                    prop.text "+"
                    
                ]
            ]
        ]

        Html.div [
            Html.ul (
                model.Entries
                |> Array.map (fun entry -> Html.li [ 
                        Html.div [
                            Bulma.field.div [
                                Checkradio.checkbox [
                                    prop.id "isCompletedCheckbox"
                                    color.isPrimary
                                    checkradio.isLarge
                                    
                                    prop.isChecked entry.IsCompleted
                                    prop.onCheckedChange (fun _ -> dispatch (MarkedEntry (entry.Id, entry.IsCompleted)))
                                ]
                                Html.label [
                                    prop.htmlFor "isCompletedCheckbox"
                                    prop.text entry.Description
                                ]
                            ]
                            
                            Bulma.button.button [
                                color.isDanger
                                prop.onClick (fun _ -> dispatch (RemovedEntry entry.Id))
                                prop.text "Delete"
                            ]
                        ]
                    ])
                |> Array.toList
            )
        ]
    ]

ReactDOM.render(
    View(),
    document.getElementById "feliz-app"
)