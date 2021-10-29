module Main

open Browser.Dom
open Elmish
open Feliz
open Feliz.UseElmish
open Fable.Core.JsInterop
open System

importSideEffects "./styles/global.scss"

type TodoEntry = {
    Id: Guid
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
| MarkEntry of Guid * bool

let init () = ({Entries = [||]; NewEntryDescription = ""}, Cmd.none)

let update message model =
    match message with
    | EntryChanged description -> 
        ({ model with NewEntryDescription = description }, Cmd.none)
    | AddedEntry ->
        let newEntry = {
            Id = Guid.NewGuid()
            Description = model.NewEntryDescription
            IsCompleted = false
        }

        ({ model with Entries = Array.append [|newEntry|] model.Entries; }, Cmd.none)
    | MarkEntry (id, isCompleted) ->
        let updateEntry entry =
            if entry.Id = id then
                { entry with IsCompleted = not isCompleted }
            else
                entry

        ({ model with Entries = Array.map updateEntry model.Entries }, Cmd.none)

[<ReactComponent>]
let view () =
    let (model, dispatch) = React.useElmish(init, update, [||])
    Html.div [
        Html.h2 [ prop.text "TodoSPA Demo" ]

        Html.div [
            Html.input [
                prop.type'.text
                prop.defaultValue model.NewEntryDescription
                prop.onTextChange (EntryChanged >> dispatch)
            ]

            Html.button [
                prop.text "+"
                prop.onClick (fun _ -> dispatch AddedEntry)
                
            ]
        ]

        Html.div [
            Html.ul (
                model.Entries
                |> Array.map (fun entry -> Html.li [ 

                        Html.div [
                            Html.label [ prop.text entry.Description ]
                            Html.br []
                            
                            match entry.IsCompleted with
                            | true -> Html.label [ prop.text "Completed" ]
                            | _ -> Html.label [ prop.text "Not Completed" ]
                            Html.br []
                            Html.button [
                                prop.text "Mark Complete"
                                prop.onClick (fun _ -> dispatch (MarkEntry (entry.Id, entry.IsCompleted)))
                            ]
                        ]
                    ])
                |> Array.toList
            )
        ]
    ]

ReactDOM.render(
    view(),
    document.getElementById "feliz-app"
)