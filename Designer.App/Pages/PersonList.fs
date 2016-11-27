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
        fullname: string
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

    let renderPerson person =
        tr []
           [
               td [] [ unbox person.id ]
               td [] [ unbox person.fullname ]
               td [] [ unbox person.email ]
               td [] [ unbox "TODO: edit" ]
           ]

    let view model (dispatch:Dispatch<Msg>) =
        div [ ClassName "component"]
            [ h1 []
                 [ unbox "Person List" ]
              p []
                [ unbox (sprintf "State: %s" model.state) ]
              table [ ClassName "table table-bordered" ]
                    [
                      thead []
                            [
                               tr []
                                  [
                                      th [] [unbox "Id"]
                                      th [] [unbox "Fullname"]
                                      th [] [unbox "E-mail" ]
                                      th [] [unbox "action" ]
                                  ]
                            ]
                      tbody [] (List.map renderPerson model.persons)
                    ]
            ]
    