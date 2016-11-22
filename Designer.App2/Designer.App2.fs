namespace Designer.App2
module Menu =
    open Fable.Core
    open Fable.Core.JsInterop
    
    open Fable.Arch
    open Fable.Arch.App
    open Fable.Arch.Html
    
    open Topnav
    open WelcomePage
    // open UserListPage
    // open UserDetailPage

    // Model
    type Page = // Idee: jeder Fall kapselt die Model-Daten der jeweiligen Seite
        | WelcomePage of WelcomePage.Model
        | UserListPage // of UserListPage.Model
        | UserDetailPage of int // of UserDetailPage.Model
    
    type Model = {
        Nav: Topnav.Model
        Input: string
        Page: Page
    }
    
    // use Messages.AppMsg instead!
    type Actions =
        | ChangeInput of string
        | Nav of Topnav.Actions
    
    let init() = { 
        Nav = Topnav.init()
        Input="" 
        Page = WelcomePage(WelcomePage.init())
    }

    // Update
    let forwardToNav navMsg model =
        { model with Nav = Topnav.update model.Nav navMsg }

    let update model msg =
        match msg with
        | ChangeInput str -> {model with Input=str}, []
        | Nav navMsg ->
            match navMsg with
            | ShowWelcomePage
                -> { model with Input = "welcome"; Page = WelcomePage(WelcomePage.init()) }
            | ShowUserList 
                -> { model with Input = "user list"; Page = UserListPage }
            | ShowUserDetail id 
                -> { model with Input = sprintf "user detail %d" id; Page = UserDetailPage id }
            | _ -> { model with Input = sprintf "%A" nav }
            |> forwardToNav navMsg, []
    
    // View
    let showPage model =
        match model.Page with
        | WelcomePage welcome
            -> Html.map id (WelcomePage.view welcome)
        | UserListPage      
            -> div [][text "TODO: user list"]
        | UserDetailPage id 
            -> div [][text (sprintf "TODO: user detail %d" id)]

    let view model =
        div
            []
            [
                Html.map Nav (Topnav.view model.Nav)
                
                hr []

                label 
                    []
                    [text "Enter name: "]
                input
                    [
                        onInput (fun x -> ChangeInput (unbox x?target?value))
                    ]
                br []
                span
                    []
                    [text (sprintf "Hello %s" model.Input)]
                
                hr []

                div
                    [className "container"]
                    [
                        // div [] [text (sprintf "page: %A" model.Page)]

                        showPage model
                    ]
            ]
    
    // Using createSimpleApp instead of createApp since our
    // update function doesn't generate any actions. See 
    // some of the other more advanced examples for how to
    // use createApp. In addition to the application functions
    // we also need to specify which renderer to use.

    open Fable.Import.Browser

    // SimpleApp: update returns Model
    // createSimpleApp (init()) view update Virtualdom.createRender

    // App: update returns Model * Action list 
    createApp (init()) view update Virtualdom.createRender
    |> withStartNodeSelector "#hello"
    |> withSubscriber (fun e -> window.console.log("Something happened: ", e))
    |> start
    |> ignore
