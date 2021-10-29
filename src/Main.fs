module Main

open App
open Fable.Core.JsInterop
open Elmish
open Elmish.React

importSideEffects "./styles/global.scss"

Program.mkSimple init update view
|> Program.withReactSynchronous "feliz-app"
|> Program.run