namespace Designer.App
module Menu =
    // open Fable.Core
    open Fable.Import
    open Elmish
    open Fable.Import.Browser
    open Elmish.Browser.Navigation
    
    // JsInterop.importAll "whatwg-fetch"
    
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
    
    let get query =
        async {
            let! r = Fable.Helpers.Fetch.fetchAs("http://localhost:9000/api/bla/" + query, [])
            return r
        }

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
          { model with 
                page = Page.PersonList
                personList = PersonList.init()
          }, []

      | Ok (Page.Welcome as page) ->
          console.log("parsed Welcome. initializing...")
          // Laden der Daten: in etwa so
          // Cmd.ofAsync get query (fun r -> FetchSuccess (query,r)) (fun ex -> FetchFailure (query,ex))

          { model with
                page = Page.Welcome
                welcome = Welcome.init()
          }, Cmd.ofAsync get "foo" (fun _ -> Msg.Welcome(Welcome.Loaded)) (fun ex -> Msg.Welcome(Welcome.Failed))

      | Ok page ->
          console.log("parsed. page:", page)
          { model with page = page; query = "" }, []
    
    let init result =
      urlUpdate result { 
        nav = TopNav.init()
        page = Page.Welcome
        welcome = Welcome.init()
        personList = PersonList.init()
        query = ""
        cache = Map.empty 
      }
    
    (* A relatively normal update function. The only notable thing here is that we
    are commanding a new URL to be added to the browser history. This changes the
    address bar and lets us use the browser&rsquo;s back button to go back to
    previous pages.
    *)

    let update (msg:Msg) model =
      console.log("App: update, msg = ", msg)

      match msg with
      | Nav cmd ->
          { model with nav = TopNav.update cmd model.nav }, []
      
      | Welcome cmd ->
          let w,cmds = Welcome.update cmd model.welcome
          { model with welcome = w }, cmds

      | PersonList cmd ->
          let p,cmds = PersonList.update cmd model.personList
          { model with personList = p }, cmds

    //   | Query query ->
    //       { model with query = query }, []
    
    //   | Enter ->
    //       let newPage = Search model.query
    //       { model with page = newPage }, Navigation.newUrl (toHash newPage)
    
    //   | FetchFailure (query,_) ->
    //       { model with cache = Map.add query [] model.cache }, []
    
    //   | FetchSuccess (query,locations) -> 
    //       { model with cache = Map.add query locations model.cache }, []
    
    
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
