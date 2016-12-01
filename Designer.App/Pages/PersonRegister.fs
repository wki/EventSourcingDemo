namespace Designer.App.Pages

module PersonRegister =
    open System.Text.RegularExpressions
    open Fable.Core
    open Fable.Import
    open Fable.Import.Fetch
    open Fable.Helpers.Fetch
    open Elmish

    // Model
    type RegisterInfo = {
        fullname: string
        email: string
    }
    type Model = {
        state: string // for debugging
        registerInfo: RegisterInfo
        emailUpdated: bool
        emailValid: bool
        fullnameUpdated: bool
        fullnameValid: bool
        sent: bool
    }

    type Msg =
    | UpdateEmail of string
    | UpdateFullname of string
    | Post of RegisterInfo
    | Posted
    | Failed of string

    let init() =
        {
            state = "show"
            registerInfo = { fullname=""; email="" }
            emailUpdated = false
            emailValid = false
            fullnameUpdated = false
            fullnameValid = false
            sent = false
        }

    // Update
    let postRegistration registerInfo =
        async {
            let! result = 
                postRecord(
                    "http://localhost:9000/api/person/register", 
                    registerInfo, 
                    [ RequestProperties.Headers [ HttpRequestHeaders.ContentType "text/json" ] ]
                )
            ()
        }

    let isEmailValid email = Regex.IsMatch(email, """^\S+@\S+[.]\S+$""")
    let isFullnameValid fullname = Regex.IsMatch(fullname, """^\S.*\S$""")

    let update msg model =
        match msg with
        | UpdateEmail email ->
            { model with 
                 state = "input"
                 registerInfo = { model.registerInfo with email = email}
                 emailUpdated = true
                 emailValid = isEmailValid email
            },
            Cmd.none
        | UpdateFullname fullname ->
            { model with 
                 state = "input"
                 registerInfo = { model.registerInfo with fullname = fullname}
                 fullnameUpdated = true 
                 fullnameValid = isFullnameValid fullname
            },
            Cmd.none
        | Post registerInfo ->
            { model with state = "post"; sent = true },
            Cmd.ofAsync postRegistration model.registerInfo (fun _ -> Msg.Posted) (fun ex -> Msg.Failed ex.Message)
        | Posted ->
            { model with state = "posted" }, Cmd.none
        | Failed msg ->
            { model with state = sprintf "failed: %s" msg }, Cmd.none

    // View
    open Fable.Helpers.React
    open Fable.Helpers.React.Props
    open Fable.Core.JsInterop

    let showForm model (dispatch: Dispatch<Msg>) =
        let (emailStatus, emailIcon) = 
            match model.emailUpdated, model.emailValid with
            | true, true -> "has-success", "glyphicon-ok"
            | true, false -> "has-error", "glyphicon-remove"
            | false, _ ->  "", ""

        let (fullnameStatus, fullnameIcon) = 
            match model.fullnameUpdated, model.fullnameValid with
            | true, true -> "has-success", "glyphicon-ok"
            | true, false -> "has-error", "glyphicon-remove"
            | false, _ ->  "", ""

        let formValid = model.emailValid && model.fullnameValid

        form [ ClassName "form-horizontal" ]
             [

                div [ ClassName (sprintf "form-group has-feedback %s" emailStatus) ]
                    [ label [ ClassName "col-sm-3 control-label"; HtmlFor "registerInputEmail" ]
                            [ unbox "Email" ]
                    
                      div [ ClassName "col-sm-6" ]
                          [ input [ 
                                    ClassName "form-control"
                                    Id "registerInputEmail"
                                    Type "email"
                                    Name "email"
                                    Placeholder "E-Mail" 
                                    OnChange ((fun (ev:React.FormEvent) -> ev.target?value) >> unbox >> UpdateEmail >> dispatch)
                                  ]
                                  []
                            span [ ClassName (sprintf "glyphicon %s form-control-feedback" emailIcon) ] []
                          ]
                    ]
                div [ ClassName (sprintf "form-group has-feedback %s" fullnameStatus) ]
                    [ label [ ClassName "col-sm-3 control-label"; HtmlFor "registerInputFullname" ]
                            [ unbox "Full name" ]
                    
                      div [ ClassName "col-sm-6" ]
                          [ input [ 
                                    ClassName "form-control"
                                    Id "registerInputFullname"
                                    Type "text" 
                                    Name "fullname"
                                    Placeholder "Full name" 
                                    OnChange ((fun (ev:React.FormEvent) -> ev.target?value) >> unbox >> UpdateFullname >> dispatch)
                                  ]
                                  []
                            span [ ClassName (sprintf "glyphicon %s form-control-feedback" fullnameIcon) ] []
                          ]
                    ]
                div [ ClassName "form-group" ]
                    [ div [ ClassName "col-sm-offset-3 col-sm-6" ]
                          [ button [
                                    Type "button"
                                    ClassName "btn btn-default"
                                    Disabled (not formValid)
                                    OnClick (fun _ -> Msg.Post model.registerInfo |> dispatch) 
                                   ]
                                   [ unbox "Register" ]
                         ]
                    ]
             ]

    let view model (dispatch: Dispatch<Msg>) =
        div [ ClassName "component" ]
            [
                h1 [] [ unbox "Register" ]

                div [] [ unbox (sprintf "State: %s" model.state) ]

                (match model.sent with
                 | false -> showForm model dispatch
                 | true -> div [] [ unbox "Registration done" ])
            ]
