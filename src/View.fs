namespace TodoSPA

open System

open Feliz
open Feliz.Bulma
open Fable.Core.JsInterop
open Fable.DateFunctions

open DomainModel

module View =
    importSideEffects "./styles/global.scss"

    type TabUrl = TabUrl of string
    type TabInfo = { IsActive: bool; Name: string; Url: TabUrl }

    let AllTab = { IsActive = false; Name = "All"; Url = TabUrl "#/" }
    let ActiveTab = { IsActive = false; Name = "Active"; Url = TabUrl "#/active" }
    let ArchivedTab = { IsActive = false; Name = "Archived"; Url = TabUrl "#/archived" }
    

    let makeDeleteButton dispatch entry =
        Bulma.button.button [
            color.isDanger
            prop.style [ style.marginRight 5]
            prop.text "Delete"
            prop.onClick (fun _ -> dispatch (RemovedEntry entry.Id))
        ]

    let makeEntryTableRow dispatch entry =
        let checkboxId = Guid.NewGuid()
        Html.tr [
            Html.td [

                let dateFormat = "dd/MM/yyyy"
                match entry.DateCompleted with
                | None -> 
                    Html.em [ prop.text ("Added at: " + entry.DateAdded.Format(dateFormat)) ]
                | Some date ->
                    Html.em [ prop.text ("Completed at: " + date.Format(dateFormat))]

                Html.br []
                Checkradio.checkbox [
                    color.isPrimary
                    prop.id (string checkboxId)
                    checkradio.isLarge
                    checkradio.isCircle
                    prop.isChecked entry.IsCompleted
                    prop.onCheckedChange (fun _ -> dispatch (MarkedEntry (entry.Id, entry.IsCompleted)))
                ]
                
                Html.label [ 
                    prop.htmlFor (string checkboxId)
                    prop.text entry.Description 
                ]
            ]

            Html.td (makeDeleteButton dispatch entry)
        ]

    let makeEntryInputArea dispatch model =
        Bulma.field.div [
            field.isGrouped
            field.isGroupedCentered

            prop.children [        
                Bulma.input.text [
                    prop.required true
                    prop.placeholder "Add a task"
                    prop.valueOrDefault model.NewEntryDescription

                    prop.onKeyUp (fun event -> if event.key = "Enter" then dispatch AddedEntry)
                    prop.onTextChange (EntryChanged >> dispatch)
                ]
                
                Bulma.button.button [
                    color.isSuccess
                    prop.onClick (fun _ -> dispatch AddedEntry)
                    prop.text "+"
                ]
            ]
        ]

    let makeTodosStateTabs model =
        let makeTab tabInfo =
            Bulma.tab [
                if tabInfo.IsActive then
                    tab.isActive
                prop.children [
                    Html.a [
                        prop.text tabInfo.Name
                        let (TabUrl url) = tabInfo.Url
                        prop.href url
                    ]
                ]
            ]
        
        Bulma.tabs [
            tabs.isCentered
            prop.children [
                match model.CurrentUrls with
                | [] ->
                    Html.ul [
                        makeTab { AllTab with IsActive = true }
                        makeTab ActiveTab
                        makeTab ArchivedTab
                    ]
                | [ "active" ] -> 
                    Html.ul [ 
                        makeTab AllTab
                        makeTab { ActiveTab with IsActive = true }
                        makeTab ArchivedTab
                    ]
                | [ "archived" ] -> 
                    Html.ul [ 
                        makeTab AllTab
                        makeTab ActiveTab
                        makeTab { ArchivedTab with IsActive = true }
                    ]
                | _ -> 
                    Html.h1 [ 
                        prop.style [ style.textAlign.center ]
                        prop.text "Tabs broken"
                    ]
            ]
        ]

    let githubForkMeRibbon =
        Html.a [
            prop.href "https://github.com/modotte/TodoSPA"
            prop.children [
                Html.img [
                    prop.classes [ "attachment-full"; "size-full" ]
                    prop.alt "Fork me on Github"
                    prop.src "https://github.blog/wp-content/uploads/2008/12/forkme_left_green_007200.png?resize=149%2C149"
                ]
            ]
        ]

    let rootContainer children = 
        Bulma.container [
            container.isFluid

            prop.children [
                githubForkMeRibbon
                children 
            ]
        ]

    let headerComponent dispatch model = 
        Html.div [
            Bulma.title [
                prop.style [ style.textAlign.center ]
                title.is2
                prop.text "TodoSPA Demo"
            ]

            makeEntryInputArea dispatch model
            makeTodosStateTabs model
            let todosLeft entries =
                entries
                |> Array.filter (fun entry -> not entry.IsCompleted)
                |> Array.length

            let todosCount = todosLeft model.Entries
            match todosCount with
            | 0 -> ()
            | _ ->
                Html.h3 [
                    prop.style [ style.textAlign.center ]
                    prop.text $"{todosCount} things left to do"
                ]
        ]

    let makeEntries dispatch entries =
        Bulma.tableContainer [
            Bulma.table [
                table.isStriped
                table.isHoverable
                table.isFullWidth
                prop.children [
                    Html.tbody (
                        entries
                        |> Array.map (fun entry -> makeEntryTableRow dispatch entry)
                        |> Array.toList
                    )
                ]
            ]
        ]

    let AllView dispatch model =
        Bulma.box [
            headerComponent dispatch model
            makeEntries dispatch model.Entries
        ] |> rootContainer

    let ActiveView dispatch model =
        Bulma.box [
            headerComponent dispatch model
            makeEntries dispatch (
                model.Entries
                // TODO: Clarify reverse boolean for activeView?
                |> Array.filter (fun entry -> not entry.IsCompleted)
            )
        ] |> rootContainer

    let ArchivedView dispatch model =
        Bulma.box [
            headerComponent dispatch model
            makeEntries dispatch (
                model.Entries
                |> Array.filter (fun entry -> entry.IsCompleted)
            )
        ] |> rootContainer
