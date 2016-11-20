namespace Designer.App2

// stolen^W borrowed from https://github.com/fable-compiler/Fable
// samples/browser/react-tutorial/src/client/components.fs

module Util =
    open Fable.Core
    open Fable.Import
    open Fable.Import.Browser

    let (|NonEmpty|_|) (s: string) =
        match s.Trim() with "" -> None | s -> Some s

    type HttpMethod<'T> =
        | Get of url: string
        | Post of url: string * data: 'T
        
    let ajax meth onSuccess onError =
        let url, meth, data =
            match meth with
            | Get url -> url, "GET", None
            | Post (url, data) ->
                url, "POST", Some(JS.JSON.stringify data)
        let req = XMLHttpRequest.Create()
        req.onreadystatechange <- fun _ ->
            if req.readyState = 4. then
                match req.status with
                | 200. | 0. ->
                    JS.JSON.parse req.responseText
                    |> unbox |> onSuccess
                | _ -> onError req.status
            null
        req.``open``(meth, url, true)
        req.setRequestHeader("Content-Type", "application/json")
        req.send(data)
