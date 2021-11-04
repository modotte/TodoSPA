namespace TodoSPA

open System
open Browser.Dom
open Elmish
open Feliz
open Feliz.Router
open Feliz.UseElmish
open Fable.DateFunctions

open DomainModel

module Main =
    let init = function
        | Some oldModel -> (oldModel, Cmd.none)
        | _ -> ({Entries = [||]; NewEntryDescription = ""; CurrentUrls = Router.currentUrl() }, Cmd.none)

    // TODO: Code smell. Put this implementation out of the update core.
    // We don't want any cheap side effects in it.
    // An idea would be to put this in its own logging module,
    // and compose it down to actually reach the logging routine
    // out of this pure state handler core.
    let withFailure error model =
        printfn "%A" error

        (model, Cmd.none)    

    let withEntryDescriptionChanged description model =
        ({ model with NewEntryDescription = description }, Cmd.none)

    let withAddedEntry model =
        let newEntry =  {
            Id = TodoId (Guid.NewGuid())
            Description = model.NewEntryDescription
            IsCompleted = false
            DateAdded = DateTime.Now
            DateCompleted = None
        }

        let resultEntries =
            if String.IsNullOrEmpty model.NewEntryDescription then
                model.Entries
            else
                Array.append [|newEntry|] model.Entries

        ({ model with Entries = resultEntries; NewEntryDescription = "" }, Cmd.none)

    let withMarkedEntry id isCompleted model =
        let updateEntry entry =
            if entry.Id = id then
                { entry with 
                    IsCompleted = not isCompleted
                    DateCompleted = Some DateTime.Now
                }
            else
                entry

        ({ model with 
            Entries = Array.map updateEntry model.Entries
         }, Cmd.none)

    let withRemovedEntry id model =
        ({ model with
            Entries = Array.filter (fun entry -> entry.Id <> id) model.Entries},
         Cmd.none)

    let withUrlsChanged segments model = ({ model with CurrentUrls = segments }, Cmd.none)

    let update message model =
        match message with
        | Failure error -> withFailure error model
        | EntryChanged description -> withEntryDescriptionChanged description model
        | AddedEntry -> withAddedEntry model
        | MarkedEntry (id, isCompleted) -> withMarkedEntry id isCompleted model
        | RemovedEntry id -> withRemovedEntry id model
        | UrlsChanged segments -> withUrlsChanged segments model

    [<ReactComponent>]
    let Router () =
        let (model, dispatch) = React.useElmish(Storage.load >> init, Storage.updateStorage update, [||])
        React.router [
            router.onUrlChanged (UrlsChanged >> dispatch)
            router.children [
                match model.CurrentUrls with
                | [] -> View.AllView dispatch model
                | [ "active" ] -> View.ActiveView dispatch model
                | [ "archived" ] -> View.ArchivedView dispatch model
                | _ -> Html.h1 "Not found"
            ]
        ]

    ReactDOM.render(
        Router(),
        document.getElementById "feliz-app"
    )