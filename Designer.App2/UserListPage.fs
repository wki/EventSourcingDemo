namespace Designer.App2
module UserList =
    open Fable.Core
    open Fable.Arch
    open Fable.Arch.Html

    open Designer.App2.Util

    // Model
    type State =
        | Idle of string
        | Loading
        | Operating

    type Model = {
        State: State
    }

    type Actions =
        | LoadData of string
        | LoadSuccess of string // TODO: define data
        | LoadError of string

    let init url =
        {
            State = Idle url
        }

    // Update
    let update model action =
        match action with
        | Idle url 
            -> ajax (Get url)
                    (ignore) // TODO: Success
                    (ignore) // TODO: Failure
               { model with State = Loading }
        | Loading 
            -> model
        | Operating 
            -> model

    // View
    let view model =
        div []
            [text "TODO"]

