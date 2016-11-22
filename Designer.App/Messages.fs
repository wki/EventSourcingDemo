namespace Designer.App
module Messages =

    // Types
    type Page = 
      | Home 
      | Blog of int 
      | Search of string

    let toHash = 
        function
        | Home -> "#home"
        | Blog id -> "#blog/" + (string id)
        | Search query -> "#search/" + query
