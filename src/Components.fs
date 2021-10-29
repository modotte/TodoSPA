module App

open Feliz
open Elmish

type TodoEntry = {
    Id: int
    Description: string
    Completed: bool
}

type Model = {
    Entry: TodoEntry
}

type Message =
| AddedEntry
| RemovedEntry

let update (message: Message) (model: Model) =
    match message with
    | AddedEntry _ -> ()
    | RemovedEntry _ -> ()

[<ReactComponent>]
let view () =
    Html.div [
        Html.h2 [ prop.text "Hello world" ]
    ]