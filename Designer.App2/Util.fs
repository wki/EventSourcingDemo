namespace Designer.App2

// stolen^W borrowed from https://github.com/fable-compiler/Fable
// samples/browser/react-tutorial/src/client/components.fs

module Util =
    open Fable.Core
    open Fable.Import
    open Fable.Import.Browser

    // open Fable.Import.Fetch
    // open Fable.Helpers.Fetch

    // version based in Fetch
    // // getting data and parsing from JSON
    // async { 
    //     try 
    //         let! records = fetchAs<MyRecord[]>("http://www.server.com/data.json" , [])
    //         // ...
    //     with
    //     | error -> ...
    // } 
    // 
    // // posting data to a server
    // async { 
    //     let! response = postRecord(
    //                       "http://www.server.com/data.json", 
    //                       myRecord,
    //                       [ RequestProperties.Headers [ 
    //                           HttpRequestHeaders.Accept "application/xml" ]
    //                       ])
    // 
    //     if response.ok then
    //         match response.Headers.ContentType with
    //         | None -> // ...
    //         | Some contentType -> // ...
    // }

    // native verseion based on Brpwser.Import
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
