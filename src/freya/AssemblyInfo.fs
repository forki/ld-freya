﻿namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("freya")>]
[<assembly: AssemblyProductAttribute("freya")>]
[<assembly: AssemblyDescriptionAttribute("Make tool for linked data")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
