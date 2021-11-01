namespace TodoSPA

open System

open Feliz
open Feliz.Bulma
open Fable.Core.JsInterop

open DomainModel

module View =
    importSideEffects "./styles/global.scss"

    let [<Literal>] ALL_TAB_NAME = "All"
    let [<Literal>] ACTIVE_TAB_NAME = "Active"
    let [<Literal>] ARCHIVED_TAB_NAME = "Archived"
    let [<Literal>] ALL_LINK = "#/"
    let [<Literal>] ACTIVE_LINK = "#/active"
    let [<Literal>] ARCHIVED_LINK = "#/archived"

    let makeDeleteButton dispatch entry =
        Bulma.button.button [
            color.isDanger
            prop.style [ style.marginRight 5]
            prop.text "Delete"
            prop.onClick (fun _ -> dispatch (RemovedEntry entry.Id))
        ]

    let makeEntryButtons dispatch entry =
        let checkboxId = Guid.NewGuid()
        Html.tr [
            Html.td [
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
        let makeTab isActive (name: string) link =
            Bulma.tab [
                match isActive with
                | true -> tab.isActive
                | false -> ()
                prop.children [
                    Html.a [
                        prop.text name
                        prop.href link
                    ]
                ]
            ]
        
        Bulma.tabs [
            tabs.isCentered
            prop.children [
                match model.CurrentUrls with
                | [] ->
                    Html.ul [
                        makeTab true ALL_TAB_NAME ALL_LINK
                        makeTab false ACTIVE_TAB_NAME ACTIVE_LINK
                        makeTab false ARCHIVED_TAB_NAME ARCHIVED_LINK
                    ]
                | [ "active" ] -> 
                    Html.ul [ 
                        makeTab false ALL_TAB_NAME ALL_LINK
                        makeTab true ACTIVE_TAB_NAME ACTIVE_LINK
                        makeTab false ARCHIVED_TAB_NAME ARCHIVED_LINK
                    ]
                | [ "archived" ] -> 
                    Html.ul [ 
                        makeTab false ALL_TAB_NAME ALL_LINK
                        makeTab false ACTIVE_TAB_NAME ACTIVE_LINK
                        makeTab true ARCHIVED_TAB_NAME ARCHIVED_LINK
                    ]
                | _ -> 
                    Html.h1 [ 
                        prop.style [ style.textAlign.center ]
                        prop.text "Tabs broken"
                    ]
            ]
        ]


    let rootContainer children = 
        Bulma.container [
            container.isFluid

            prop.children [ children ]
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
                        |> Array.map (fun entry -> makeEntryButtons dispatch entry)
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
