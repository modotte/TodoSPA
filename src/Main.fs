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
    Completed: bool
}

type Model = {
    Entries: TodoEntry array
    NewEntryDescription: string
}

type Message =
| EntryChanged of string
| AddedEntry

let init () = ({Entries = [||]; NewEntryDescription = ""}, Cmd.none)

let update message model =
    match message with
    | EntryChanged description -> 
        ({ model with NewEntryDescription = description }, Cmd.none)
    | AddedEntry ->
        let newEntry = {
            Id = Guid.NewGuid()
            Description = model.NewEntryDescription
            Completed = false
        }

        ({ model with Entries = Array.append [|newEntry|] model.Entries; }, Cmd.none)

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
                |> Array.map (fun entry -> Html.li [ prop.text entry.Description ])
                |> Array.toList
            )
        ]
    ]

ReactDOM.render(
    view(),
    document.getElementById "feliz-app"
)