module freya.Tests

open Model
open Tools
open System.Text.RegularExpressions
open FSharp.RDF
open Xunit
open Swensen.Unquote

let matchingTarget =
    { Id = Uri.from "http://nice.org.uk/ns/target1"
      ProvId = Uri.from "http://nice.org.uk/qualitystandards/resource"
      Path = Path.from "qualitystandards/standard_1/statement_23.md"
      Content = "" }

let nonMatchingTarget =
    { Id = Uri.from "http://nice.org.uk/ns/target1"
      ProvId = Uri.from "http://nice.org.uk/qualitystandards/resource"
      Path = Path.from "qualitystandards/lol/standard_23.md"
      Content = "" }

let qsCompilation = """
@prefix : <http://nice.org.uk/ns/compilation#> .
@prefix owl: <http://www.w3.org/2002/07/owl#> .
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .
@base <http://nice.org.uk/ns/compilation/> .

<http://nice.org.uk/ns/compilation/>
  rdf:type owl:Ontology ;
  owl:imports <http://nice.org.uk/ns/compilation> .


<http://nice.org.uk/ns/compilation#QualityStandards>
  rdf:type <http://nice.org.uk/ns/compilation#DirectoryPattern> ,
  owl:NamedIndividual ;
  <http://nice.org.uk/ns/compilation#expression> "qualitystandards"^^xsd:string ;
  <http://nice.org.uk/ns/compilation#parent> <http://nice.org.uk/ns/compilation#Root> .


:QualityStatement
  rdf:type <http://nice.org.uk/ns/compilation#FilePattern> ,
  owl:NamedIndividual ;
  <http://nice.org.uk/ns/compilation#expression> "statement_(?<QualityStatementId>.*).md"^^xsd:string ;
  <http://nice.org.uk/ns/compilation#tool> <http://nice.org.uk/ns/compilation#Content> ;
  <http://nice.org.uk/ns/compilation#represents> <http://nice.org.uk/ns/qualitystandard#QualityStatement>;
  <http://nice.org.uk/ns/compilation#parent> <http://nice.org.uk/ns/compilation#QualityStandard> .


<http://nice.org.uk/ns/compilation#QualityStandard>
  rdf:type <http://nice.org.uk/ns/compilation#DirectoryPattern> ,
           owl:NamedIndividual ;
  <http://nice.org.uk/ns/compilation#expression> "standard_(?<QualityStandardId>.*)"^^xsd:string ;
  <http://nice.org.uk/ns/compilation#parent> <http://nice.org.uk/ns/compilation#QualityStandards> .

"""

let g = graph.loadFormat graph.parse.ttl (graph.fromString qsCompilation)
let rp = loadMake g |> List.head

[<Fact>]
let ``Tools fail to match unless correctly configured``() =
  test <@ toolsFor nonMatchingTarget rp = None @>
let ``Match provenence entities to compilation tools``() =
  test <@ toolsFor matchingTarget rp = Some({ Target = matchingTarget
                                              Represents = (Uri.from "http://nice.org.uk/ns/qualitystandard#QualityStatement")
                                              Tools = [ Content ]
                                              Captured =
                                                [ ("QualityStandardId", "1")
                                                  ("QualityStatementId", "23") ] }) @>

let prov = """@base <http://nice.org.uk/ns/compilation#>.

@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>.
@prefix xsd: <http://www.w3.org/2001/XMLSchema#>.
@prefix prov: <http://www.w3.org/ns/prov#>.
@prefix owl: <http://www.w3.org/2002/07/owl#>.
@prefix compilation: <http://nice.org.uk/ns/compilation#>.
@prefix cnt: <http://www.w3.org/2011/content#>.
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .

<http://nice.org.uk/ns/compilation#compilation_2015-02-23T12:12:47.2583040+00:00>
  a compilation:Compilation;
  rdfs:label "Change this to a compilation message that is actualy useful to somebody";
  prov:qualifiedAssociation [a prov:Association ;
                             prov:agent <http://nice.org.uk/ns/prov#user/ryanroberts> ;
                             prov:hadRole "initiator"];
  prov:startedAtTime "2015-02-23T12:12:47.259270+00:00"^^xsd:dateTime;
  prov:uses <http://nice.org.uk/ns/prov/commit/a71586c1dfe8a71c6cbf6c129f404c5642ff31bd/new.md>;
  prov:wasAssociatedWith <http://nice.org.uk/ns/prov/user/ryanroberts>.

<http://nice.org.uk/ns/prov/commit/a71586c1dfe8a71c6cbf6c129f404c5642ff31bd/new.md>
  a prov:Entity;
  compilation:path "qualitystandards/standard_1/statement_23.md";
  cnt:chars "Some content"^^xsd:string;
  prov:specializationOf <http://nice.org.uk/ns/prov/new.md>;
  prov:wasAttributedTo <http://nice.org.uk/ns/prov/user/schacon@gmail.com>;
  prov:wasGeneratedBy <http://nice.org.uk/ns/prov/commit/c47800c>.
"""

let provM = graph.loadFormat graph.parse.ttl (graph.fromString prov) |> loadProvenance

let ``Translate provenence to compilation targets`` () =
 test <@
    {
        Id = Uri.from "http://nice.org.uk/ns/compilation#compilation_2015-02-23T12:12:47.2583040+00:00"
        Targets = [{
                Id = Uri.from "http://nice.org.uk/ns/prov#new.md"
                ProvId = Uri.from "http://nice.org.uk/ns/prov/commit/a71586c1dfe8a71c6cbf6c129f404c5642ff31bd/new.md"
                Path = Path[Segment "qualitystandards"; Segment "standard_1"; Segment "statement_23.md"];
                Content = "Some content"}
        ]
    } = provM
    @>

let res = makeAll [rp] provM.Targets
[<Fact>]
let ``Execute specified tools on compilation targets to produce ontology`` () =
  test <@
  let x = match res with | [Success({Prov=_;Output=x})] -> x
  not(List.isEmpty x)
@>