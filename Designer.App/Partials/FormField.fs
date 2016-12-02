namespace Designer.App
module FormField =
    open Fable.Core
    open Fable.Import
//    open Fable.Import.Fetch
//    open Fable.Helpers.Fetch
//    open Elmish

    open Fable.Helpers.React
    open Fable.Helpers.React.Props
    open Fable.Core.JsInterop

    // various helpers for rendering form fields

    let textField description tagId fieldType (handler: string -> unit) updated valid =
        let (status, icon) = 
            match updated, valid with
            | true, true -> "has-success", "glyphicon-ok"
            | true, false -> "has-error", "glyphicon-remove"
            | false, _ ->  "", ""

        div [ ClassName (sprintf "form-group has-feedback %s" status) ]
            [ label [ ClassName "col-sm-3 control-label"; HtmlFor tagId ]
                    [ unbox description ]
            
              div [ ClassName "col-sm-6" ]
                  [ input [ 
                            ClassName "form-control"
                            Id tagId
                            Type fieldType
                            Placeholder description 
                            OnChange ((fun (ev:React.FormEvent) -> ev.target?value) >> unbox >> handler)
                          ]
                          []
                    span [ ClassName (sprintf "glyphicon %s form-control-feedback" icon) ] []
                  ]
            ]

    let submitButton description handler valid =
        div [ ClassName "form-group" ]
            [ div [ ClassName "col-sm-offset-3 col-sm-6" ]
                  [ button [
                            Type "button"
                            ClassName "btn btn-default"
                            Disabled (not valid)
                            OnClick (fun _ -> handler()) 
                           ]
                           [ unbox description ]
                 ]
            ]
