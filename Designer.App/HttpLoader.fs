namespace Designer.App
module HttpLoader =

    let get query =
        async {
            let! r = Fable.Helpers.Fetch.fetchAs("http://localhost:9000/api/bla/" + query, [])
            return r
        }
