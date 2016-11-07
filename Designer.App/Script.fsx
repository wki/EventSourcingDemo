// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.
// #load "Component1.fs"
// open DesignerApp
// DesignerApp.runnable

#r "node_modules/fable-core/Fable.Core.dll"
#load "node_modules/fable-import-react/Fable.Import.React.fs"
#load "node_modules/fable-import-react/Fable.Helpers.React.fs"
#load "node_modules/fable-elmish/elmish.fs"
#load "node_modules/fable-elmish-react/elmish-app.fs"
#load "node_modules/fable-elmish-react/elmish-react.fs"

#load "Container.fs"

open Fable.Core
open Fable.Import
open Elmish

module C = Container

open Elmish.React

Program.mkSimple C.init C.update C.view
|> Program.withConsoleTrace
|> Program.toHtml Program.run "elmish-app"
