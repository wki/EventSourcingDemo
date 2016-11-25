namespace Designer.App
module Menu =
    // open Fable.Core
    open Fable.Import
    open Elmish
    open Fable.Import.Browser
    open Elmish.Browser.Navigation
    
    // JsInterop.importAll "whatwg-fetch"
    
    open Designer.App.HttpLoader
    open Designer.App.Navigation
    open Designer.App.Partials // allows to write eg "TopNav.Model"
    open Designer.App.Pages // allows to write eg "Welcome.view" 
    
    type Model = { 
        nav: TopNav.Model   // Daten der Haupt Navigation
        page: Page          // notwendig für toHash Konvertierung

        // Daten individueller Seiten. der einfachheit halber parallel zueinander
        welcome: Welcome.Model
        personList: PersonList.Model

        // beide überflüssig
        query: string
        cache: Map<string,string list> 
    }
    
    type Msg =
      | Nav of TopNav.Msg
      | Welcome of Welcome.Msg
      | PersonList of PersonList.Msg
      // | Query of string
      // | Enter
      // | FetchFailure of string*exn
      // | FetchSuccess of string*(string list)
    

    (* If the URL is valid, we just update our model or issue a command. 
    If it is not a valid URL, we modify the URL to whatever makes sense.
    *)
    let urlUpdate (result:Result<Page,string>) model =
      match result with
      | Error e ->
          Browser.console.error("Error parsing url:", e)  
          ( model, Navigation.modifyUrl (toHash model.page) )
    
      | Ok (Page.PersonList as page) ->
          console.log("parsed PersonList. initializing...")
          let p,cmds = PersonList.init()
          { model with 
                page = Page.PersonList
                personList = p
          }, cmds |> Cmd.map Msg.PersonList

      | Ok (Page.Welcome as page) ->
          console.log("parsed Welcome. initializing...")
          let w, cmds = Welcome.init()
          { model with
                page = Page.Welcome
                welcome = w
          }, cmds |> Cmd.map Msg.Welcome

      | Ok page ->
          console.log("parsed. page:", page)
          { model with page = page; query = "" }, []
    
    let init result =
      urlUpdate result { 
        nav = TopNav.init() |> fst
        page = Page.Welcome
        welcome = Welcome.init() |> fst
        personList = PersonList.init() |> fst
        query = ""
        cache = Map.empty 
      }
    
    let update (msg:Msg) model : Model * Cmd<Msg> =
      // take the result of component's update and
      // construct updated model and returned command as a tuple
      let toModelCmd updateModel msgType result =
          let x,cmds = result
          updateModel x, cmds |> Cmd.map msgType

      console.log("App: update, msg = ", msg)

      match msg with
      | Nav cmd ->
          model.nav
          |> TopNav.update cmd
          |> toModelCmd (fun n -> { model with nav = n }) Msg.Nav 
      
      | Welcome cmd ->
          model.welcome
          |> Welcome.update cmd
          |> toModelCmd (fun w -> { model with welcome = w }) Msg.Welcome

      | PersonList cmd ->
          model.personList
          |> PersonList.update cmd
          |> toModelCmd (fun p -> { model with personList=p }) Msg.PersonList

    // VIEW
    open Fable.Helpers.React
    open Fable.Helpers.React.Props
    
    let view model (dispatch: Dispatch<Msg>) =
      div []
        [ TopNav.view model.nav (Nav >> dispatch)

          div [ ClassName "container" ]
              [
                (
                  match model.page with
                  | Page.Welcome      -> Welcome.view    model.welcome    (Welcome >> dispatch)
                  | Page.PersonList   -> PersonList.view model.personList (PersonList >> dispatch)
                  | Page.Search query -> div [][ unbox "search TODO"]
                )
              ]
        ]
    

    // App
    open Elmish.React
    
    // generate debug output for url Updates
    let traceUrlUpdate (result:Result<Page,string>) m = 
        console.log("UrlUpdate:", result)
        urlUpdate result m
    
    // generate debug output of last message
    let trace message model =
        console.log (sprintf "Message: %A" message)
        ()

    Program.mkProgram init update view
    |> Program.withTrace <| trace
    |> Program.withConsoleTrace
    |> Program.toHtml (Program.runWithNavigation hashParser traceUrlUpdate) "elmish-app"
