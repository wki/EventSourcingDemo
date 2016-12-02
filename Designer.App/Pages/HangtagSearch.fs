namespace Designer.App.Pages
module HangtagSearch =
    open Fable.Core
    open Fable.Import
    open Fable.Import.Fetch
    open Fable.Helpers.Fetch
    open Elmish

    // Model
    type SearchInfo = {
        artNr: string
        name: string
        kind: string
    }
    type Model = {
        searchInfo: SearchInfo
        state: string
        sent: bool
    }

    type Msg =
    | UpdateArtNr of string
    | UpdateName of string
    | UpdateKind of string
    | Post of SearchInfo
    | Posted
    | Failed

    let init() =
        {
            searchInfo = { artNr = ""; name = ""; kind = "" }
            state = "show"
            sent = false
        }, [] // Cmd.none

    // Update
    let update msg model =
        model, []
        
    // View
    open Fable.Helpers.React
    open Fable.Helpers.React.Props
    open Fable.Core.JsInterop
    open Designer.App.FormField

    let showForm model (dispatch: Dispatch<Msg>) =
        let updateArtNr = Msg.UpdateArtNr >> dispatch
        let updateName = Msg.UpdateName >> dispatch
        let updateKind = Msg.UpdateKind >> dispatch

        let submitForm() = model.searchInfo |> (Msg.Post >> dispatch)

        form [ ClassName "form-horizontal" ]
             [
                 textField "ArtNr" "searchHangtagArtNr" "text" updateArtNr false true
                 textField "Name"  "searchHangtagName"  "text" updateName  false true
                 textField "Kind"  "searchHangtagKind"  "text" updateKind  false true
             ]
    
    let view model (dispatch: Dispatch<Msg>) =
        div [ ClassName "component" ]
            [
                h1 [] [ unbox "Search Hangtag" ]

                div [] [ unbox (sprintf "State: %s" model.state) ]

                (match model.sent with
                 | false -> showForm model dispatch
                 | true -> div [] [ unbox "Sent. TODO: show result list" ])
            ]
