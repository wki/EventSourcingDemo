namespace Designer.App.Pages
//
// Welcome page
//
module Welcome =
    open Fable.Core
    open Fable.Import
    open Fable.Import.Browser
    open Elmish

    // Model
    type Model = unit
    type Msg =
    | Load
    | Loaded
    | Failed

    let init() =
        console.log("Welcome: init.")
        ()

    // Update
    let update (msg:Msg) model =
        console.log("Welcome: update, msg = ", msg)
        model, []

    // View
    open Fable.Helpers.React
    open Fable.Helpers.React.Props

    let view model (dispatch:Dispatch<Msg>) =
        div [ ClassName "jumbotron"]
            [ h1 []
                 [ unbox "Welcome" ]
              p []
                [ unbox "This is an empty welcome page" ]
            ]
    