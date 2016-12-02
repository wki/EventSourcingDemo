namespace Designer.App.Pages
module HangtagCreate =
    open Fable.Core
    open Fable.Import
    open Fable.Import.Fetch
    open Fable.Helpers.Fetch
    open Elmish

    // Model
    type CreateInfo = {
        kind: string
    }
    type Model = {
        createInfo: CreateInfo
        state: string
        updated: bool
        valid: bool
        sent: bool
    }

    type Msg =
    | UpdateKind of string
    | Post of CreateInfo
    | Posted
    | Failed of string

    let init() =
        {
            createInfo = { kind = "" }
            state = "show"
            updated = false
            valid = false
            sent = false
        }, [] // Cmd.none
    
    // Update
    let update msg model =
        { model with state = "bla" }, []
    
    // View
    open Fable.Helpers.React
    open Fable.Helpers.React.Props
    open Fable.Core.JsInterop
    open Designer.App.FormField

    let showForm model (dispatch: Dispatch<Msg>) =
        let updateKind = Msg.UpdateKind >> dispatch
        let submitForm() = model.createInfo |> (Msg.Post >> dispatch)

        form [ ClassName "form-horizontal" ]
             [
                 textField "Kind" "createHangtagKind" "text" updateKind model.updated model.valid

                 submitButton "Create" submitForm model.valid
             ]

    let view model (dispatch: Dispatch<Msg>) =
        div [ ClassName "component" ]
            [
                h1 [] [ unbox "Create Hangtag" ]

                div [] [ unbox (sprintf "State: %s" model.state) ]

                (match model.sent with
                 | false -> showForm model dispatch
                 | true -> div [] [ unbox "Saved" ])
            ]
