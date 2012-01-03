//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : EntityReferencesToolWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/29/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to implement the Entity References tool
// window.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/11/2011  EFW  Created the code
//=============================================================================

using System;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

using SandcastleBuilder.Package.Editors;
using SandcastleBuilder.Package.Nodes;
using SandcastleBuilder.Utils;
using SandcastleBuilder.WPF.UserControls;

namespace SandcastleBuilder.Package.ToolWindows
{
	/// <summary>
    /// This is used to find and insert entity references such as token, image, code snippet, code entity, and
    /// table of contents links into files.
    /// </summary>
    [Guid("581e89c0-e423-4453-bde3-a0403d5f380d")]
    public class EntityReferencesToolWindow : EntityReferencesToolWindowBase, IVsSelectionEvents
    {
        #region Private data members
        //=====================================================================

        private object scope;
        private uint selectionMonitorCookie;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public EntityReferencesToolWindow()
        {
            var ucEntityReferences = new EntityReferencesControl { AllowAnimatedGif = true };

            base.Content = ucEntityReferences;

            ucEntityReferences.FileContentNeeded += ucEntityReferences_FileContentNeeded;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// Start monitoring for selection change events when initialized
        /// </summary>
        protected override void Initialize()
        {
            IntPtr ppHier = IntPtr.Zero, ppSC = IntPtr.Zero;
            uint pitemid;
            IVsMultiItemSelect ppMIS;

            base.Initialize();

            IVsMonitorSelection ms = Utility.GetServiceFromPackage<IVsMonitorSelection,
                SVsShellMonitorSelection>(true);

            if(ms != null)
            {
                ms.AdviseSelectionEvents(this, out selectionMonitorCookie);

                try
                {
                    // Get the current project if there is one and select it for use by the tool window
                    ms.GetCurrentSelection(out ppHier, out pitemid, out ppMIS, out ppSC);

                    if(pitemid != VSConstants.VSITEMID_NIL && ppHier != IntPtr.Zero)
                    {
                        IVsHierarchy hierarchy = Marshal.GetObjectForIUnknown(ppHier) as IVsHierarchy;

                        ((IVsSelectionEvents)this).OnSelectionChanged(null, 0, null, null, hierarchy,
                            pitemid, null, null);
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
        }

        /// <summary>
        /// Stop monitoring for selection change events when disposed
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            IVsMonitorSelection ms = Utility.GetServiceFromPackage<IVsMonitorSelection,
                SVsShellMonitorSelection>(true);

            if(ms != null)
                ms.UnadviseSelectionEvents(selectionMonitorCookie);

            base.Dispose(disposing);
        }

        /// <summary>
        /// This is overridden to pass hot keys on to the contained user control.
        /// </summary>
        /// <param name="m">The message to pre-process</param>
        /// <returns>True if the message was handled, false if not</returns>
        /// <remarks>When a WPF user control is hosted in a docked tool window, the hot keys no longer
        /// work.  This works around the problem by manually seeing if the control makes use of the
        /// hot key, and if it does, processing it here.</remarks>
        protected override bool PreProcessMessage(ref System.Windows.Forms.Message m)
        {
            if(m.Msg == 0x0100 /* WM_KEYDOWN */)
            {
                System.Windows.Forms.Keys keyCode = (System.Windows.Forms.Keys)m.WParam &
                    System.Windows.Forms.Keys.KeyCode;

                if(keyCode == System.Windows.Forms.Keys.F1)
                {
                    ApplicationCommands.Help.Execute(null, (UserControl)base.Content);
                    return true;
                }
            }

            if(m.Msg == 0x0104 /*WM_SYSKEYDOWN*/)
            {
                if(Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                {
                    // Cache a copy of the scope on first use
                    if(scope == null && base.Content != null)
                    {
                        // Get the scope for handling hot keys.  The key used here doesn't matter.
                        // We're just getting the scope to use.
                        AccessKeyPressedEventArgs e = new AccessKeyPressedEventArgs("X");

                        ((UserControl)base.Content).RaiseEvent(e);
                        scope = e.Scope;
                    }

                    string key = ((char)m.WParam).ToString();

                    // See if the hot key is registered for the control.  If so, handle it.
                    if(scope != null && AccessKeyManager.IsKeyRegistered(scope, key))
                    {
                        AccessKeyManager.ProcessKey(scope, key, false);
                        return true;
                    }
                }
            }

            return base.PreProcessMessage(ref m);
        }
        #endregion

        #region IVsSelectionEvents Members
        //=====================================================================

        /// <summary>
        /// Not used by this tool window
        /// </summary>
        /// <param name="dwCmdUICookie">The command ID cookie</param>
        /// <param name="fActive">Active state flag</param>
        /// <returns></returns>
        int IVsSelectionEvents.OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Not used by this tool window
        /// </summary>
        /// <param name="elementid">The changed element ID</param>
        /// <param name="varValueOld">The old value</param>
        /// <param name="varValueNew">The new value</param>
        /// <returns></returns>
        int IVsSelectionEvents.OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Monitor for changes to the active project and notify the entity references user control when a
        /// new SHFB project is selected as the active project.
        /// </summary>
        /// <param name="pHierOld">The hierarchy for the previously selected item</param>
        /// <param name="itemidOld">Project item identifier for the previously selected item</param>
        /// <param name="pMISOld">Interface for previous multi-selection items</param>
        /// <param name="pSCOld">Interface for the previous selection container</param>
        /// <param name="pHierNew">The hierarchy for the new selection</param>
        /// <param name="itemidNew">Project item identifier for the new selection</param>
        /// <param name="pMISNew">Interface for new multi-selection items</param>
        /// <param name="pSCNew">Interface for the new selection container</param>
        /// <returns></returns>
        int IVsSelectionEvents.OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld,
          IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew,
          IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
        {
            SandcastleProject shfbProject = null;
            EntityReferencesControl ucEntityReferences;
            object project;

            if(pHierOld == null || !pHierOld.Equals(pHierNew))
            {
                ucEntityReferences = base.Content as EntityReferencesControl;

                if(ucEntityReferences != null)
                {
                    if(pHierNew != null)
                    {
                        ErrorHandler.ThrowOnFailure(pHierNew.GetProperty(VSConstants.VSITEMID_ROOT,
                            (int)__VSHPROPID.VSHPROPID_ExtObject, out project));

                        EnvDTE.Project envDTEProject = project as EnvDTE.Project;

                        if(envDTEProject != null)
                        {
                            SandcastleBuilderProjectNode projectNode =
                                envDTEProject.Object as SandcastleBuilderProjectNode;

                            if(projectNode != null && projectNode.SandcastleProject != ucEntityReferences.CurrentProject)
                                shfbProject = projectNode.SandcastleProject;
                        }
                    }

                    ucEntityReferences.CurrentProject = shfbProject;
                }
            }

            return VSConstants.S_OK;
        }
        #endregion

        #region Routed event handlers
        //=====================================================================

        /// <summary>
        /// This is used to get information from token, content layout, and site map files open in editors
        /// so that current information is displayed for them in the entity references control.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucEntityReferences_FileContentNeeded(object sender, FileContentNeededEventArgs e)
        {
            IVsUIShell uiShell = Utility.GetServiceFromPackage<IVsUIShell, SVsUIShell>(true);
            IEnumWindowFrames enumFrames;
            IVsWindowFrame[] frames = new IVsWindowFrame[1];
            object docView;
            uint frameCount;
            ContentLayoutEditorPane contentLayoutPane;
            SiteMapEditorPane siteMapPane;
            TokenEditorPane tokenFilePane;

            if(uiShell.GetDocumentWindowEnum(out enumFrames) == VSConstants.S_OK)
                while(enumFrames.Next(1, frames, out frameCount) == VSConstants.S_OK && frameCount == 1)
                    if(frames[0].GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView) == VSConstants.S_OK)
                    {
                        contentLayoutPane = docView as ContentLayoutEditorPane;

                        if(contentLayoutPane != null)
                            e.ContentLayoutFiles.Add(contentLayoutPane.Filename, contentLayoutPane.Topics);
                        else
                        {
                            siteMapPane = docView as SiteMapEditorPane;

                            if(siteMapPane != null)
                                e.SiteMapFiles.Add(siteMapPane.Filename, siteMapPane.Topics);
                            else
                            {
                                tokenFilePane = docView as TokenEditorPane;

                                if(tokenFilePane != null)
                                    e.TokenFiles.Add(tokenFilePane.Filename, tokenFilePane.Tokens);
                            }
                        }
                    }
        }
        #endregion
    }
}
