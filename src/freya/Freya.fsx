#r "../../packages/json-ld.net/lib/net40-Client/JsonLD.dll"
#r "../../packages/Newtonsoft.Json/lib/net40/Newtonsoft.Json.dll"
#r "../../packages/VDS.Common/lib/net40-client/VDS.Common.dll"
#r "../../packages/ExtCore/lib/net40/ExtCore.dll"
#I "../../packages/FParsec/lib/net40-client"
#r "../../packages/FParsec/lib/net40-client/FParsec.dll"
#r "../../packages/FParsec/lib/net40-client/FParsecCS.dll"
#I "../../packages/FSharp.RDF/lib/"
#r "../../packages/FSharp.RDF/lib/owlapi.dll"
#r "../../packages/FSharp.RDF/lib/FSharp.RDF.dll"
#r "../../packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "../../packages/FSharp.Formatting/lib/net40/FSharp.Markdown.dll"
#I "../../packages/FSharp.RDF/lib"
#I "../../packages"
#r "../../packages/ExtCore/lib/net40/ExtCore.dll"
#r "../../packages/dotNetRDF/lib/net40/dotNetRDF.dll"
#r "../../packages/FParsec/lib/net40-client/FParsec.dll"
#r "../../packages/UnionArgParser/lib/net40/UnionArgParser.dll"
#r "../../packages/SharpYaml/lib/SharpYaml.dll"
#r "../../packages/FSharp.RDF/lib/FSharp.RDF.dll"
#r "../../packages/FSharpx.Core/lib/40/FSharpx.Core.dll"
#r "../../packages/FSharp.Collections.ParallelSeq/lib/net40/FSharp.Collections.ParallelSeq.dll"
#r "../../packages/FSharp.Compiler.Service/lib/net40/FSharp.Compiler.Service.dll"
#r "../../bin/freya.exe"
open FSharp.Data
open Newtonsoft.Json.Linq
open Freya.Builder
open FSharp.RDF
open FSharp.RDF.Assertion
open 

{
 Represents = Uri.from "http://lol"
 TargetId = Uri.from "http://lol2"
 Path = Path.from "/somewhere"
 Content = Markdown.Parse """
 ```
   Stuff
 ```
 I am a title
 ------------

 ## Quality Statement

 This is the text you want as an abstract
 
 But this isn't

 ### And really

 Not this

 ## This

 Is right out
 """}
|> (fun md ->
 ()
)
