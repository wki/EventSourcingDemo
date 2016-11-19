namespace Designer.App2
module WelcomePage =
    open Fable.Core
    open Fable.Arch
    open Fable.Arch.Html

    // Model
    type Model = unit
    type Actions = unit
    let init() = ()

    // Update
    let update model msg = model

    // View
    let view model =
        div
            [attribute "class" "jumbotron"]
            [
                h1
                    []
                    [text "Welcome"]
                p
                    []
                    [text "This is an empty welcome page"]
            ]
