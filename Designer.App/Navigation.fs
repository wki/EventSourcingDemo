module Designer.App.Navigation

open Fable.Core
open Fable.Import
open Elmish

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

let view model (dispatch:Dispatch<Msg>) =
  // let onClick msg =
  //   fun _ -> msg |> dispatch   

  div [ OnClick (fun _ -> dispatch (if model.popupVisible then HidePopup; else ShowPopup)) ] 
      [ unbox (sprintf "%O" model.popupVisible)]
