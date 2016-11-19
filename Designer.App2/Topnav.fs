namespace Designer.App2
module Topnav =
    open Fable.Core
    open Fable.Arch
    open Fable.Arch.Html

    // Model
    type Model = {
        Message: string
        DropdownVisible: bool
    }

    type Actions =
        | ToggleDropdown
        | DisplayDropdown
        | HideDropdown
        | ShowWelcomePage
        | ShowUserList
        | ShowUserDetail of int
    
    let init():Model = { Message = ""; DropdownVisible = false }

    // Update
    let update model msg =
        match msg with
        | ToggleDropdown
            -> { model with DropdownVisible = not model.DropdownVisible }
        | DisplayDropdown
            -> { model with DropdownVisible = true }
        | HideDropdown
            -> { model with DropdownVisible = false }
        | ShowWelcomePage
            -> { model with Message = "welcome"; DropdownVisible = false }
        | ShowUserList
            -> { model with Message = "user list"; DropdownVisible = false }
        | ShowUserDetail id 
            -> { model with Message = sprintf "user detail %d" id; DropdownVisible = false }
    
    // View
    let className c = attribute "class" c

    let navbarHeader model =
        div
            [className "navbar-header"]
            [
                div
                    [className "navbar-brand"]
                    [
                        a
                            [onMouseClick (fun e -> ShowWelcomePage)]
                            [text "Demo"]
                    ]
            ]

    let navItem2 name handler =
        li
            []
            [
                a
                    [
                        attribute "href" "#"
                        onMouseClick handler
                    ]
                    [text name]
            ]

    let navItem name =
        li
            []
            [
                a
                    [attribute "href" "#"]
                    [text name]
            ]
    
    let navbarDropdown model =
        li
            [className "dropdown"]
            [
                a
                    [
                        attribute "href" "#"
                        className "dropdown-toggle"
                        onMouseClick (fun e -> ToggleDropdown)
                    ]
                    [
                        text "Dropdown" // (sprintf "Dropdown %A" model.DropdownVisible)
                    ]
                ul
                    [
                        className "dropdown-menu"
                        Style ["display", if model.DropdownVisible then "block" else "none"]
                    ]
                    [
                        navItem2 "user list" (fun e -> ShowUserList)
                        navItem2 "user detail" (fun e -> ShowUserDetail 32)
                        navItem "bla 1"
                        navItem "bla 2"
                        navItem "bla 3"
                    ]
            ]

    let navbarBody model =
        div 
            [className "collapse navbar-collapse"]
            [
                ul
                    [className "nav navbar-nav"]
                    [
                        navItem2 "foo" (fun e -> ShowUserList)
                        navItem "bar"
                        navbarDropdown model
                        // maybe search form
                    ]
            ]

    let view model =
        nav
            [className "navbar navbar-default"]
            [
                div 
                    [className "container"] // war: container-fluid ??
                    [
                        navbarHeader model
                        navbarBody model
                    ]   
            ]
