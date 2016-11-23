namespace Designer.App.Pages
//
// Welcome page
//
module Welcome =
    open Fable.Core
    open Fable.Import
    open Elmish

    // Model
    type Model = unit
    type Msg =
    | Show

    let init() = ()

    // Update
    let update msg model =
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
    