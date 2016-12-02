namespace Designer.App.Partials
module TopNav =
    open Fable.Core
    open Fable.Import
    open Elmish
    open Designer.App.Navigation
    
    // Model
    type Model = {
        popupVisible: bool
    }

    type Msg =
    | ShowPopup
    | HidePopup

    let init() = { popupVisible = false }, []
    
    // Update
    
    let update msg model =
      match msg with
      | ShowPopup -> { model with popupVisible = true }, []
      | HidePopup -> { model with popupVisible = false }, []
    
    // View
    open Fable.Helpers.React
    open Fable.Helpers.React.Props

    // mus ausgelagert werden!    
    let viewLink page description =
      li [ ] // TODO: OnClick (dispatch HidePopup)
         [ a [ Href (toHash page) ] [ unbox description] ]

    let view model (dispatch:Dispatch<Msg>) =
      let toggleDropdown =
          (fun _ -> dispatch (if model.popupVisible then HidePopup; else ShowPopup))

      nav [ ClassName "navbar navbar-default" ]
          [ div [ ClassName "container-fluid" ]
              [ div [ ClassName "collapse navbar-collapse" ]
                  [ ul [ ClassName "nav navbar-nav" ]
                      [ viewLink Page.Welcome "Home"
                        viewLink Page.PersonRegister "Register"
                        viewLink Page.PersonList "Persons"
                        li [ ClassName "dropdown" ]
                           [ a [ ClassName "dropdown-toggle"
                                 OnClick toggleDropdown 
                               ]
                               [ unbox "Hangtag"
                                 span [ ClassName "caret" ] []
                               ]
                             ul [ ClassName "dropdown-menu"
                                  Style [ Display (if model.popupVisible then "block" else "none") ] 
                                ]
                                [ viewLink Page.HangtagCreate "create..."
                                  viewLink Page.HangtagClone "clone..."
                                  viewLink Page.HangtagSearch "search..."
                                  viewLink Page.Welcome "ToDo"
                                ]
                           ]
                      ]
                  ]
              ]
          ]
