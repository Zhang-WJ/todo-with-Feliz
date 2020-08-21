module Main

open Fable.Core.JsInterop
importAll "../styles/main.scss"

open Elmish
open Elmish.React
open Elmish.Debug
open Elmish.HMR

open Feliz
open Feliz.Router

type State = { CurrentUrl: string list }
type Msg = UrlChanged of string list

let init() = { CurrentUrl = Router.currentUrl() }

let update (UrlChanged segments) state =
    { state with CurrentUrl = segments }

let render state dispatch =
    React.router [
        router.onUrlChanged (UrlChanged >> dispatch)

        router.children [
            match state.CurrentUrl with
            | [ ] -> App.todoMVC.app
            | [ "users" ] -> Html.h1 "Users page"
            | [ "users"; Route.Int userId ] -> Html.h1 (sprintf "User ID %d" userId)
            | _ -> Html.h1 "Not found"
        ]
    ]


// App
Program.mkSimple init update render
#if DEBUG
|> Program.withDebugger
#endif
|> Program.withReactSynchronous "feliz-app"
|> Program.run