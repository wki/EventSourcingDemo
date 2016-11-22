namespace Designer.App2
module Messages =
    open System

    [<RequireQualifiedAccess>]
    type Page =
    | Welcome
    | UserList
    | UserDetail of int

    // messages handled by application
    type AppMsg =
    | NavigateTo of Page
    | LoadUserList
    | WelcomeMsg of WelcomeMsg
    | UserListMsg of UserListMsg
    | UserDetailMsg of UserDetailMsg

    and [<RequireQualifiedAccess>] WelcomeMsg =
    | Welcome

    and [<RequireQualifiedAccess>] UserListMsg =
    | UserList

    and [<RequireQualifiedAccess>] UserDetailMsg =
    | Show
