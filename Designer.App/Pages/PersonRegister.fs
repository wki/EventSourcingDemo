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
    open Designer.App.FormField

    let showForm model (dispatch: Dispatch<Msg>) =
        let formValid = model.emailValid && model.fullnameValid

        // message handlers for input fields
        let updateEmail = Msg.UpdateEmail >> dispatch
        let updateFullname = Msg.UpdateFullname >> dispatch
        let submitForm() = model.registerInfo |> (Msg.Post >> dispatch)

        form [ ClassName "form-horizontal" ]
             [
                textField "E-Mail"   "registerInputEmail" "email" updateEmail    model.emailUpdated    model.emailValid
                textField "Fullname" "registerFullname"   "text"  updateFullname model.fullnameUpdated model.fullnameValid

                submitButton "Register" submitForm formValid
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
