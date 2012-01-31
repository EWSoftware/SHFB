//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : TopicPreviewerToolWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/22/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to implement the Topic Previewer tool
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
// 1.9.3.4  01/21/2012  EFW  Created the code
//=============================================================================

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using SandcastleBuilder.Package.Editors;
using SandcastleBuilder.Package.Nodes;
using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.WPF.Commands;
using SandcastleBuilder.WPF.UserControls;

namespace SandcastleBuilder.Package.ToolWindows
{
    /// <summary>
    /// This is used to preview conceptual content topics in the project.
    /// </summary>
    [Guid("3764ef30-ce37-4240-a79e-a9cb33073846")]
    public class TopicPreviewerToolWindow : TopicPreviewerToolWindowBase, IVsSelectionEvents
    {
        #region Private data members
        //=====================================================================

        private TopicPreviewerControl ucTopicPreviewer;
        private object scope;
        private uint selectionMonitorCookie;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public TopicPreviewerToolWindow()
        {
            ucTopicPreviewer = new TopicPreviewerControl();

            base.Content = ucTopicPreviewer;

            // Hook up the command bindings and event handlers
            ucTopicPreviewer.CommandBindings.Add(new CommandBinding(EditorCommands.Edit,
                cmdEdit_Executed, cmdEdit_CanExecute));

            ucTopicPreviewer.FileContentNeeded += ucTopicPreviewer_FileContentNeeded;
            ucTopicPreviewer.TopicContentNeeded += ucTopicPreviewer_TopicContentNeeded;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Load the conceptual content information and preview the topics
        /// </summary>
        /// <param name="project">The current project</param>
        /// <param name="previewTopic">The filename of the topic to show as the starting topic or null for the
        /// first topic.</param>
        public void PreviewTopic(SandcastleProject project, string previewTopic)
        {
            if(project == null || ucTopicPreviewer.CurrentProject == null ||
              ucTopicPreviewer.CurrentProject.Filename != project.Filename)
                ucTopicPreviewer.CurrentProject = project;

            ucTopicPreviewer.Refresh();
            ucTopicPreviewer.FindAndDisplay(previewTopic);
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
                    ApplicationCommands.Help.Execute(null, ucTopicPreviewer);
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

                        ucTopicPreviewer.RaiseEvent(e);
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
        /// Monitor for changes to the active project and notify the topic previewer user control when a
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
            object project;

            if(pHierOld == null || !pHierOld.Equals(pHierNew))
            {
                if(ucTopicPreviewer != null)
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

                            if(projectNode != null)
                                shfbProject = projectNode.SandcastleProject;
                        }
                    }

                    if((shfbProject == null && ucTopicPreviewer.CurrentProject != null) ||
                      (shfbProject != null && (ucTopicPreviewer.CurrentProject == null ||
                      ucTopicPreviewer.CurrentProject.Filename != shfbProject.Filename)))
                        ucTopicPreviewer.CurrentProject = shfbProject;
                }
            }

            return VSConstants.S_OK;
        }
        #endregion

        #region Routed event handlers
        //=====================================================================

        /// <summary>
        /// This is used to get information from token and content layoutfiles open in editors so that current
        /// information is displayed for them in the topic previewer control.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucTopicPreviewer_FileContentNeeded(object sender, FileContentNeededEventArgs e)
        {
            IVsUIShell uiShell = Utility.GetServiceFromPackage<IVsUIShell, SVsUIShell>(true);
            IEnumWindowFrames enumFrames;
            IVsWindowFrame[] frames = new IVsWindowFrame[1];
            object docView;
            uint frameCount;
            ContentLayoutEditorPane contentLayoutPane;
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
                            tokenFilePane = docView as TokenEditorPane;

                            if(tokenFilePane != null)
                                e.TokenFiles.Add(tokenFilePane.Filename, tokenFilePane.Tokens);
                        }
                    }
        }

        /// <summary>
        /// This is used to get the content of a specific topic file if it is open in an editor so that the
        /// current content is displayed for it in the topic previewer control.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucTopicPreviewer_TopicContentNeeded(object sender, TopicContentNeededEventArgs e)
        {
            IVsHierarchy hier;
            IVsPersistDocData persistDocData;
            IVsTextStream srpStream;
            IntPtr docData = IntPtr.Zero;
            uint itemid, cookie = 0;
            int hr = VSConstants.E_FAIL;

            IVsRunningDocumentTable rdt = Utility.GetServiceFromPackage<IVsRunningDocumentTable,
                SVsRunningDocumentTable>(true);

            if(rdt == null)
                return;

            try
            {
                // Getting a read lock on the document.  This must be released later.
                hr = rdt.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, e.TopicFilename, out hier,
                    out itemid, out docData, out cookie);

                if(ErrorHandler.Failed(hr) || docData == IntPtr.Zero)
                    return;

                persistDocData = Marshal.GetObjectForIUnknown(docData) as IVsPersistDocData;

                // Try to get the Text lines
                IVsTextLines srpTextLines = persistDocData as IVsTextLines;

                if(srpTextLines == null)
                {
                    // Try getting a text buffer provider first
                    IVsTextBufferProvider srpTextBufferProvider = persistDocData as IVsTextBufferProvider;

                    if(srpTextBufferProvider != null)
                        hr = srpTextBufferProvider.GetTextBuffer(out srpTextLines);
                }

                if(ErrorHandler.Succeeded(hr))
                {
                    srpStream = srpTextLines as IVsTextStream;

                    if(srpStream != null)
                    {
                        IVsBatchUpdate srpBatchUpdate = srpStream as IVsBatchUpdate;

                        if(srpBatchUpdate != null)
                            ErrorHandler.ThrowOnFailure(srpBatchUpdate.FlushPendingUpdates(0));

                        int lBufferSize = 0;
                        hr = srpStream.GetSize(out lBufferSize);

                        if(ErrorHandler.Succeeded(hr))
                        {
                            IntPtr dest = IntPtr.Zero;

                            try
                            {
                                // GetStream() returns Unicode data so we need to double the buffer size
                                dest = Marshal.AllocCoTaskMem((lBufferSize + 1) * 2);
                                ErrorHandler.ThrowOnFailure(srpStream.GetStream(0, lBufferSize, dest));

                                // Get the contents
                                e.TopicContent = Marshal.PtrToStringUni(dest);
                            }
                            finally
                            {
                                if(dest != IntPtr.Zero)
                                    Marshal.FreeCoTaskMem(dest);
                            }
                        }
                    }
                }
            }
            finally
            {
                if(docData != IntPtr.Zero)
                    Marshal.Release(docData);

                if(cookie != 0)
                    ErrorHandler.ThrowOnFailure(rdt.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, cookie));
            }
        }
        #endregion

        #region Command event handlers
        //=====================================================================

        /// <summary>
        /// Determine whether or not the command can execute
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdEdit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            TocEntry t = ucTopicPreviewer.CurrentTopic;

            e.CanExecute = (t != null && t.SourceFile.Exists);
        }

        /// <summary>
        /// Open the selected file for editing
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdEdit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TocEntry t = ucTopicPreviewer.CurrentTopic;

            if(t != null && !String.IsNullOrEmpty(t.SourceFile))
            {
                string fullName = t.SourceFile;

                if(File.Exists(fullName))
                    VsShellUtilities.OpenDocument(this, fullName);
                else
                    Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_INFO, "File does not exist: " + fullName);
            }
            else
                Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_INFO, "No file is associated with this topic");
        }
        #endregion
    }
}