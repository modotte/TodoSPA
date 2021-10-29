module Main

open Browser.Dom
open Elmish
open Feliz
open Feliz.UseElmish
open Fable.Core.JsInterop

importSideEffects "./styles/global.scss"

type TodoEntry = {
    Id: int
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
    | AddedEntry -> (model, Cmd.none)

[<ReactComponent>]
let view () =
    let (model, dispatch) = React.useElmish(init, update, [||])
    Html.div [
        Html.h2 [ prop.text "TodoSPA Demo" ]

        Html.div [
            Html.input [
                prop.type'.text
                prop.defaultValue ""
                prop.onTextChange (EntryChanged >> dispatch)
            ]

            Html.button [
                prop.text "+"
                
            ]
        ]

        Html.div [

        ]
    ]

ReactDOM.render(
    view(),
    document.getElementById "feliz-app"
)