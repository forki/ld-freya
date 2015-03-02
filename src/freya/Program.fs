open VDS.RDF
open common.RDF
open VDS.RDF.Writing
open VDS.RDF.Query
open Model

type common.RDF.Uri with
    static member fromVDS (n : INode) = 
        match n with
        | :? IUriNode as n -> Uri.Sys n.Uri
        | _ -> failwith "Not a uri node"

type Compilation with
    static member load (s : Store.store) = 
        let resultset = Store.resultset s
        let single (r : SparqlResult) = r.[0]
        let triple (r : SparqlResult) = (r.[0], r.[1], r.[2])
        { Id = 
              resultset "select ?id where {?id a compilation:Compilation}"
              |> Seq.exactlyOne
              |> single
              |> Uri.fromVDS
          Targets = 
              resultset """
                        select ?id ?chars ?path 
                        where {
                            ?cmp a compilation:Compilation .
                            ?cmp prov:uses ?id .
                            ?id cnt:chars ?chars .
                            ?id compilation:path ?path .
                        }
                        """
              |> Seq.map triple
              |> Seq.map (function 
                     | (id, chars, path) -> 
                         { Id = Uri.fromVDS id
                           Path = Path.fromStr (string path)
                           Content = string chars })
              |> Seq.toList }

open Nessos.UnionArgParser

type Arguments = 
    | CompilationOntology of string
    | Provenance of string
    interface IArgParserTemplate with
        member s.Usage = 
            match s with
            | CompilationOntology e -> "Path or url of compilation ontology"
            | Provenance p -> "Path or url to input provenance"

[<EntryPoint>]
let main argv = 
    let parser = UnionArgParser.Create<Arguments>()
    let args = parser.Parse argv
    let ont = (args.GetResult(<@ CompilationOntology @>))
    let prov = (args.GetResult(<@ Provenance @>))
    let prov = Store.loadFile prov
    Store.defaultNamespaces prov "http://nice.org.uk/ns/compilation#"
    Store.dump prov |> ignore
    prov
    |> Compilation.load
    |> printfn "%A"
    0 // return an integer exit code
