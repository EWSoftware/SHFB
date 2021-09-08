Imports System.Diagnostics
Imports System.Reflection
Imports System.Xml
Imports System.Xml.XPath

Imports Sandcastle.Core.BuildAssembler
Imports Sandcastle.Core.BuildAssembler.BuildComponent

' Search for "TODO" to find changes that you need to make to this build component template.

Namespace $safeprojectname$

    ''' <summary>
    ''' TODO: Set your build component's unique ID and description in the export attribute in the factory class
    ''' below.
    ''' </summary>
    ''' <remarks>The <c>BuildComponentExportAttribute</c> is used to export your component so that the help
    ''' file builder finds it and can make use of it.  The example below shows the basic usage for a common
    ''' build component.  Multiple copies of build components can be created depending on their usage.  The
    ''' host process will create instances as needed and will dispose of them when it is done with them.</remarks>
    Public Class $safeprojectname$Component
        Inherits BuildComponentCore

        #Region "Build component factory for MEF"
        '=====================================================================

        ''' <summary>
        ''' This is used to create a new instance of the build component
        ''' </summary>
        ''' <remarks>The <c>IsVisible</c> property is typically set to true so that the component can be exposed
        ''' in configuration tools such as the Sandcastle Help File Builder.  If set to false, the component will
        ''' be hidden but can be used if referenced in a configuration file or as a dependency.</remarks>
        <BuildComponentExport("$safeprojectname$", IsVisible := true, Version := AssemblyInfo.ProductVersion,
          Copyright := AssemblyInfo.Copyright, Description := "$safeprojectname$ build component")>
        Public NotInheritable Class Factory
            Inherits BuildComponentFactory

            ''' <summary>
            ''' Constructor
            ''' </summary>
            Public Sub New()
                ' Build placement tells tools such as the Sandcastle Help File Builder how to insert the
                ' component into build configurations in projects to which it is added.

                ' TODO: Set placement for reference builds or remove if not used in reference builds
                Me.ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.Before,
                    "XSL Transform Component")

                ' TODO: Set placement for conceptual builds or remove if not used in conceptual builds
                Me.ConceptualBuildPlacement = new ComponentPlacement(PlacementAction.Before,
                    "XSL Transform Component")
            End Sub

            ''' <inheritdoc />
            Public Overrides Function Create() As BuildComponentCore
                Return New $safeprojectname$Component(Me.BuildAssembler)
            End Function

            ''' <inheritdoc />
            Public Overrides ReadOnly Property DefaultConfiguration As String
                Get
                    Return "<!-- TODO: Define your build component's default configuration here -->" & Environment.NewLine &
"<itemOne value=""Test #1"" />" & Environment.NewLine &
"<itemTwo value=""Test #2"" />"
                End Get
            End Property
        End Class

        #End Region

        #Region "Constructor"
        '=====================================================================

        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="buildAssembler">A reference to the build assembler</param>
        Protected Sub New(buildAssembler As BuildAssemblerCore)
            MyBase.New(buildAssembler)
        End Sub

        #End Region

        #Region "Abstract method implementations"
        '=====================================================================

        ''' <summary>
        ''' Initialize the build component
        ''' </summary>
        ''' <param name="configuration">The component configuration</param>
        Public Overrides Sub Initialize(configuration As XPathNavigator)
            Dim asm As Assembly = Assembly.GetExecutingAssembly()
            Dim fvi As FileVersionInfo = FileVersionInfo.GetVersionInfo(asm.Location)

            Me.WriteMessage(MessageLevel.Info, "[{0}, version {1}]\r\n    $safeprojectname$ Component.  {2}",
                fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright)

            ' TODO: Add your build component configuration code here
        End Sub

        ''' <summary>
        ''' Apply this build component's changes to the document
        ''' </summary>
        ''' <param name="document">The document to modify</param>
        ''' <param name="key">The document's key</param>
        Public Overrides Sub Apply(document As XmlDocument, key As String)
            ' TODO: Add your document modification code here.

            Me.WriteMessage(MessageLevel.Diagnostic, "In $safeprojectname$Component Apply() method")
        End Sub

        #End Region

    End Class

End Namespace
