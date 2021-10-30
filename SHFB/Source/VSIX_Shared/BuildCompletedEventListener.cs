//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : BuildCompletedEventListener.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/26/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This file contains the class used to listen for build started events to flush pending property page changes
// and for build completed events so that we can open the help file after successful builds if so requested.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/18/2011  EFW  Created the code
//===============================================================================================================

using System;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;

using SandcastleBuilder.Package.Nodes;
using SandcastleBuilder.Package.PropertyPages;
using SandcastleBuilder.Package.ToolWindows;

namespace SandcastleBuilder.Package
{
    /// <summary>
    /// This is used to listen for build started events to flush pending property page changes and for
    /// build completed events so that we can open the help file after successful builds if so requested.
    /// </summary>
    internal class BuildCompletedEventListener : UpdateSolutionEventsListener
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        public BuildCompletedEventListener(IServiceProvider serviceProvider) : base(serviceProvider)
		{
		}

        /// <summary>
        /// This is overridden to flush pending changes to project property pages
        /// </summary>
        /// <param name="cancelUpdate">Not used</param>
        /// <returns>Always returns S_OK</returns>
        /// <remarks>This is needed so that pending property page changes are flushed to the project prior
        /// to the build occurring.  Typically, this happens automatically.  However, if a build is
        /// invoked using the context menu on the project node, it does not.  This works around that
        /// issue.</remarks>
        public override int UpdateSolution_StartUpdate(ref int cancelUpdate)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach(var p in BasePropertyPage.AllPropertyPages)
                if(!p.IsDisposed)
                    ((Microsoft.VisualStudio.OLE.Interop.IPropertyPage)p).Apply();

            return VSConstants.S_OK;
        }

        /// <summary>
        /// This is overridden to open the help file after a successful build or the log viewer tool window
        /// after a failed build if so indicated by the Sandcastle Help File Builder options.
        /// </summary>
        /// <inheritdoc />
        /// <remarks>Note that the base method documentation is wrong.  The action parameter is actually a
        /// combination of VSSOLNBUILDUPDATEFLAGS values.</remarks>
        public override int UpdateProjectCfg_Done(IVsHierarchy hierarchy, IVsCfg configProject,
          IVsCfg configSolution, uint action, int success, int cancel)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            SandcastleBuilderProjectNode projectNode;
            SandcastleBuilderOptionsPage options;

            if(configProject is ProjectConfig cfg)
            {
                if(cancel == 0 && success != 0)
                {
                    // Open the help file on a successful build if so requested
                    projectNode = cfg.ProjectMgr as SandcastleBuilderProjectNode;
                    options = SandcastleBuilderPackage.Instance.GeneralOptions;

                    if(projectNode != null && options != null)
                        if((action & (uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD) != 0 && options.OpenHelpAfterBuild)
                            SandcastleBuilderPackage.Instance.ViewBuiltHelpFile(projectNode);
                        else
                            if((action & (uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_CLEAN) != 0)
                        {
                            if(cfg.ProjectMgr.Package.FindToolWindow(typeof(ToolWindows.BuildLogToolWindow), 0,
                              false) is BuildLogToolWindow window)
                            {
                                window.ClearLog();
                            }
                        }
                }
                else
                    if(cancel == 0 && success == 0)
                {
                    // Open the build log tool window on a failed build if so requested
                    projectNode = cfg.ProjectMgr as SandcastleBuilderProjectNode;
                    options = SandcastleBuilderPackage.Instance.GeneralOptions;

                    if(projectNode != null && options != null)
                        if((action & (uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD) != 0 && options.OpenLogViewerOnFailedBuild)
                            projectNode.OpenBuildLogToolWindow(true);
                        else
                        {
                            // The user doesn't want it opened.  However, if it's already open, refresh the
                            // log file reference so that it shows the correct file when it is displayed.
                            if(cfg.ProjectMgr.Package.FindToolWindow(typeof(ToolWindows.BuildLogToolWindow), 0,
                              false) is BuildLogToolWindow window && ((IVsWindowFrame)window.Frame).IsVisible() == VSConstants.S_OK)
                            {
                                projectNode.OpenBuildLogToolWindow(false);
                            }
                        }
                }
            }

            return VSConstants.S_OK;
        }
    }
}
