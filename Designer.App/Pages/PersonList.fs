namespace Designer.App.Pages
//
// PersonList page
//
module PersonList =
    open Fable.Core
    open Fable.Import
    open Fable.Import.Browser
    open Fable.Import.Fetch
    open Fable.Helpers.Fetch
    open Elmish
    open Designer.App.HttpLoader

    // Model

    type Person = {
        id: int
        fullName: string
        email: string
    }
    type Model = {
        state: string
        persons: Person list
    }
    type Msg =
    | Load
    | Loaded of Person list
    | Failed

    let init() =
        { 
            state = "initializing"
            persons = [] 
        }, Cmd.ofMsg Load

    // Update
    let loadPersonList url =
        async { 
            let! personList =
                fetchAs<Person list>
                    ("http://localhost:9000/api/" + url,
                    [])
            return personList
        } 

    let update (msg:Msg) model =
        console.log("Welcome: update, msg = ", msg)
        match msg with
        | Load   -> 
            { model with state = "loading" },
            Cmd.ofAsync loadPersonList "person/list" (fun p -> Msg.Loaded p) (fun ex -> Msg.Failed)
        | Loaded p -> 
            { model with state = "loaded"; persons = p }, 
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
              div []
                  [
                      sprintf "%A" model.persons |> unbox
                    //   model.persons
                    //   |> List.map (fun p -> p.id)
                    //   |> List.fold (func acc (n,x) -> acc + "," + x) ""
                    //   |> unbox
                  ]
            ]
    