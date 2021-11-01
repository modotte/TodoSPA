namespace TodoSPA

open Elmish
open Browser.WebStorage
open Thoth.Json

open DomainModel

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

    let updateStorage update (message: Message) (model: Model) = 
        let setStorage (model: Model) =
            Cmd.OfFunc.attempt save model (string >> Failure)
        
        match message with
        | Failure _ -> (model, Cmd.none)
        | _ ->
            let (newModel, commands) = update message model
            (newModel, Cmd.batch [ setStorage newModel; commands ])