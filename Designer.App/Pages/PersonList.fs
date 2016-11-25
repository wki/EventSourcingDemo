namespace Designer.App.Pages
//
// PersonList page
//
module PersonList =
    open Fable.Core
    open Fable.Import
    open Fable.Import.Browser
    open Elmish
    open Designer.App.HttpLoader

    // Model
    type Person = {
        firstName: string
        lastName: string
    }
    type Model = {
        state: string
        persons: Person list
    }
    type Msg =
    | Load
    | Loaded
    | Failed

    let init() =
        { 
            state = "initializing"
            persons = [] 
        }, Cmd.ofMsg Load

    // Update
    let update (msg:Msg) model =
        console.log("Welcome: update, msg = ", msg)
        match msg with
        | Load   -> 
            { model with state = "loading" },
            Cmd.ofAsync get "foo" (fun _ -> Msg.Loaded) (fun ex -> Msg.Failed)
        | Loaded -> 
            { model with state = "loaded" }, 
            []
        | Failed -> 
            { model with state = "failed" }, 
            []

    // View
    open Fable.Helpers.React
    open Fable.Helpers.React.Props

    let view model (dispatch:Dispatch<Msg>) =
        div [ ClassName "container"]
            [ h1 []
                 [ unbox "Person" ]
              p []
                [ unbox (sprintf "State: %s" model.state) ]
            ]
    