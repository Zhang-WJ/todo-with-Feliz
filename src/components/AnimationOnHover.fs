module AnimationOnHover

open Feliz
open System

let animationsOnHover' = React.functionComponent(fun (props: {| content: ReactElement |}) ->
    let (hovered, setHovered) = React.useState(false)
    Html.div [
        prop.style [
            style.width 200
            style.padding 10
            style.cursor "pointer"
            style.transitionDuration (TimeSpan.FromMilliseconds 1000.0)
            style.transitionProperty [
                transitionProperty.backgroundColor
                transitionProperty.color
            ]

            if hovered then
               style.backgroundColor.orangeRed
               style.color.black
            else
               style.backgroundColor.whiteSmoke
               style.color.black
        ]
        prop.onMouseEnter (fun _ -> setHovered(true))
        prop.onMouseLeave (fun _ -> setHovered(false))
        prop.children [ props.content ]
    ])

let animationsOnHover content = animationsOnHover' {| content = React.fragment content |}