namespace Designer.App.Pages
//
// Person Detail page for a person identified by id
//
module PersonDetail =
    open Fable.Core
    open Fable.Import
    // open Fable.Import.Browser
    open Fable.Import.Fetch
    open Fable.Helpers.Fetch
    open Elmish
    // open Designer.App.HttpLoader
    // open Designer.App.Navigation

    // Model
    type Person = {
        id: int
        fullname: string
        email: string
    }
    type Model = {
        state: string // for debugging
        id: int
        person: Person option
    }
    type Msg =
    | Load of int
    | Loaded of Person
    | Failed of string

    let init personId =
        { 
            state = "initializing"
            id = personId
            person = None 
        }, Cmd.ofMsg (Load personId)
    
    // Update
    let loadPerson url =
        async {
            // let! person =
            //     fetchAs<Person>
            //         ("http://localhost:9000/api/" + url,
            //         [])
            let! result = 
                fetchAsync("http://localhost:9000/api/" + url, [])
            let! json = result.text() |> Async.AwaitPromise

            // HACK: mixin $type attribute...
            // should work with fable 0.7+ without hack
            // https://github.com/fable-compiler/Fable/issues/517
            let t = typeof<Person>
            let r = "{\"$type\": \"" + (t.FullName) + "\", "  
            let json' = json.Replace("{", r)
            let person = Fable.Core.JsInterop.ofJson<Person> json'

            return person
        }

    let update msg model =
        match msg with
        | Load personId ->
            { model with state = "loading" },
            Cmd.ofAsync loadPerson (sprintf "person/%d" personId) Msg.Loaded (fun ex -> Msg.Failed ex.Message)
        | Loaded p ->
            { model with state = "loaded"; person = Some(p) },
            []
        | Failed msg ->
            { model with state = sprintf "failed: %s" msg },
            []
    
    // View
    open Fable.Helpers.React
    open Fable.Helpers.React.Props

    let showInfo name value =
        div [ ClassName "row" ]
            [
                div [ ClassName "col-md-4" ]
                    [ b [] [ unbox name ]]
                div [ ClassName "col-md-8" ]
                    [ unbox value ]
            ]

    let showPerson (p:Person) =
        div []
            [
                h3 [] [unbox "Person Details"]
                showInfo "Id" (sprintf "%d" p.id)
                showInfo "Full name" p.fullname
                showInfo "E-Mail" p.email
            ]

    let view model (dispatch: Dispatch<Msg>) =
        div [ ClassName "component" ]
            [
                h1 [] [ unbox "Person Detail" ]
                p [] [ unbox (sprintf "Status: %s" model.state) ]
                
                (match model.person with
                    | Some person -> showPerson person
                    | None -> div [][ unbox "empty"])
            ]