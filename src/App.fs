
module App

open Feliz
open Elmish

type State = { Count: int }

type Msg =
    | Increment
    | Decrement

let init() = { Count = 0 }, Cmd.none

let update (msg: Msg) (state: State) =
    match msg with
    | Increment -> { state with Count = state.Count + 1 }, Cmd.none
    | Decrement -> { state with Count = state.Count - 1 }, Cmd.none

let render (state: State) (dispatch: Msg -> unit) =
    Html.div [
        prop.className ["counter-app"]
        prop.children [
            AnimationOnHover.animationsOnHover [
             Html.div [
                prop.onClick (fun _ -> dispatch Increment)
                prop.text "Increment"
                ]
            ]
        
            AnimationOnHover.animationsOnHover [
                 Html.div [
                    prop.onClick (fun _ -> dispatch Decrement)
                    prop.text "Decrement"
                ]
            ]

            Html.h1 state.Count
            
            App.todoMVC.app
        ]
        
    ]
    