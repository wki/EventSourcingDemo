namespace Designer.App.Pages
module HangtagClone =
    open Fable.Core
    open Fable.Import
    open Fable.Import.Fetch
    open Fable.Helpers.Fetch
    open Elmish

    // Model
    type CloneInfo = {
        oldArtNr: string
        newArtNr: string
    }
    type Model = {
        cloneInfo: CloneInfo
        state: string
        oldArtNrUpdated: bool
        oldArtNrValid: bool
        newArtNrUpdated: bool
        newArtNrValid: bool
        sent: bool
    }

    type Msg =
    | UpdateOldArtnr of string
    | UpdateNewArtnr of string
    | Post of CloneInfo
    | Posted
    | Failed of string

    let init() =
        {
            cloneInfo = { oldArtNr = ""; newArtNr = "" }
            state = "show"
            oldArtNrUpdated = false
            oldArtNrValid = false
            newArtNrUpdated = false
            newArtNrValid = false
            sent = false
        }, [] //Cmd.none
    
    // Update
    let update msg model =
        { model with state = "bla" }, Cmd.none
    
    // View
    open Fable.Helpers.React
    open Fable.Helpers.React.Props
    open Fable.Core.JsInterop
    open Designer.App.FormField

    let showForm model (dispatch: Dispatch<Msg>) =
        let updateOldArtNr = Msg.UpdateOldArtnr >> dispatch
        let updateNewArtNr = Msg.UpdateNewArtnr >> dispatch
        let formValid = model.oldArtNrValid && model.newArtNrValid
        let submitForm() = model.cloneInfo |> (Msg.Post >> dispatch)

        form [ ClassName "form-horizontal" ]
             [
                 textField "Old ArtNr" "cloneHangtagOldArtNr" "text" updateOldArtNr model.oldArtNrUpdated model.oldArtNrValid
                 textField "New ArtNr" "cloneHangtagNewArtNr" "text" updateNewArtNr model.newArtNrUpdated model.newArtNrValid

                 submitButton "Clone" submitForm formValid
             ]

    let view model (dispatch: Dispatch<Msg>) =
        div [ ClassName "component" ]
            [
                h1 [] [ unbox "Clone Hangtag" ]

                div [] [ unbox (sprintf "State: %s" model.state) ]

                (match model.sent with
                 | false -> showForm model dispatch
                 | true -> div [] [ unbox "Cloned" ])
            ]
