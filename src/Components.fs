module App

open Feliz
open Elmish

type TodoEntry = {
    Id: int
    Description: string
    Completed: bool
}

type Model = {
    Entries: TodoEntry array
    NewEntry: string
}

type Message =
| AddedEntry
| RemovedEntry

let init () = ({Entries = [||]; NewEntry = ""}, Cmd.none)

let update message (model: Model, cmd: Cmd<'a>) =
    match message with
    | AddedEntry -> (model, Cmd.none)
    | RemovedEntry -> (model, Cmd.none)

let view model dispatch =
    Html.div [
        Html.h2 [ prop.text "Hello world" ]
    ]