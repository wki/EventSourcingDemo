namespace Designer.App.Pages
//
// PersonList page
//
module PersonList =
    open Fable.Core
    open Fable.Import
    open Elmish

    // Model
    type Model = unit
    type Msg =
    | Show

    let init() = ()

    // Update
    let update msg model = model

    // View
    open Fable.Helpers.React
    open Fable.Helpers.React.Props

    let view model (dispatch:Dispatch<Msg>) =
        div [ ClassName "container"]
            [ h1 []
                 [ unbox "Person" ]
              p []
                [ unbox "This is person list" ]
            ]
    