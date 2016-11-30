namespace Designer.App.Pages

module PersonRegister =
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
            sent = false
        }

    // Update
    let postRegistration registerInfo =
        async {
            let! result = postRecord("http://localhost:9000/api/register", registerInfo, [])
            ()
        }

    let update msg model =
        match msg with
        | UpdateEmail email ->
            { model with state = "input"; registerInfo = { model.registerInfo with email = email} },
            Cmd.none
        | UpdateFullname fullname ->
            { model with state = "input"; registerInfo = { model.registerInfo with fullname = fullname} },
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

    let showForm (dispatch: Dispatch<Msg>) =
        form [ ClassName "form-horizontal" ]
             [
                div [ ClassName "form-group" ]
                    [
                        label [ ClassName "col-sm-3 control-label"; HtmlFor "registerInputEmail" ]
                              [ unbox "Email" ]
                    
                        input [ 
                                ClassName "col-sm-6 form-control"
                                Id "registerInputEmail"
                                Type "email"
                                Name "email"
                                OnChange ((fun (ev:React.FormEvent) -> ev.target?value) >> unbox >> UpdateEmail >> dispatch)
                              ]
                              []
                    ]
                div [ ClassName "form-group" ]
                    [
                        label [ ClassName "col-sm-3 control-label"; HtmlFor "registerInputFullname" ]
                              [ unbox "Email" ]
                    
                        input [ 
                                ClassName "col-sm-6 form-control"
                                Id "registerInputFullname"
                                Type "text" 
                                Name "fullname"
                              ]
                              []
                    ]
                div [ ClassName "form-group" ]
                    [
                        div [ ClassName "col-sm-offset-3 col-sm-6" ]
                            [
                                button [
                                        Type "button"
                                        ClassName "btn btn-default"
                                       ]
                                       []
                            ]
                    ]
             ]

    let view model (dispatch: Dispatch<Msg>) =
        div [ ClassName "component" ]
            [
                h1 [] [ unbox "Register" ]

                div [] [ unbox sprintf "State: %s" model.state ]

                (match model.sent with
                 | false -> showForm dispatch
                 | true -> div [] [ unbox "Registration done" ])
            ]
