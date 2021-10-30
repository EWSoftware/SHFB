Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Xml.Linq

Imports SandcastleBuilder.Utils.BuildComponent
Imports SandcastleBuilder.Utils.BuildEngine

' Search for "TODO" to find changes that you need to make to this plug-in template.

Namespace $safeprojectname$

    ''' <summary>
    ''' TODO: Set your plug-in's unique ID and description in the export attribute below.
    ''' </summary>
    ''' <remarks>The <c>HelpFileBuilderPlugInExportAttribute</c> is used to export your plug-in so that the help
    ''' file builder finds it and can make use of it.  The example below shows the basic usage for a common
    ''' plug-in.  Set the additional attribute values as needed:
    '''
    ''' <list type="bullet">
    '''     <item>
    '''         <term>RunsInPartialBuild</term>
    '''         <description>Set this to true if your plug-in should run in partial builds used to generate
    ''' reflection data for the API Filter editor dialog or namespace comments used for the Namespace Comments
    ''' editor dialog.  Typically, this is left set to false.</description>
    '''     </item>
    ''' </list>
    '''
    ''' Plug-ins are singletons in nature.  The composition container will create instances as needed and will
    ''' dispose of them when the container is disposed of.</remarks>
    <HelpFileBuilderPlugInExport("$safeprojectname$", Version := AssemblyInfo.ProductVersion,
      Copyright := AssemblyInfo.Copyright, Description := "$safeprojectname$ plug-in")>
    Public NotInheritable Class $safeprojectname$PlugIn
        Implements IPlugIn

        #Region "Private data members"
        '=====================================================================

        Private plugInExecutionPoints As List(Of ExecutionPoint)

        Private builder As BuildProcess

        #End Region

        #Region "IPlugIn implementation"
        '=====================================================================

        ''' <summary>
        ''' This read-only property returns a collection of execution points that define when the plug-in should
        ''' be invoked during the build process.
        ''' </summary>
        Public ReadOnly Property ExecutionPoints As IEnumerable(Of ExecutionPoint) Implements IPlugIn.ExecutionPoints
            Get
                If plugInExecutionPoints Is Nothing Then
                    ' TODO: Modify this to set your execution points
                    plugInExecutionPoints = New List(Of ExecutionPoint) From
                    {
                        New ExecutionPoint(BuildStep.ValidatingDocumentationSources, ExecutionBehaviors.Before),
                        New ExecutionPoint(BuildStep.GenerateReflectionInfo, ExecutionBehaviors.Before)
                    }
                End If

                Return plugInExecutionPoints
            End Get
        End Property

        ''' <summary>
        ''' This method is used to initialize the plug-in at the start of the build process
        ''' </summary>
        ''' <param name="buildProcess">A reference to the current build process</param>
        ''' <param name="configuration">The configuration data that the plug-in should use to initialize itself</param>
        Public Sub Initialize(buildProcess As BuildProcess, configuration As XElement) Implements IPlugIn.Initialize
            builder = buildProcess

            Dim metadata As HelpFileBuilderPlugInExportAttribute = DirectCast(Enumerable.First(Of Object)(
                MyBase.GetType.GetCustomAttributes(GetType(HelpFileBuilderPlugInExportAttribute), False)),
                HelpFileBuilderPlugInExportAttribute)

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright)

            ' TODO: Add your initialization code here such as reading the configuration data
        End Sub

        ''' <summary>
        ''' This method is used to execute the plug-in during the build process
        ''' </summary>
        ''' <param name="context">The current execution context</param>
        Public Sub Execute(context As ExecutionContext) Implements IPlugIn.Execute
            ' TODO: Add your execution code here
            builder.ReportProgress("In $safeprojectname$PlugIn Execute() method")
        End Sub

        #End Region

        #Region "IDisposable implementation"
        '=====================================================================

        ' TODO: If the plug-in hasn't got any disposable resources, this finalizer can be removed
        ''' <summary>
        ''' This handles garbage collection to ensure proper disposal of the plug-in if not done explicitly
        ''' with <see cref="Dispose()"/>.
        ''' </summary>
        Protected Overrides Sub Finalize()
            Me.Dispose()
        End Sub

        ''' <summary>
        ''' This implements the Dispose() interface to properly dispose of the plug-in object
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            ' TODO: Dispose of any resources here if necessary
            GC.SuppressFinalize(Me)
        End Sub

        #End Region

    End Class

End Namespace
