Imports System
Imports System.Reflection
Imports System.Resources
Imports System.Runtime.InteropServices

' General assembly information
<Assembly: AssemblyCopyright(AssemblyInfo.Copyright)>
<Assembly: AssemblyCulture("")>

<Assembly: ComVisible(false)>

<Assembly: CLSCompliant(true)>

' Resources contained within the assembly are English
<Assembly: NeutralResourcesLanguage("en")>

<Assembly: AssemblyVersion(AssemblyInfo.ProductVersion)>
<Assembly: AssemblyFileVersion(AssemblyInfo.ProductVersion)>
<Assembly: AssemblyInformationalVersion(AssemblyInfo.ProductVersion)>

' This defines constants that can be used here and in the custom presentation style export attribute
Friend Partial Class AssemblyInfo
    ' Product version
    Public Const ProductVersion As String = "1.0.0.0"

    ' Assembly copyright information
    Public Const Copyright As String = "Copyright \xA9 $year$, $username$, All Rights Reserved."
End Class
