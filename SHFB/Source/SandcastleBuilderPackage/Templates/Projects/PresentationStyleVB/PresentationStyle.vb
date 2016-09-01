' NOTES
' =====
' This project is a simple shell.  The folder structure in the Solution Explorer is a suggested folder
' layout that mimics the existing presentation styles in Sandcastle.  If this assembly will contain
' multiple presentation styles, group the presentation style folders into a subfolder named after the
' presentation style.
'
' Copy the files from an existing presentation style to get a head start or start from scratch by creating
' your own.  When adding files to the folders, be sure to set their Build Action property to "Content" and
' their Copy to Output Folder property to "Copy if newer".
'

Imports System
Imports System.Collections.Generic
Imports System.Reflection

Imports Sandcastle.Core
Imports Sandcastle.Core.PresentationStyle

' Search for "TODO" to find changes that you need to make to this presentation style template.

Namespace safeprojectname

    ''' <summary>
    ''' TODO: Set your presentation style's unique ID and description in the export attribute below.
    ''' </summary>
    ''' <remarks>The <c>PresentationStyleExportAttribute</c> is used to export your presentation style so
    ''' that the help file builder finds it and can make use of it.  The example below shows the basic usage
    ''' for a common presentation style.  Presentation styles are singletons in nature.  The composition
    ''' container will create instances as needed and will dispose of them when the container is disposed
    ''' of.</remarks>
    <PresentationStyleExport("$safeprojectname$", "$safeprojectname$", Version := AssemblyInfo.ProductVersion,
      Copyright := AssemblyInfo.Copyright, Description := "$safeprojectname$ custom presentation style")>
    Public NotInheritable Class safeprojectnamePresentationStyle
        Inherits PresentationStyleSettings

        ''' <inheritdoc />
        Public Overrides ReadOnly Property Location As String
            Get
                Return ComponentUtilities.AssemblyFolder(Assembly.GetExecutingAssembly())
            End Get
        End Property

        ''' <summary>
        ''' Constructor
        ''' </summary>
        Public Sub New()
            ' The base path of the presentation style files relative to the assembly's location.  If your
            ' assembly will reside in the same folder as the presentation style content, you can remove this
            ' property setting.  If adding multiple presentation styles to the assembly, set this to the name
            ' of the subfolder that contains the presentation style content folders.
            Me.BasePath = "$safeprojectname$"

            ' TODO: Adjust the rest of these properties as needed.

            Me.SupportedFormats = HelpFileFormats.HtmlHelp1 Or HelpFileFormats.MSHelpViewer Or
                HelpFileFormats.Website

            Me.SupportsNamespaceGrouping = True
            Me.SupportsCodeSnippetGrouping = True

            ' If relative, these paths are relative to the base path
            Me.ResourceItemsPath = "Content"
            Me.ToolResourceItemsPath = "SHFBContent"

            Me.DocumentModelTransformation = New TransformationFile(
                "%SHFBROOT%\ProductionTransforms\ApplyVSDocModel.xsl", New Dictionary(Of String, String) From
                {
                    { "IncludeAllMembersTopic", "false" },
                    { "project", "{@ProjectNodeIDOptional}" }
                })

            Me.IntermediateTocTransformation = New TransformationFile(
                "%SHFBROOT%\ProductionTransforms\CreateVSToc.xsl")

            Me.BuildAssemblerConfiguration = "Configuration\BuildAssembler.config"

            ' Note that UNIX based web servers may be case-sensitive with regard to folder and filenames so
            ' match the case of the folder and filenames in the literals to their actual casing on the file
            ' system.
            Me.ContentFiles.Add(New ContentFiles(Me.SupportedFormats, "icons\*.*"))
            Me.ContentFiles.Add(New ContentFiles(Me.SupportedFormats, "scripts\*.*"))
            Me.ContentFiles.Add(New ContentFiles(Me.SupportedFormats, "styles\*.*"))

            ' By default, this will use the standard web file content from the Sandcastle Help File Builder
            Me.ContentFiles.Add(New ContentFiles(HelpFileFormats.Website, Nothing, "..\LegacyWeb\*.*",
                String.Empty, New String() { ".aspx", ".html", ".htm", ".php" }))

            Me.TransformComponentArguments.Add(New TransformComponentArgument("logoFile", True, True, Nothing,
                "An optional logo file to insert into the topic headers.  Specify the filename only, omit " &
                "the path.  Place the file in your project in an icons\ folder and set the Build Action to " &
                "Content.  If blank, no logo will appear in the topic headers.  If building website output " &
                "and your web server is case-sensitive, be sure to match the case of the folder name in your " &
                "project with that of the presentation style.  The same applies to the logo filename itself."))
            Me.TransformComponentArguments.Add(New TransformComponentArgument("logoHeight", True, True, Nothing,
                "An optional logo height.  If left blank, the actual logo image height is used."))
            Me.TransformComponentArguments.Add(New TransformComponentArgument("logoWidth", True, True, Nothing,
                "An optional logo width.  If left blank, the actual logo image width is used."))
            Me.TransformComponentArguments.Add(New TransformComponentArgument("logoAltText", True, True, Nothing,
                "Optional logo alternate text.  If left blank, no alternate text is added."))
            Me.TransformComponentArguments.Add(New TransformComponentArgument("logoPlacement", True, True,
                 "left", "An optional logo placement.  Specify left, right, or above.  If not specified, the " &
                "default is left."))
            Me.TransformComponentArguments.Add(New TransformComponentArgument("logoAlignment", True, True,
                "left", "An optional logo alignment when using the 'above' placement option.  Specify left, " &
                "right, or center.  If not specified, the default is left."))
            Me.TransformComponentArguments.Add(New TransformComponentArgument("maxVersionParts", False, True,
                Nothing, "The maximum number of assembly version parts to show in API member topics.  Set to 2, " &
                "3, or 4 to limit it to 2, 3, or 4 parts or leave it blank for all parts including the " &
                "assembly file version value if specified."))
            Me.TransformComponentArguments.Add(New TransformComponentArgument("defaultLanguage", True, True,
                "cs", "The default language to use for syntax sections, code snippets, and a language-specific " &
                "text.  This should be set to cs, vb, cpp, fs, or the keyword style parameter value of a " &
                "third-party syntax generator if you want to use a non-standard language as the default."))
            Me.TransformComponentArguments.Add(New TransformComponentArgument("includeEnumValues", False, True,
                "true", "Set this to 'true' to include the column for the numeric value of each field in " &
                "enumerated type topics.  Set it to 'false' to omit the numeric values column."))
            Me.TransformComponentArguments.Add(New TransformComponentArgument("baseSourceCodeUrl", False, True,
                Nothing, "If you set the Source Code Base Path property in the Paths category, specify the URL to " &
                "the base source code folder on your project's website here.  Some examples for GitHub are " &
                "shown below." & Environment.NewLine & Environment.NewLine &
                "Important: Be sure to set the Source Code Base Path property and terminate the URL below with " &
                "a slash if necessary."  & Environment.NewLine & Environment.NewLine &
                "Format: https://github.com/YourUserID/YourProject/blob/BranchNameOrCommitHash/BaseSourcePath/" &
                Environment.NewLine &
                "Master branch: https://github.com/JohnDoe/WidgestProject/blob/master/src/" & Environment.NewLine &
                "A different branch: https://github.com/JohnDoe/WidgestProject/blob/dev-branch/src/"  & Environment.NewLine &
                "A specific commit: https://github.com/JohnDoe/WidgestProject/blob/c6e41c4fc2a4a335352d2ae8e7e85a1859751662/src/"))
            Me.TransformComponentArguments.Add(New TransformComponentArgument("requestExampleUrl", False, True,
                Nothing, "To include a link that allows users to request an example for an API topic, set the URL " &
                "to which the request will be sent.  This can be a web page URL or an e-mail URL.  Only include " &
                "the URL as the parameters will be added automatically by the topic.  For example:" &
                Environment.NewLine & Environment.NewLine &
                "Create a new issue on GitHub: https://github.com/YourUserID/YourProject/issues/new" &
                Environment.NewLine & "Send via e-mail: mailto:YourEmailAddress@Domain.com"))

            ' Add plug-in dependencies if any
            'Me.PlugInDependencies.Add(New PlugInDependency("Lightweight Website Style", Nothing))
        End Sub

    End Class

End Namespace
