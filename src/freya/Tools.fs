namespace Freya

open FSharp.RDF
open FSharp.Markdown
open Assertion
open rdf
open owl
open Tracing
open YamlParser
open ExtCore
open ExtCore.Collections
open System

module Tools =
  let either =
    function
    | ToolExecution.Failure x -> x
    | ToolExecution.Success x -> x

  let step f (PipelineStep(m, xp)) =
    let xr =
      xp
      |> List.map either
      |> List.collect (fun { Provenance = _; Extracted = r } -> r)

    let res = f m xr
    PipelineStep(m, res :: xp)

  let private contentS t m _ =
    pipeline.succeed (semanticExtraction m t [])
      [owl.individual m.Target.Id [ m.Represents ]
          [dataProperty !!"http://www.w3.org/2011/content#chars" ((snd m.Target.Content) ^^ xsd.string) ] ]

  let content = step << contentS

  type YNode = Freya.YamlParser.Node

  let yamlMetadataS t m _ =
    let translate = function
      | YNode.Map xs ->
        [ for (prefix, YNode.Map xs') in xs do
            for (property, List xs'') in xs' do
              for (Scalar n) in xs'' do
                let predicate = P(Uri.from(sprintf "%s:%s" prefix property))
                match n with
                | Scalar.String s ->
                  yield (predicate, O(Node.Literal(Literal.String s), lazy []))
                | Scalar.Uri u -> yield (predicate, O(Node.from u, lazy [])) ]

    let yamlToStatements y =
      try
        pipeline.succeed (semanticExtraction m t [])
          [ rdf.resource m.Target.Id (translate (parse y)) ]
      with e ->
        pipeline.succeed
          (semanticExtraction m t
             [ warn (sprintf "Failed to parse yaml: %s" e.Message)
                 (fileLocation m.Target.Path) ]) []

    let md = Markdown.Parse(snd m.Target.Content)
    match md.Paragraphs with
    | CodeBlock(yaml, _, _) :: _ -> yamlToStatements yaml
    | _ ->
      pipeline.succeed
        (semanticExtraction m t
           [ warn "No metadata block at start of file"
               (fileLocation m.Target.Path) ]) []

  let yamlMetadata = step << yamlMetadataS

  open Pandoc

  let convertMarkdownS t m xr =
    match xr with
    | r :: xr ->
      let prov = convertResources r [] t
      pipeline.succeed prov [ r ]
    | [] -> pipeline.fail []

  let convertMarkdown x t =
    step (convertMarkdownS (x,
                            { Output = Path.from "/artifacts/work"
                              ToolMatch = t
                              WorkingDir = Path.from "." }))

  let exec tm t =
    match t with
    | SemanticExtractor(Content) -> content t
    | SemanticExtractor(YamlMetadata) -> yamlMetadata t
    | KnowledgeBaseProcessor(MarkdownConvertor x) -> convertMarkdown x tm

  //Produce a single PipelineStep -> PipelineStep from the tool list to apply to the source, this way they
  //will execute sequentially as part of a parallel sequence
  let execMatches x =
    (x.Tools
     |> List.map (exec x)
     |> List.reduce (>>)) (PipelineStep(x, []))

  let make xrp t =
    xrp
    |> List.map (Target.toolsFor t)
    |> List.choose id
    |> List.map execMatches

  open FSharp.Collections.ParallelSeq

  let makeAll xrp xt =
    printfn "%d prov entities for compilation using %d tools" (Seq.length xt) (List.length xrp)
    PSeq.collect (make xrp) xt
    |> PSeq.map pipeline.result
