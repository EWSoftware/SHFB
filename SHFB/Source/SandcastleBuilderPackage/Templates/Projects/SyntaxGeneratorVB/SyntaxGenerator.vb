Imports System
Imports System.IO
Imports System.Reflection
Imports System.Xml.XPath

Imports Sandcastle.Core.BuildAssembler.SyntaxGenerator

' Search for "TODO" to find changes that you need to make to this syntax generator template.

Namespace $safeprojectname$

    ''' <summary>
    ''' TODO: Set your syntax generator's unique ID and description in the export attribute in the factory class
    ''' below.
    ''' </summary>
    ''' <remarks>The <c>SyntaxGeneratorExportAttribute</c> is used to export your syntax generator so that the
    ''' help file builder finds it and can make use of it.  The example below shows the basic usage for a common
    ''' syntax generator.  Multiple copies of syntax generators can be created depending on their usage.  The
    ''' host process will create instances as needed.
    ''' 
    ''' <para>NOTE: For non-languages, a simpler base type, <c>SyntaxGeneratorCore</c>, may be used.  An example
    ''' of its use can be found in the <c>AspNetSyntaxGenerator</c>.</para></remarks>
    public class $safeprojectname$DeclarationSyntaxGenerator
        Inherits SyntaxGeneratorTemplate

        #Region "Syntax generator factory for MEF"
        '=====================================================================

        ' TODO: Set the unique style ID name.
        ' NOTE: If you change LanguageName, rename the SyntaxContent\$safeprojectname$.xml file to the same name
        '       and update the IDs of the content items in it with the new name.
        Private Const LanguageName As String = "$safeprojectname$"
        Private Const StyleIdName As String = "xyz"

        ''' <summary>
        ''' This is used to create a new instance of the syntax generator
        ''' </summary>
        ''' <remarks>The <c>keywordStyleParameter</c> parameter is used to set the keyword style in the
        ''' presentation style and should be unique to your programming language.  Set the additional attributes
        ''' as needed:
        '''
        ''' <list type="bullet">
        '''     <item>
        '''         <term>AlternateIds</term>
        '''         <description>Specify a comma-separated list of other language names that can be mapped to
        ''' this generator.</description>
        '''     </item>
        '''     <item>
        '''         <term>IsConfigurable</term>
        '''         <description>Set this to true if your syntax generator contains configurable settings.
        ''' Designers can use the <c>DefaultConfiguration</c> property to obtain the default configuration.</description>
        '''     </item>
        '''     <item>
        '''         <term>DefaultConfiguration</term>
        '''         <description>If your syntax generator has configurable settings, use this property to specify
        ''' the default settings in an XML fragment.</description>
        '''     </item>
        ''' </list>
        ''' </remarks>
        <SyntaxGeneratorExport("$safeprojectname$", "$safeprojectname$", "cs", SortOrder := 500,
          Version := AssemblyInfo.ProductVersion, Copyright := AssemblyInfo.Copyright,
          Description := "Generates $safeprojectname$ declaration syntax sections")>
        Public NotInheritable Class Factory
            Implements ISyntaxGeneratorFactory

            ''' <inheritdoc />
            Public ReadOnly Property ResourceItemFileLocation As String
                Get
                    Return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SyntaxContent")
                End Get
            End Property

            ''' <inheritdoc />
            Public Function Create() As SyntaxGeneratorCore Implements ISyntaxGeneratorFactory.Create
                Return New $safeprojectname$DeclarationSyntaxGenerator With { .Language = LanguageName, .StyleId = StyleIdName }
            End Function
        End Class
        #End Region

        #Region "Abstract method implementations"
        '=====================================================================

        ' TODO: Each of the following methods must be implemented.  Syntax generation is rather complex.
        ' It may be best to copy one of the existing syntax generators if the language is a close match
        ' for the one you are trying to implement.

        ''' <inheritdoc />
        Public Overrides Sub WriteClassSyntax(reflection As XPathNavigator, writer As SyntaxWriter)
            Throw New System.NotImplementedException()
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub WriteConstructorSyntax(reflection As XPathNavigator, writer As SyntaxWriter)
            Throw New System.NotImplementedException()
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub WriteDelegateSyntax(reflection As XPathNavigator, writer As SyntaxWriter)
            Throw New System.NotImplementedException()
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub WriteEnumerationSyntax(reflection As XPathNavigator, writer As SyntaxWriter)
            Throw New System.NotImplementedException()
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub WriteEventSyntax(reflection As XPathNavigator, writer As SyntaxWriter)
            Throw New System.NotImplementedException()
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub WriteFieldSyntax(reflection As XPathNavigator, writer As SyntaxWriter)
            Throw New System.NotImplementedException()
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub WriteInterfaceSyntax(reflection As XPathNavigator, writer As SyntaxWriter)
            Throw New System.NotImplementedException()
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub WriteNamespaceSyntax(reflection As XPathNavigator, writer As SyntaxWriter)
            Throw New System.NotImplementedException()
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub WritePropertySyntax(reflection As XPathNavigator, writer As SyntaxWriter)
            Throw New System.NotImplementedException()
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub WriteStructureSyntax(reflection As XPathNavigator, writer As SyntaxWriter)
            Throw New System.NotImplementedException()
        End Sub
        #End Region

    End Class

End Namespace
