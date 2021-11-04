namespace TodoSPA

open System

module DomainModel =
    type TodoId = TodoId of Guid

    type TodoEntry = {
        Id: TodoId
        Description: string
        IsCompleted: bool
        DateAdded: DateTime
        DateCompleted: DateTime option
    }

    type Model = {
        Entries: TodoEntry array
        NewEntryDescription: string
        CurrentUrls: string list
    }

    type Message =
    | Failure of string
    | EntryChanged of string
    | AddedEntry
    | MarkedEntry of TodoId * bool
    | RemovedEntry of TodoId
    | UrlsChanged of string list
    