#if DEBUG

// This is the example used in the help file.  It's here so that we can maintain it if the interface changes but
// it's only in the debug build.

#region Help File Plug-in Example
//=============================================================================
// System  : <Your Plug-In Project>
// File    : <YourFileName.cs>
// Author  : <Your Name Here>
// Updated : <--Date-->
// Compiler: Microsoft Visual C#
//
// <A description of the plug-in.>
//
// Date        Who  Comments
// ============================================================================
// <--Date-->  <->  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

// TODO: Use your namespace
namespace Test.PlugIns
{
    // TODO: Edit AssemblyInfo.cs to set your plug-in's version.  This will
    // become part of the plug-in's metadata.
    // TODO: Edit AssemblyInfo.cs to set your plug-in's copyright  This will
    // become part of the plug-in's metadata.

    /// <summary>
    /// TODO: Set your plug-in's unique ID and description in the export
    /// attribute below.
    /// </summary>
    /// <remarks>The <see cref="HelpFileBuilderPlugInExportAttribute"/> is used
    /// to export your plug-in so that the help file builder finds it and can
    /// make use of it.  The example below shows the basic usage for a common
    /// plug-in.  Set the additional attribute values as needed:
    /// 
    /// <list type="bullet">
    ///     <item>
    ///         <term>AdditionalCopyrightInfo</term>
    ///         <description>Specify user-defined copyright information such as
    /// additional copyright information for tools used specifically by this
    /// plug-in.</description>
    ///     </item>
    ///     <item>
    ///         <term>IsConfigurable</term>
    ///         <description>Set this to true if your plug-in contains configurable
    /// settings.  The <see cref="ConfigurePlugIn"/> method will be called to let
    /// the user change the settings.</description>
    ///     </item>
    ///     <item>
    ///         <term>RunsInPartialBuild</term>
    ///         <description>Set this to true if your plug-in should run in
    /// partial builds used to generate reflection data for the API Filter
    /// editor dialog or namespace comments used for the Namespace Comments
    /// editor dialog.  Typically, this is left set to false.</description>
    ///     </item>
    /// </list>
    /// 
    /// Plug-ins are singletons in nature.  The composition container will
    /// create instances as needed and will dispose of them when the container
    /// is disposed of.
    /// </remarks>
    [HelpFileBuilderPlugInExport("ZZZ - TODO: Add your plug-in's unique ID here",
      Description = "TODO: Add your plug-in description here")]
    public sealed class TestPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;

        private BuildProcess builder;
        #endregion

        #region IPlugIn implementation
        //=====================================================================

        /// <summary>
        /// This read-only property returns a collection of execution points
        /// that define when the plug-in should be invoked during the build
        /// process.
        /// </summary>
        public IEnumerable<ExecutionPoint> ExecutionPoints
        {
            get
            {
                if(executionPoints == null)
                    executionPoints = new List<ExecutionPoint>
                    {
                        // TODO: Modify this to set your execution points
                        new ExecutionPoint(BuildStep.ValidatingDocumentationSources,
                            ExecutionBehaviors.Before),
                        new ExecutionPoint(BuildStep.GenerateReflectionInfo,
                            ExecutionBehaviors.Before)
                    };

                return executionPoints;
            }
        }

        /// <summary>
        /// This method is used by the Sandcastle Help File Builder to let the
        /// plug-in perform its own configuration.
        /// </summary>
        /// <param name="project">A reference to the active project</param>
        /// <param name="currentConfig">The current configuration XML fragment</param>
        /// <returns>A string containing the new configuration XML fragment</returns>
        /// <remarks>The configuration data will be stored in the help file
        /// builder project.</remarks>
        public string ConfigurePlugIn(SandcastleProject project, string currentConfig)
        {
            // TODO: Add and invoke a configuration dialog if you need one.
            // You will also need to set the IsConfigurable property to true on
            // the class's export attribute.
            MessageBox.Show("This plug-in has no configurable settings",
                "Build Process Plug-In", MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return currentConfig;
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the
        /// build process.
        /// </summary>
        /// <param name="buildProcess">A reference to the current build
        /// process.</param>
        /// <param name="configuration">The configuration data that the plug-in
        /// should use to initialize itself.</param>
        public void Initialize(BuildProcess buildProcess, XPathNavigator configuration)
        {
            builder = buildProcess;

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id,
                metadata.Version, metadata.Copyright);

            // TODO: Add your initialization code here such as reading the
            // configuration data.
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            // TODO: Add your execution code here
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        // TODO: Change the name of this method to your plug-in name.  If the
        // plug-in hasn't got any disposable resources, it can be removed.
        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the
        /// plug-in if not done explicitly with <see cref="Dispose()"/>.
        /// </summary>
        ~TestPlugIn()
        {
            this.Dispose();
        }

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of the
        /// plug-in object.
        /// </summary>
        public void Dispose()
        {
            // TODO: Dispose of any resources here if necessary
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
#endregion

#endif
