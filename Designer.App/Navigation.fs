namespace Designer.App
module Navigation =
    open Fable.Core
    open Fable.Import
    open Elmish
    open Messages
    
    // Model
    type Model = {
        popupVisible: bool
    }
    
    let init() = {
      popupVisible = false
    }
    
    // Update
    type Msg =
      | ShowPopup
      | HidePopup
    
    let update msg model =
      match msg with
      | ShowPopup -> { model with popupVisible = true }
      | HidePopup -> { model with popupVisible = false }
    
    // View
    open Fable.Helpers.React
    open Fable.Helpers.React.Props

    // mus ausgelagert werden!    
    let viewLink page description =
      li []
         [ a [ Href (toHash page) ] [ unbox description] ]

    let view model (dispatch:Dispatch<Msg>) =
      let toggleDropdown =
          (fun _ -> dispatch (if model.popupVisible then HidePopup; else ShowPopup))

      nav [ ClassName "navbar navbar-default" ]
          [ div [ ClassName "container-fluid" ]
              [ div [ ClassName "collapse navbar-collapse" ]
                  [ ul [ ClassName "nav navbar-nav" ]
                      [ viewLink Home "Home"
                        viewLink (Blog 42) "Mouse Facts"
                        viewLink (Blog 26) "Workout Plan"
                        li [ ClassName "dropdown" ]
                           [ a [ ClassName "dropdown-toggle"
                                 OnClick toggleDropdown 
                               ]
                               [ unbox "Dropdown"
                                 span [ ClassName "caret" ] []
                               ]
                             ul [ ClassName "dropdown-menu"
                                  Style [ Display (if model.popupVisible then "block" else "none") ] 
                                ]
                                [ viewLink Home "Home 3"
                                  viewLink Home "Home 4"
                                  viewLink Home "Home 5"
                                ]
                           ]
                      ]
                  ]
              ]
          ]
