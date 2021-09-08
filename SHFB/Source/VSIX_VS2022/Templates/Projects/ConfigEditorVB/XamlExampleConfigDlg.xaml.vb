﻿' TODO: Include the build output in your component or plug-in project.  Add the following element to include
'       the assembly from this project in the component or plug-in project's package output.  If there are
'       other dependencies besides the Sandcastle assemblies, you may need to add additional elements for them
'       as well.
'
'       This keeps the platform-specific UI elements isolated and allows the component to work with MSBuild and
'       dotnet build.
'
'      <ItemGroup>
'          <None Include="..\$safeprojectname$\bin\$(Configuration)\net472\$safeprojectname$.dll">
'              <Pack>true</Pack>
'              <PackagePath>tools\</PackagePath>
'              <Visible>false</Visible>
'          </None>
'      </ItemGroup>

Imports System.ComponentModel.Composition.Hosting

Imports Sandcastle.Core.BuildAssembler

Imports SandcastleBuilder.Utils
Imports SandcastleBuilder.Utils.BuildComponent

''' <summary>
''' This is an example configuration form for a build component or plug-in created with XAML
''' </summary>
Public Class XamlExampleConfigDlg

    ' TODO: Implement the editor factory for your build component or plug-in.  Remove the factory class
    ' that you don't need.

    ' TOOD: For a plug-in
#Region "Plug-in configuration editor factory for MEF"
    '=====================================================================

    ''' <summary>
    ''' This allows editing of the plug-in configuration
    ''' </summary>
    <PlugInConfigurationEditorExport("TODO: Put your build component ID here")>
    Public NotInheritable Class PlugInFactory
        Implements IPlugInConfigurationEditor

        ''' <inheritdoc />
        Private Function EditConfiguration(project As SandcastleProject, configuration As XElement) As Boolean Implements IPlugInConfigurationEditor.EditConfiguration
            Dim dlg As New XamlExampleConfigDlg(configuration)
            Return dlg.ShowDialog()
        End Function

    End Class
#End Region

    ' TODO: For a build component
#Region "Build component configuration editor factory for MEF"
    '=====================================================================

    ''' <summary>
    ''' This allows editing of the component configuration
    ''' </summary>
    <ConfigurationEditorExport("TODO: Put your build component ID here")>
    Public NotInheritable Class BuildComponentFactory
        Implements IConfigurationEditor

        ''' <inheritdoc />
        Private Function EditConfiguration(configuration As XElement, container As CompositionContainer) As Boolean Implements IConfigurationEditor.EditConfiguration
            Dim dlg As New XamlExampleConfigDlg(configuration)
            Return dlg.ShowDialog()
        End Function
    End Class
#End Region

#Region "Private data members"
    '=====================================================================

    Private ReadOnly configuration As XElement

#End Region

#Region "Constructor"
    '=====================================================================

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="configuration">The current configuration element</param>
    Public Sub New(configuration As XElement)
        InitializeComponent()

        If configuration Is Nothing
            Throw New ArgumentNullException(NameOf(configuration))
        End If

        Me.configuration = configuration

        ' TODO: Load configuration from the XML and set the control values
        Dim node As XElement = configuration.Element("configElement")

        If node IsNot Nothing
            txtExample.Text = node.Attribute("configValue")?.Value
        End If
    End Sub
#End Region

#Region "Event handlers"
    '=====================================================================

    ''' <summary>
    ''' Save changes to the configuration
    ''' </summary>
    ''' <param name="sender">The sender of the event</param>
    ''' <param name="e">The event arguments</param>
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        ' TODO: Validate the settings and update the configuration elements with values from the controls
        Dim node As XElement = configuration.Element("configElement")

        If node Is Nothing
            node = New XElement("configElement", New XAttribute("configValue", String.Empty))
            configuration.Add(node)
        End If

        node.Attribute("configValue").Value = txtExample.Text

        Me.DialogResult = True
        Me.Close()

    End Sub

#End Region

End Class
