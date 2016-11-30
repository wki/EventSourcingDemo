namespace Designer.App
module Navigation =
    open Fable.Import.Browser
    open Elmish
    open Elmish.Browser.Navigation
    open Elmish.UrlParser

    [<RequireQualifiedAccess>]
    type Page = 
      | Welcome 
      | PersonRegister
      | PersonList
      | PersonDetail of int
      | Search of string

    /// Konvertierung Seite zu Hash URL
    let toHash = 
        function
        | Page.Welcome -> "#welcome"
        | Page.PersonRegister -> "#register"
        | Page.PersonList -> "#persons"
        | Page.PersonDetail id -> sprintf "#person/%d" id
        | Page.Search query -> "#search/" + query

    let pageParser : Parser<Page->_,_> =
      oneOf
        [ format Page.Welcome (s "welcome")
          format Page.PersonRegister (s "register")
          format Page.PersonList (s "persons")
          format Page.PersonDetail (s "person" </> i32)
          format Page.Search (s "search" </> str) ]
    
    let hashParser (location:Location) =
      UrlParser.parse id pageParser (location.hash.Substring 1)
