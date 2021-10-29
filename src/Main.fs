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

let update message model =
    match message with
    | EntryChanged description -> 
        ({ model with NewEntryDescription = description }, Cmd.none)
    | AddedEntry ->
        let newEntry = {
            Id = TodoId (Guid.NewGuid())
            Description = model.NewEntryDescription
            IsCompleted = false
        }

        ({ model with Entries = Array.append [|newEntry|] model.Entries; }, Cmd.none)
    | MarkedEntry (id, isCompleted) ->
        let updateEntry entry =
            if entry.Id = id then
                { entry with IsCompleted = not isCompleted }
            else
                entry

        ({ model with Entries = Array.map updateEntry model.Entries }, Cmd.none)
    | RemovedEntry id ->
        ({ model with Entries = Array.filter (fun entry -> entry.Id <> id) model.Entries}, Cmd.none)

[<ReactComponent>]
let View () =
    let (model, dispatch) = React.useElmish(init, update, [||])
    Bulma.container [
        Html.h2 [ prop.text "TodoSPA Demo" ]

        Html.div [
            Bulma.input.text [
                prop.defaultValue model.NewEntryDescription
                prop.onTextChange (EntryChanged >> dispatch)
            ]

            Bulma.button.button [
                color.isSuccess
                prop.onClick (fun _ -> dispatch AddedEntry)
                prop.text "+"
                
            ]
        ]

        Html.div [
            Html.ul (
                model.Entries
                |> Array.map (fun entry -> Html.li [ 

                        Html.div [
                            Html.h2 [ prop.text entry.Description ]
                            Html.br []
                            
                            match entry.IsCompleted with
                            | true -> Html.label [ prop.text "Completed" ]
                            | _ -> Html.label [ prop.text "Not Completed" ]
                            Html.br []
                            Bulma.button.button [
                                color.isPrimary
                                prop.onClick (fun _ -> dispatch (MarkedEntry (entry.Id, entry.IsCompleted)))
                                prop.text "Mark Complete"
                            ]
                            Html.br []
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