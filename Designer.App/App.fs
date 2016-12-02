namespace Designer.App
module Menu =
    // open Fable.Core
    open Fable.Import
    open Elmish
    open Fable.Import.Browser
    open Elmish.Browser.Navigation
    
    open Designer.App.HttpLoader
    open Designer.App.Navigation
    open Designer.App.Partials // allows to write eg "TopNav.Model"
    open Designer.App.Pages    // allows to write eg "Welcome.view" 
    
    // Model
    [<RequireQualifiedAccess>]
    type Data =
      | Empty
      | Welcome of Welcome.Model
      | PersonRegister of PersonRegister.Model
      | PersonList of PersonList.Model
      | PersonDetail of PersonDetail.Model
      | HangtagCreate of HangtagCreate.Model
      | HangtagClone of HangtagClone.Model
      | HangtagSearch of HangtagSearch.Model

    type Model = { 
        nav: TopNav.Model   // main navigation's data
        data: Data          // displayed page and its data
    }

    // [<RequireQualifiedAccess>]
    type Msg =
      | Nav of TopNav.Msg
      | Welcome of Welcome.Msg
      | PersonRegister of PersonRegister.Msg
      | PersonList of PersonList.Msg
      | PersonDetail of PersonDetail.Msg
      | HangtagCreate of HangtagCreate.Msg
      | HangtagClone of HangtagClone.Msg
      | HangtagSearch of HangtagSearch.Msg

    (* If the URL is valid, we just update our model or issue a command. 
    If it is not a valid URL, we modify the URL to whatever makes sense.
    *)
    let urlUpdate (result:Result<Page,string>) model =
      match result with
      | Error e ->
          Browser.console.error("Error parsing url:", e)  
          ( model, Navigation.modifyUrl (toHash Page.Welcome) )
    
      | Ok (Page.Welcome as page) ->
          // console.log("parsed Welcome. initializing...")
          let w, cmds = Welcome.init()
          { model with data = Data.Welcome w }, cmds |> Cmd.map Msg.Welcome
      
      | Ok (Page.PersonRegister as page) ->
          let r = PersonRegister.init()
          { model with data = Data.PersonRegister r }, Cmd.none |> Cmd.map Msg.PersonRegister

      | Ok (Page.PersonList as page) ->
          // console.log("parsed PersonList. initializing...")
          let p,cmds = PersonList.init()
          { model with data = Data.PersonList p }, cmds |> Cmd.map Msg.PersonList

      | Ok (Page.PersonDetail(personId) as page) ->
          // console.log("parsed PersonDetail. initializing...")
          let d, cmds = PersonDetail.init(personId)
          { model with data = Data.PersonDetail d }, cmds |> Cmd.map Msg.PersonDetail
    
      | Ok (Page.HangtagCreate as page) ->
          let c, cmds = HangtagCreate.init()
          { model with data = Data.HangtagCreate c }, cmds |> Cmd.map Msg.HangtagCreate

      | Ok (Page.HangtagClone as page) ->
          let c, cmds = HangtagClone.init()
          { model with data = Data.HangtagClone c }, cmds |> Cmd.map Msg.HangtagClone

      | Ok (Page.HangtagSearch as page) ->
          let s, cmds = HangtagSearch.init()
          { model with data = Data.HangtagSearch s }, cmds |> Cmd.map Msg.HangtagSearch

      | Ok page ->
          // console.log("parsed. page:", page)
          model, []

    // Init
    let init result =
      urlUpdate result { 
        nav = TopNav.init() |> fst
        data = Data.Empty
      }
    
    // Update
    let update msg model : Model * Cmd<Msg> =
      // take the result of component's update and
      // construct updated model and returned command as a tuple
      let toModelCmd updateModel msgType result =
          let x,cmds = result
          updateModel x, cmds |> Cmd.map msgType

      // update model's data with the result and return model, command tuple
      let toModelDataCmd dataType msgType result =
          toModelCmd (fun x -> { model with data = dataType x }) msgType result

      match msg with
      | Nav cmd ->
          model.nav
          |> TopNav.update cmd
          |> toModelCmd (fun n -> { model with nav = n }) Msg.Nav 
      
      | Welcome cmd ->
          match model.data with
          | Data.Welcome w ->
              w
              |> Welcome.update cmd
              |> toModelDataCmd Data.Welcome Msg.Welcome
          | _ -> model, Cmd.none

      | PersonRegister cmd ->
          match model.data with
          | Data.PersonRegister r ->
              r
              |> PersonRegister.update cmd
              |> toModelDataCmd Data.PersonRegister Msg.PersonRegister
          | _ -> model, Cmd.none
    
      | PersonList cmd ->
          match model.data with
          | Data.PersonList p ->
              p
              |> PersonList.update cmd
              |> toModelDataCmd Data.PersonList Msg.PersonList
          | _ -> model, Cmd.none

      | PersonDetail cmd ->
          match model.data with
          | Data.PersonDetail d ->
              d
              |> PersonDetail.update cmd
              |> toModelDataCmd Data.PersonDetail Msg.PersonDetail
          | _ -> model, Cmd.none

      | HangtagCreate cmd ->
         match model.data with
         | Data.HangtagCreate c ->
             c
             |> HangtagCreate.update cmd
             |> toModelDataCmd Data.HangtagCreate Msg.HangtagCreate
         | _ -> model, Cmd.none

      | HangtagClone cmd ->
         match model.data with
         | Data.HangtagClone c ->
             c
             |> HangtagClone.update cmd
             |> toModelDataCmd Data.HangtagClone Msg.HangtagClone
         | _ -> model, Cmd.none

      | HangtagSearch cmd ->
         match model.data with
         | Data.HangtagSearch s ->
             s
             |> HangtagSearch.update cmd
             |> toModelDataCmd Data.HangtagSearch Msg.HangtagSearch
         | _ -> model, Cmd.none

    // View
    open Fable.Helpers.React
    open Fable.Helpers.React.Props
    
    let view model (dispatch: Dispatch<Msg>) =
      div []
        [ TopNav.view model.nav (Nav >> dispatch)

          div [ ClassName "container" ]
              [
                (
                  match model.data with
                  | Data.Welcome w        -> Welcome.view        w (Welcome >> dispatch)
                  | Data.PersonRegister r -> PersonRegister.view r (PersonRegister >> dispatch)
                  | Data.PersonList p     -> PersonList.view     p (PersonList >> dispatch)
                  | Data.PersonDetail d   -> PersonDetail.view   d (PersonDetail >> dispatch)
                  | Data.HangtagCreate c  -> HangtagCreate.view  c (HangtagCreate >> dispatch)
                  | Data.HangtagClone c   -> HangtagClone.view   c (HangtagClone >> dispatch)
                  | Data.HangtagSearch s  -> HangtagSearch.view  s (HangtagSearch >> dispatch)
                  | _ -> div [][unbox "TODO"]
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
