namespace Designer.App
module Menu =
    // open Fable.Core
    open Fable.Import
    open Elmish
    open Fable.Import.Browser
    open Elmish.Browser.Navigation
    open Elmish.UrlParser
    
    // JsInterop.importAll "whatwg-fetch"
    
    open Messages
    module topNav = Navigation
    
    type Model =
      { nav : topNav.Model
        page : Page
        query : string
        cache : Map<string,string list> }
    
    /// The URL is turned into a Result.
    let pageParser : Parser<Page->_,_> =
      oneOf
        [ format Home (s "home")
          format Blog (s "blog" </> i32)
          format Search (s "search" </> str) ]
    
    let hashParser (location:Location) =
      UrlParser.parse id pageParser (location.hash.Substring 1)
    
    type Msg =
      | Nav of topNav.Msg
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
    
      | Ok page ->
          { model with page = page; query = "" }, []
    
    let init result =
      urlUpdate result { nav = topNav.init(); page = Home; query = ""; cache = Map.empty }
    
    let traceUrlUpdate (result:Result<Page,string>) m = 
        console.log("UrlUpdate:", result)
        urlUpdate result m
    
    
    (* A relatively normal update function. The only notable thing here is that we
    are commanding a new URL to be added to the browser history. This changes the
    address bar and lets us use the browser&rsquo;s back button to go back to
    previous pages.
    *)
    let update msg model =
      match msg with
      | Nav cmd ->
          { model with nav = topNav.update cmd model.nav }, []
    
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
        [ topNav.view model.nav (Nav >> dispatch)

          div [ ClassName "container" ]
              [ unbox "TODO: switch page"]
        ]
    

    // App
    open Elmish.React
    
    let trace message model =
        console.log (sprintf "Message: %A" message)
        ()

    Program.mkProgram init update view
    |> Program.withTrace <| trace
    |> Program.withConsoleTrace
    |> Program.toHtml (Program.runWithNavigation hashParser traceUrlUpdate) "elmish-app"