namespace Designer.App.Pages
//
// Welcome page
//
module Welcome =
    open Fable.Core
    open Fable.Import
    open Fable.Import.Browser
    open Elmish
    open Designer.App.HttpLoader
    
    // Model
    type Model = unit
    type Msg =
    | Load
    | Loaded
    | Failed

    let init() = (), Cmd.ofMsg Load

    // Update
    let update (msg:Msg) model =
        console.log("Welcome: update, msg = ", msg)
        match msg with
        | Load   -> model, Cmd.ofAsync get "foo" (fun _ -> Msg.Loaded) (fun ex -> Msg.Failed)
        | Loaded -> model, []
        | Failed -> model, []

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
    