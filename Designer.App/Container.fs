module Container
open Elmish

// Model
type Model =
    { Foo: int }

let init() = { Foo = 1 }, Cmd.none

// Update
type Msg =
    | Reset
    | Something

let update msg model : Model*Cmd<Msg> =
    { Foo = 1 }, []

// View
module R = Fable.Helpers.React
open Fable.Helpers.React.Props

let view model (dispatch: Dispatch<Msg>) =
    R.div [ ClassName "asdf" ] [ unbox "hello world" ]
    // [ "dsaf" ]
