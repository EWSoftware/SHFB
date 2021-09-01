//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SandcastleBuilderPackage.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/31/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This file contains the class that defines the Sandcastle Help File Builder Visual Studio package
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB
// This notice, the author's name, and all copyright notices must remain intact in all applications,
// documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/18/2011  EFW  Created the code
// 12/11/2011  EFW  Added support for Entity References tool window
// 12/26/2011  EFW  Added support for the SHFB file editors
// 01/21/2012  EFW  Added support for the Topic Previewer tool window
// 10/06/2012  EFW  Added support for Help Viewer 2.0
// 03/08/2014  EFW  Added support for the Open XML file format
// 04/01/2015  EFW  Added support for the Markdown file format
// 05/03/2015  EFW  Removed support for the MS Help 2 file format
//===============================================================================================================

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using EnvDTE;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Sandcastle.Core;
using Sandcastle.Platform.Windows;

using SandcastleBuilder.Package.Editors;
using SandcastleBuilder.Package.Nodes;
using SandcastleBuilder.Package.PropertyPages;
using SandcastleBuilder.Package.ToolWindows;

using SandcastleBuilder.Utils;

using SandcastleBuilder.WPF.PropertyPages;
using SandcastleBuilder.WPF.UI;

namespace SandcastleBuilder.Package
{
    /// <summary>
    /// This is the class that implements the Sandcastle Help File Builder
    /// package.
    /// </summary>
    /// <remarks>
    /// The class implements the <c>IVsPackage</c> interface and registers itself with the shell.  This package
    /// uses the helper classes defined inside the Managed Package Framework (MPF) to do it.  It derives from the
    /// <c>Package</c> class that provides the implementation of the <c>IVsPackage</c> interface and uses the
    /// registration attributes defined in the framework to register itself and its components with the shell.</remarks>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is a package
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    // This attribute is used to register the information needed to show the this package in the Help/About
    // dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#111", "SHFB", IconResourceID = 400)]
    // Define the package GUID
    [Guid(GuidList.guidSandcastleBuilderPackagePkgString)]
    // Register the project factory.  The template is in a VSTemplate so a non-existent path is used for the
    // template directory parameter and we set LanguageVsTemplate to our project type value.  The display name is
    // set to "Documentation" to provide a more generic category for the Add New Project and Add New Item dialog
    // boxes.
    [ProvideProjectFactory(typeof(SandcastleBuilderProjectFactory), "Documentation", "#112", "shfbproj",
      "shfbproj", @".\NullPath", LanguageVsTemplate = "SHFBProject")]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This registers the general options page
    [ProvideOptionPage(typeof(SandcastleBuilderOptionsPage), "Sandcastle Help File Builder", "General",
      110, 200, true)]
    // Connect the options page to a settings category so that its properties can be imported/exported
    [ProvideProfile(typeof(SandcastleBuilderOptionsPage), "Sandcastle Help File Builder", "General",
      110, 110, true, DescriptionResourceID = 201)]
    // Provide project option pages
    [ProvideObject(typeof(BuildEventPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(BuildPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(ComponentPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(Help1WebsitePropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(HelpFilePropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(MissingTagPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(MSHelpViewerPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(PathPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(PlugInPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(SummaryPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(TransformArgumentsPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(UserDefinedPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(VisibilityPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    // Provide tool windows
    [ProvideToolWindow(typeof(BuildLogToolWindow), Orientation = ToolWindowOrientation.Right,
        Style = VsDockStyle.MDI, MultiInstances = false, Transient = false, PositionX = 100, PositionY = 100,
        Width = 300, Height = 300)]
    [ProvideToolWindow(typeof(EntityReferencesToolWindow), Orientation = ToolWindowOrientation.Right,
        Style = VsDockStyle.Float, MultiInstances = false, Transient = false, PositionX = 100, PositionY = 100,
        Width = 300, Height = 300)]
    [ProvideToolWindow(typeof(TopicPreviewerToolWindow), Orientation = ToolWindowOrientation.Right,
        Style = VsDockStyle.Float, MultiInstances = false, Transient = false, PositionX = 100, PositionY = 100,
        Width = 300, Height = 300)]
    // Provide custom file editors.  As above, a non-existent template path is used.
    [ProvideEditorExtension(typeof(ContentLayoutEditorFactory), ".content", 50,
      ProjectGuid = GuidList.guidSandcastleBuilderProjectFactoryString, NameResourceID = 129,
      TemplateDir = @".\NullPath")]
    [ProvideEditorExtension(typeof(ResourceItemEditorFactory), ".items", 50,
      ProjectGuid = GuidList.guidSandcastleBuilderProjectFactoryString, NameResourceID = 133,
      TemplateDir = @".\NullPath")]
    [ProvideEditorExtension(typeof(SiteMapEditorFactory), ".sitemap", 50,
      ProjectGuid = GuidList.guidSandcastleBuilderProjectFactoryString, NameResourceID = 119,
      TemplateDir = @".\NullPath")]
    [ProvideEditorExtension(typeof(TokenEditorFactory), ".tokens", 50,
      ProjectGuid = GuidList.guidSandcastleBuilderProjectFactoryString, NameResourceID = 135,
      TemplateDir = @".\NullPath")]
    // Register a path that should be probed for candidate assemblies at assembly load time.  This lets the
    // package find its dependency assemblies.
    [ProvideBindingPath()]
    public sealed class SandcastleBuilderPackage : Microsoft.VisualStudio.Project.ProjectPackage
    {
        #region Private data members
        //=====================================================================

        private BuildCompletedEventListener buildCompletedListener;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the package instance
        /// </summary>
        internal static SandcastleBuilderPackage Instance { get; private set; }

        /// <summary>
        /// This returns the project type for the Framework Retargeting dialog
        /// </summary>
        /// <remarks>My assumption, which appears to be correct, is that this should return the same value as the
        /// <c>SandcastleBuilderProjectNode.ProjectType</c> property.</remarks>
        public override string ProductUserContext => "SHFBProject";

        /// <summary>
        /// Return the general options page for the package
        /// </summary>
        public SandcastleBuilderOptionsPage GeneralOptions =>
            (SandcastleBuilderOptionsPage)this.GetDialogPage(typeof(SandcastleBuilderOptionsPage));

        /// <summary>
        /// This returns a <see cref="SandcastleBuilderProjectNode"/> instance for the
        /// current project if it is one or null if not.
        /// </summary>
        public static SandcastleBuilderProjectNode CurrentProjectNode
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                Array activeProjects = null;

                if(Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE)) is DTE dte)
                    activeProjects = dte.ActiveSolutionProjects as Array;

                if(activeProjects != null && activeProjects.Length > 0)
                {
                    if(activeProjects.GetValue(0) is Project p)
                        return (p.Object as SandcastleBuilderProjectNode);
                }

                return null;
            }
        }

        /// <summary>
        /// This returns a <see cref="SandcastleProject"/> instance for the
        /// current project if it is one or null if not
        /// </summary>
        /// <value>The caller should not dispose of the project</value>
        public static SandcastleProject CurrentSandcastleProject
        {
            get
            {
#pragma warning disable VSTHRD010
                var pn = CurrentProjectNode;

                if(pn != null)
                    return pn.SandcastleProject;
#pragma warning restore VSTHRD010

                return null;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Default constructor of the package.
        /// </summary>
        /// <remarks>Inside this method you can place any initialization code that does not require any Visual
        /// Studio service because at this point the package object is created but not sited yet inside Visual
        /// Studio environment. The place to do all the other initialization is the Initialize method.</remarks>
        public SandcastleBuilderPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for {0}",
                this.ToString()));

            // Ensure that the custom controls are known by the base property page class
            if(!BasePropertyPage.CustomControls.ContainsKey("Sandcastle.Platform.Windows.UserControls.FilePathUserControl"))
            {
                BasePropertyPage.CustomControls.Add("Xceed.Wpf.Toolkit.IntegerUpDown", "Value");
                BasePropertyPage.CustomControls.Add("Sandcastle.Platform.Windows.UserControls.FilePathUserControl",
                    "PersistablePath");
                BasePropertyPage.CustomControls.Add("Sandcastle.Platform.Windows.UserControls.FolderPathUserControl",
                    "PersistablePath");
            }
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            SandcastleBuilderPackage.Instance = null;

            if(buildCompletedListener != null)
            {
                buildCompletedListener.Dispose();
                buildCompletedListener = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// This is overridden to initialize the package
        /// </summary>
        /// <remarks>This method is called right after the package is sited, so this is the place where you can
        /// put all the initialization code that relies on services provided by Visual Studio.</remarks>
        protected override async System.Threading.Tasks.Task InitializeAsync(
          System.Threading.CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of {0}",
                this.ToString()));

            await base.InitializeAsync(cancellationToken, progress);

            // When initialized asynchronously, we may be on a background thread at this point.  Do any
            // initialization that requires the UI thread after switching to the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            SandcastleBuilderPackage.Instance = this;

            // Add our command handlers for menu items (commands must exist in the .vsct file)
            if(await this.GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                CommandID commandId;
                OleMenuCommand menuItem;

                // Create the command for button ViewHelpFile
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.ViewHelpFile);
                menuItem = new OleMenuCommand(ViewHelpFileExecuteHandler, null, ViewHelpFileQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);

                // Create the command for button ViewHtmlHelp
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.ViewHtmlHelp);
                menuItem = new OleMenuCommand(ViewHtmlHelpExecuteHandler, null, ViewHtmlHelpQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);

                // Create the command for button ViewMshcHelp
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.ViewMshcHelp);
                menuItem = new OleMenuCommand(ViewMshcHelpExecuteHandler, null, ViewMshcHelpQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);

                // Create the command for button ViewAspNetWebsite
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.ViewAspNetWebsite);
                menuItem = new OleMenuCommand(ViewAspNetWebsiteExecuteHandler, null, ViewAspNetWebsiteQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);

                // Create the command for button ViewFaq
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.ViewFaq);
                menuItem = new OleMenuCommand(ViewFaqExecuteHandler, commandId);
                mcs.AddCommand(menuItem);

                // Create the command for button ViewShfbHelp
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.ViewShfbHelp);
                menuItem = new OleMenuCommand(ViewShfbHelpExecuteHandler, commandId);
                mcs.AddCommand(menuItem);

                // Create the command for button OpenInStandaloneGUI
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.OpenInStandaloneGUI);
                menuItem = new OleMenuCommand(OpenInStandaloneGUIExecuteHandler, null, OpenInStandaloneGUIQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);

                // Create the command for button ViewBuildLog
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.ViewBuildLog);
                menuItem = new OleMenuCommand(ViewBuildLogExecuteHandler, null, ViewBuildLogQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);

                // Create the command for button EntityReferencesWindow
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.EntityReferencesWindow);
                menuItem = new OleMenuCommand(EntityReferencesWindowExecuteHandler, commandId);
                mcs.AddCommand(menuItem);

                // Create the command for button TopicPreviewerWindow
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.TopicPreviewerWindow);
                menuItem = new OleMenuCommand(TopicPreviewerWindowExecuteHandler, commandId);
                mcs.AddCommand(menuItem);

                // Create the command for button ViewDocxHelp
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.ViewDocxHelp);
                menuItem = new OleMenuCommand(ViewDocxHelpExecuteHandler, null, ViewDocxHelpQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
            }

            // Register the project factory
            this.RegisterProjectFactory(new SandcastleBuilderProjectFactory(this));

            // Register the SHFB file editor factories
            base.RegisterEditorFactory(new ContentLayoutEditorFactory());
            base.RegisterEditorFactory(new ResourceItemEditorFactory());
            base.RegisterEditorFactory(new SiteMapEditorFactory());
            base.RegisterEditorFactory(new TokenEditorFactory());

            // Create the update solution event listener for build completed events
            buildCompletedListener = new BuildCompletedEventListener(this);

            // Register for solution events so that we can clear the component cache when necessary
            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnAfterCloseSolution += (s, e) => ComponentCache.Clear();

            try
            {
                // Set the owning window for WPF modal dialogs to the main Visual Studio window
                Sandcastle.Platform.Windows.WpfHelpers.MainWindowHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            }
            catch
            {
                // Ignore exceptions.  There is no main window when invoked for a command line build.
                // It may also try to load the package before the main window is available if tool windows
                // were left open.  Worst case, modal dialogs may not appear over the main form on dual
                // monitor systems.
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to set the state of a menu command on the View Help menu
        /// </summary>
        /// <param name="command">The command object</param>
        /// <param name="format">The help file format</param>
        private static void SetViewHelpCommandState(OleMenuCommand command, HelpFileFormats? format)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Array activeProjects = null;
            bool visible = false, enabled = false;

            if(Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE)) is DTE dte)
            {
                Solution s = dte.Solution;

                // Hide the menu option if a SHFB project is not loaded
                if(s != null)
                {
#pragma warning disable VSTHRD010
                    visible = s.Projects.Cast<Project>().Any(
                        p => p.UniqueName.EndsWith(".shfbproj", StringComparison.OrdinalIgnoreCase));
#pragma warning restore VSTHRD010

                    // Check the active project for the specified help format if visible
                    if(visible)
                    {
                        try
                        {
                            activeProjects = dte.ActiveSolutionProjects as Array;
                        }
                        catch
                        {
                            // The above can throw an exception while the project is loading which
                            // we should ignore.
                        }

                        if(activeProjects != null && activeProjects.Length > 0)
                        {
                            if(activeProjects.GetValue(0) is Project p && p.Object != null &&
                              p.UniqueName.EndsWith(".shfbproj", StringComparison.OrdinalIgnoreCase))
                            {
                                SandcastleBuilderProjectNode pn = (SandcastleBuilderProjectNode)p.Object;
                                string projectHelpFormat = (pn.GetProjectProperty("HelpFileFormat") ??
                                    HelpFileFormats.HtmlHelp1.ToString());

                                enabled = (!pn.BuildInProgress && (format == null || projectHelpFormat.IndexOf(
                                    format.ToString(), StringComparison.OrdinalIgnoreCase) != -1));
                            }
                        }
                    }
                }
            }

            command.Visible = visible;
            command.Enabled = enabled;
        }

        /// <summary>
        /// View the last built help output
        /// </summary>
        /// <param name="projectNode">The project node for which to open the help file</param>
        internal void ViewBuiltHelpFile(SandcastleBuilderProjectNode projectNode)
        {
            if(projectNode != null)
            {
                SandcastleProject project = projectNode.SandcastleProject;

                if(project == null)
                    return;

#pragma warning disable VSTHRD010
                if((project.HelpFileFormat & HelpFileFormats.HtmlHelp1) != 0)
                    ViewBuiltHelpFile(project, PkgCmdIDList.ViewHtmlHelp);
                else
                    if((project.HelpFileFormat & HelpFileFormats.OpenXml) != 0)
                        ViewBuiltHelpFile(project, PkgCmdIDList.ViewDocxHelp);
                    else
                        if((project.HelpFileFormat & HelpFileFormats.Markdown) != 0)
                            ViewBuiltHelpFile(project, 0);
                        else
                            if((project.HelpFileFormat & HelpFileFormats.Website) != 0)
                                Utility.OpenUrl(projectNode.StartWebServerInstance());
                            else
                            {
                                // This format opens a modal dialog box so we'll use it last if nothing else
                                // is selected.
                                var options = this.GeneralOptions;

                                if(options != null)
                                {
                                    var dlg = new LaunchMSHelpViewerDlg(project, options.MSHelpViewerPath);
                                    dlg.ShowModalDialog();
                                }
                            }
#pragma warning restore VSTHRD010
            }
        }

        /// <summary>
        /// View the last built help output
        /// </summary>
        /// <param name="project">The project to use or null to use the current project</param>
        /// <param name="commandId">The ID of the command that invoked the request which determines the help
        /// file format launched.  Zero is used for markdown content since there is no viewer for it.</param>
        private static void ViewBuiltHelpFile(SandcastleProject project, uint commandId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string outputPath;

            if(project == null)
            {
                project = CurrentSandcastleProject;

                if(project == null)
                    return;
            }

            // Make sure we start out in the project's output folder in case the output folder is relative to it
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(project.Filename)));
            outputPath = project.OutputPath;

            if(String.IsNullOrEmpty(outputPath))
                outputPath = Directory.GetCurrentDirectory();
            else
                outputPath = Path.GetFullPath(outputPath);

            if(commandId == PkgCmdIDList.ViewHtmlHelp)
                outputPath += project.HtmlHelpName + ".chm";
            else
                if(commandId == PkgCmdIDList.ViewDocxHelp)
                    outputPath += project.HtmlHelpName + ".docx";
                else
                    if(commandId == 0)
                        outputPath += "_Sidebar.md";
                    else
                        outputPath += "Index.html";

            // If there are substitution tags present, have a go at resolving them
            if(outputPath.IndexOf("{@", StringComparison.Ordinal) != -1)
            {
                try
                {
                    var bp = new SandcastleBuilder.Utils.BuildEngine.BuildProcess(project);
                    outputPath = bp.SubstitutionTags.TransformText(outputPath);
                }
                catch
                {
                    // Ignore errors
                    Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_WARNING, "The help filename appears to " +
                        "contain substitution tags but they could not be resolved to determine the actual " +
                        "file to open for viewing.  Building website output and viewing it can be used to " +
                        "work around this issue.");
                    return;
                }
            }

            if(!File.Exists(outputPath))
            {
                Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_INFO, "A copy of the help file does not appear " +
                    "to exist yet.  It may need to be built.");
                return;
            }

            try
            {
                if(outputPath.EndsWith(".chm", StringComparison.OrdinalIgnoreCase) ||
                  outputPath.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Process.Start(outputPath);
                }
                else
                    if(outputPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                    {
                        if(Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE)) is DTE dte)
                        {
                            var doc = dte.ItemOperations.OpenFile(outputPath, EnvDTE.Constants.vsViewKindPrimary);

                            if(doc != null)
                                doc.Activate();
                        }
                    }
                    else
                        Utility.OpenUrl(outputPath);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_CRITICAL, "Unable to open help file '{0}'\r\n" +
                    "Reason: {1}", outputPath, ex.Message);
            }
        }
        #endregion

#pragma warning disable VSTHRD010
        #region Project event handlers
        //=====================================================================

        /// <summary>
        /// Set the state of the open in standalone GUI command
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void OpenInStandaloneGUIQueryStatusHandler(object sender, EventArgs e)
        {
            SetViewHelpCommandState((OleMenuCommand)sender, null);
        }

        /// <summary>
        /// Open the current project in the standalone GUI
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void OpenInStandaloneGUIExecuteHandler(object sender, EventArgs e)
        {
            var pn = CurrentProjectNode;

            if(pn != null)
                pn.OpenInStandaloneGui();
        }
        #endregion

        #region View help file event handlers
        //=====================================================================

        /// <summary>
        /// Set the state of the View Help File command
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ViewHelpFileQueryStatusHandler(object sender, EventArgs e)
        {
            SetViewHelpCommandState((OleMenuCommand)sender, null);
        }

        /// <summary>
        /// View the help file produced by the last build.  Pick the first available format
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ViewHelpFileExecuteHandler(object sender, EventArgs e)
        {
            this.ViewBuiltHelpFile(CurrentProjectNode);
        }

        /// <summary>
        /// Set the state of the View HTML Help file command
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ViewHtmlHelpQueryStatusHandler(object sender, EventArgs e)
        {
            SetViewHelpCommandState((OleMenuCommand)sender, HelpFileFormats.HtmlHelp1);
        }

        /// <summary>
        /// View the last built HTML Help file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ViewHtmlHelpExecuteHandler(object sender, EventArgs e)
        {
            ViewBuiltHelpFile(null, PkgCmdIDList.ViewHtmlHelp);
        }

        /// <summary>
        /// Set the state of the View MS Help View file command
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ViewMshcHelpQueryStatusHandler(object sender, EventArgs e)
        {
            SetViewHelpCommandState((OleMenuCommand)sender, HelpFileFormats.MSHelpViewer);
        }

        /// <summary>
        /// Launch the MS Help Viewer to view the last built Microsoft Help Viewer file.  This
        /// will optionally install it first.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ViewMshcHelpExecuteHandler(object sender, EventArgs e)
        {
            SandcastleProject project = CurrentSandcastleProject;

            var options = this.GeneralOptions;

            if(project != null && options != null)
            {
                var dlg = new LaunchMSHelpViewerDlg(project, options.MSHelpViewerPath);
                dlg.ShowModalDialog();
            }
        }

        /// <summary>
        /// Set the state of the View ASP.NET website help command
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ViewAspNetWebsiteQueryStatusHandler(object sender, EventArgs e)
        {
            SetViewHelpCommandState((OleMenuCommand)sender, HelpFileFormats.Website);
        }

        /// <summary>
        /// View the last built ASP.NET help website
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ViewAspNetWebsiteExecuteHandler(object sender, EventArgs e)
        {
            var pn = CurrentProjectNode;

            if(pn != null)
                Utility.OpenUrl(pn.StartWebServerInstance());
        }

        /// <summary>
        /// Set the state of the View Open XML help command
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ViewDocxHelpQueryStatusHandler(object sender, EventArgs e)
        {
            SetViewHelpCommandState((OleMenuCommand)sender, HelpFileFormats.OpenXml);
        }

        /// <summary>
        /// View the last built Open XML document
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ViewDocxHelpExecuteHandler(object sender, EventArgs e)
        {
            ViewBuiltHelpFile(null, PkgCmdIDList.ViewDocxHelp);
        }

        /// <summary>
        /// Set the state of the view build log content command
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ViewBuildLogQueryStatusHandler(object sender, EventArgs e)
        {
            SetViewHelpCommandState((OleMenuCommand)sender, null);
        }

        /// <summary>
        /// View the build log content for the current project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ViewBuildLogExecuteHandler(object sender, EventArgs e)
        {
            var pn = CurrentProjectNode;

            if(pn != null)
                pn.OpenBuildLogToolWindow(true);
        }
        #endregion
#pragma warning restore VSTHRD010

        #region Tool window event handlers
        //=====================================================================

        /// <summary>
        /// Show the Entity References tool window
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void EntityReferencesWindowExecuteHandler(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var window = this.FindToolWindow(typeof(ToolWindows.EntityReferencesToolWindow), 0, true);

            if(window == null || window.Frame == null)
                throw new NotSupportedException("Unable to create Entity References tool window");

            var windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        /// <summary>
        /// Show the Topic Previewer tool window
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void TopicPreviewerWindowExecuteHandler(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IntPtr ppHier = IntPtr.Zero, ppSC = IntPtr.Zero;
            string filename = null;

            var window = this.FindToolWindow(typeof(ToolWindows.TopicPreviewerToolWindow), 0, true);

            if(window == null || window.Frame == null)
                throw new NotSupportedException("Unable to create Topic Previewer tool window");

            var windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

            if(window is TopicPreviewerToolWindow previewer)
            {
                if(Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsShellMonitorSelection)) is IVsMonitorSelection ms)
                {
                    try
                    {
                        // Get the current filename and, if it's a MAML topic, show it by default
                        ms.GetCurrentSelection(out ppHier, out uint pitemid, out IVsMultiItemSelect ppMIS, out ppSC);

                        if(pitemid != VSConstants.VSITEMID_NIL && ppHier != IntPtr.Zero)
                        {
                            IVsHierarchy hierarchy = Marshal.GetObjectForIUnknown(ppHier) as IVsHierarchy;

                            hierarchy.GetCanonicalName(pitemid, out filename);

                            if(filename != null && !filename.EndsWith(".aml", StringComparison.OrdinalIgnoreCase))
                                filename = null;
                        }
                    }
                    finally
                    {
                        if(ppHier != IntPtr.Zero)
                            Marshal.Release(ppHier);

                        if(ppSC != IntPtr.Zero)
                            Marshal.Release(ppSC);
                    }
                }

                previewer.PreviewTopic(CurrentSandcastleProject, filename);
            }
        }
        #endregion

        #region SHFB Help event handlers
        //=====================================================================

        /// <summary>
        /// View the FAQ topic in the help file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ViewFaqExecuteHandler(object sender, EventArgs e)
        {
            UiUtility.ShowHelpTopic("1aea789d-b226-4b39-b534-4c97c256fac8");
        }

        /// <summary>
        /// View the SHFB help file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ViewShfbHelpExecuteHandler(object sender, EventArgs e)
        {
            UiUtility.ShowHelpTopic("bd1ddb51-1c4f-434f-bb1a-ce2135d3a909");
        }
        #endregion
    }
}
