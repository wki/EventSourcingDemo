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
    open Designer.App.Navigation

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
    | Failed of string

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

    let update msg model =
        match msg with
        | Load   -> 
            { model with state = "loading" },
            Cmd.ofAsync loadPersonList "person/list" Msg.Loaded (fun ex -> Msg.Failed ex.Message)
        | Loaded p -> 
            { model with state = "loaded"; persons = p }, 
            []
        | Failed msg -> 
            { model with state = sprintf "failed: %s" msg }, 
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
               td [] 
                  [ 
                   a [ 
                        ClassName "btn btn-default btn-xs" 
                        Href (toHash (Page.PersonDetail person.id)) 
                      ] 
                      [ unbox "details..."] ]
           ]

    let view model (dispatch: Dispatch<Msg>) =
        div [ ClassName "component"]
            [ h1 []
                 [ unbox "Person List" ]
              p []
                [ unbox (sprintf "State: %s" model.state) ]
              table [ ClassName "table table-bordered table-striped" ]
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
    