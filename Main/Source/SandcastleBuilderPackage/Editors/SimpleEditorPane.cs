//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SimpleEditorPane.cs
// Author  : Istvan Novak
// Updated : 12/29/2011
// Source  : http://learnvsxnow.codeplex.com/
// Note    : Copyright 2008-2011, Istvan Novak, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that implements the core functionality for an
// editor pane.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/26/2011  EFW  Added the code to the project and updated it to
//                           support WPF user controls.
//=============================================================================

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Constants = Microsoft.VisualStudio.OLE.Interop.Constants;
using Microsoft.VisualStudio.Project.Automation;

namespace SandcastleBuilder.Package.Editors
{
    // ==================================================================================
    /// <summary>
    /// This class provides a base class for simple text or dialog based custom editors.
    /// </summary>
    /// <typeparam name="TFactory">Editor factory for this pane.</typeparam>
    /// <typeparam name="TUIControl">The WPF UI control representing the editor.</typeparam>
    // ==================================================================================
    public abstract class SimpleEditorPane<TFactory, TUIControl> :
      WindowPane,
      IOleCommandTarget,
      IVsPersistDocData,
      IPersistFileFormat
        where TFactory : IVsEditorFactory
        where TUIControl : Control, new()
    {
        #region Constant values

        // --- Our editor will support only one file format, this is its index.
        private const uint FileFormatIndex = 0;

        // --- Character separating file format lines.
        private const char EndLineChar = (char)10;

        #endregion

        #region Private fields

        // --- UI control bound to this editor pane.
        private TUIControl _UIControl;

        // --- File extension used for this editor.
        private readonly string _FileExtensionUsed;

        // --- Guid of the command set belonging to this editor
        private readonly Guid _CommandSetGuid;

        // --- Full path to the file.
        private string _FileName;

        // --- Determines whether an object has changed since being saved to its current file.
        private bool _IsDirty;

        // --- Flag true when we are loading the file. It is used to avoid to change the 
        // --- _IsDirty flag when the changes are related to the load operation.
        private bool _Loading;

        // --- This flag is true when we are asking the QueryEditQuerySave service if we can 
        // --- edit the file. It is used to avoid to have more than one request queued.
        private bool _GettingCheckoutStatus;

        // --- Indicate that object is in NoScribble mode or in Normal mode. Object enter 
        // --- into the NoScribble mode when IPersistFileFormat.Save() call is occurred.
        // --- This flag using to indicate SaveCompleted state (entering into the Normal mode).
        private bool _NoScribbleMode;

        // Accelerator key scope
        private object scope;
        #endregion

        #region Lifecycle methods

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Creates a new instance of this editor pane.
        /// </summary>
        /// <remarks>
        /// Creates a new instance of the UI control associated with the editor pane.
        /// </remarks>
        // --------------------------------------------------------------------------------
        protected SimpleEditorPane() : base(null)
        {
            _FileExtensionUsed = GetFileExtension();
            _CommandSetGuid = GetCommandSetGuid();
            base.Content = _UIControl = new TUIControl();
        }

        #endregion

        #region Abstract and virtual members to override

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Gets the file extension used by this editor.
        /// </summary>
        /// <returns>
        /// File extension used by the editor.
        /// </returns>
        /// <remarks>
        /// This method is called only once at construction time. The returned file 
        /// extension must contain the leading "." character.
        /// </remarks>
        // --------------------------------------------------------------------------------
        protected abstract string GetFileExtension();

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Gets the GUID of the command set related to this editor.
        /// </summary>
        /// <returns>
        /// GUID of the command set for this editor.
        /// </returns>
        // --------------------------------------------------------------------------------
        protected abstract Guid GetCommandSetGuid();

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flag indicating if this editor pane supports any of the VSStd97
        /// commands.
        /// </summary>
        /// <param name="commandID">ID of the VSStd97 command.</param>
        /// <param name="status">Status of the command.</param>
        /// <returns>
        /// True, if the editor pane supports the command; otherwise, false.
        /// </returns>
        /// <remarks>
        /// This methods returns false. If a derived class overrides it, it also must
        /// set the status accordingly.
        /// </remarks>
        // --------------------------------------------------------------------------------
        protected virtual bool SupportsVSStd97Command(uint commandID, ref OLECMDF status)
        {
            return false;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Executes a VSStd97 command that are not in the standard command set (Cut,
        /// Copy, Paste, SelectAll, Undo, Redo).
        /// </summary>
        /// <param name="execArgs">Command execution arguments.</param>
        /// <returns>
        /// True, if the editor pane supports the command; otherwise, false.
        /// </returns>
        /// <remarks>
        /// This methods returns false. If a derived class overrides it, it also must
        /// return the support status accordingly.
        /// </remarks>
        // --------------------------------------------------------------------------------
        protected virtual bool ExecuteVSStd97Command(ExecArgs execArgs)
        {
            return false;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flag indicating if this editor pane supports a command belonging to 
        /// its command group.
        /// </summary>
        /// <param name="queryStatusArgs">Arguments to set up the command status.</param>
        /// <returns>
        /// True, if the editor pane supports the command; otherwise, false.
        /// </returns>
        /// <remarks>
        /// This methods returns false. If a derived class overrides it, it also must
        /// set the status accordingly.
        /// </remarks>
        // --------------------------------------------------------------------------------
        protected virtual bool SupportsOwnedCommand(QueryStatusArgs queryStatusArgs)
        {
            return false;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Executes command belonging to this editor's command group.
        /// </summary>
        /// <param name="execArgs">Command execution arguments.</param>
        /// <returns>
        /// True, if the editor pane supports the command; otherwise, false.
        /// </returns>
        /// <remarks>
        /// This methods returns false. If a derived class overrides it, it also must
        /// return the support status accordingly.
        /// </remarks>
        // --------------------------------------------------------------------------------
        protected virtual bool ExecuteOwnedCommand(ExecArgs execArgs)
        {
            return false;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flag indicating if this editor pane supports any command.
        /// </summary>
        /// <param name="queryStatusArgs">Arguments to set up the command status.</param>
        /// <returns>
        /// True, if the editor pane supports the command; otherwise, false.
        /// </returns>
        /// <remarks>
        /// This methods returns false. If a derived class overrides it, it also must
        /// set the status accordingly.
        /// </remarks>
        // --------------------------------------------------------------------------------
        protected virtual bool SupportsCommand(QueryStatusArgs queryStatusArgs)
        {
            return false;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Executes a command.
        /// </summary>
        /// <param name="execArgs">Command execution arguments.</param>
        /// <returns>
        /// True, if the editor pane supports the command; otherwise, false.
        /// </returns>
        /// <remarks>
        /// This methods returns false. If a derived class overrides it, it also must
        /// return the support status accordingly.
        /// </remarks>
        // --------------------------------------------------------------------------------
        protected virtual bool ExecuteCommand(ExecArgs execArgs)
        {
            return false;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Override this abstract method to define how to load the editor content from a
        /// file.
        /// </summary>
        /// <param name="fileName">Name of the file to load the content from.</param>
        // --------------------------------------------------------------------------------
        protected abstract void LoadFile(string fileName);

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Override this abstract method to define how to save the editor content into a
        /// file.
        /// </summary>
        /// <param name="fileName">Name of the file to save the content to.</param>
        // --------------------------------------------------------------------------------
        protected abstract void SaveFile(string fileName);

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Use this method to sign that the content of the editor has been changed.
        /// </summary>
        // --------------------------------------------------------------------------------
        protected virtual void OnContentChanged()
        {
            // --- During the load operation the text of the control will change, but
            // --- this change must not be stored in the status of the document.
            if(!_Loading)
            {
                // --- The only interesting case is when we are changing the document
                // --- for the first time
                if(!_IsDirty)
                {
                    // Check if the QueryEditQuerySave service allow us to change the file
                    if(!CanEditFile())
                    {
                        // --- We can not change the file (e.g. a checkout operation failed),
                        // --- so undo the change and exit.
                        var commandSupport = this.CommonCommandSupport;

                        if(commandSupport != null && commandSupport.SupportsUndo)
                            commandSupport.DoUndo();

                        return;
                    }

                    // --- It is possible to change the file, so update the status.
                    _IsDirty = true;
                }
            }
        }

        #endregion

        #region Advanced virtual members (rarely to override)

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Returns the path to the object's current working file.
        /// </summary>
        /// <param name="ppszFilename">Pointer to the file name.</param>
        /// <param name="pnFormatIndex">
        /// Value that indicates the current format of the file as a zero based index into 
        /// the list of formats. Since we support only a single format, we need to 
        /// return zero. Subsequently, we will return a single element in the format list 
        /// through a call to GetFormatList.
        /// </param>
        /// <returns>S_OK if the function succeeds.</returns>
        // --------------------------------------------------------------------------------
        protected virtual int OnGetCurFile(out string ppszFilename, out uint pnFormatIndex)
        {
            // --- We only support 1 format so return its index
            pnFormatIndex = FileFormatIndex;
            ppszFilename = _FileName;
            return VSConstants.S_OK;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Provides the caller with the information necessary to open the standard 
        /// common "Save As" dialog box. This returns an enumeration of supported formats, 
        /// from which the caller selects the appropriate format.
        /// Each string for the format is terminated with a newline (\n) character. 
        /// The last string in the buffer must be terminated with the newline character 
        /// as well. The first string in each pair is a display string that describes the 
        /// filter, such as "Text Only (*.txt)". The second string specifies the filter 
        /// pattern, such as "*.txt". To specify multiple filter patterns for a single 
        /// display string, use a semicolon to separate the patterns: "*.htm;*.html;*.asp". 
        /// A pattern string can be a combination of valid file name characters and the 
        /// asterisk (*) wildcard character. Do not include spaces in the pattern string. 
        /// The following string is an example of a file pattern string: 
        /// "HTML File (*.htm; *.html; *.asp)\n*.htm;*.html;*.asp\nText File (*.txt)\n*.txt\n."
        /// </summary>
        /// <param name="ppszFormatList">
        /// Pointer to a string that contains pairs of format filter strings.
        /// </param>
        /// <returns>S_OK if the method succeeds.</returns>
        // --------------------------------------------------------------------------------
        protected virtual int OnGetFormatList(out string ppszFormatList)
        {
            string formatList =
              string.Format(CultureInfo.CurrentCulture,
              "Editor Files (*{0}){1}*{0}{1}{1}",
              FileExtensionUsed, EndLineChar);
            ppszFormatList = formatList;
            return VSConstants.S_OK;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Notifies the object that it has concluded the Save transaction.
        /// </summary>
        /// <param name="pszFilename">Pointer to the file name.</param>
        /// <returns>S_OK if the function succeeds.</returns>
        // --------------------------------------------------------------------------------
        protected virtual int OnSaveCompleted(string pszFilename)
        {
            return _NoScribbleMode ? VSConstants.S_FALSE : VSConstants.S_OK;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Initialization for the object.
        /// </summary>
        /// <param name="nFormatIndex">
        /// Zero based index into the list of formats that indicates the current format
        /// of the file.
        /// </param>
        /// <returns>S_OK if the method succeeds.</returns>
        // --------------------------------------------------------------------------------
        protected virtual int OnInitNew(uint nFormatIndex)
        {
            if(nFormatIndex != FileFormatIndex)
            {
                throw new ArgumentException("Unknown format");
            }
            // --- Until someone change the file, we can consider it not dirty as
            // --- the user would be annoyed if we prompt him to save an empty file
            _IsDirty = false;
            return VSConstants.S_OK;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Determines whether an object has changed since being saved to its current file.
        /// </summary>
        /// <param name="pfIsDirty">true if the document has changed.</param>
        /// <returns>S_OK if the method succeeds.</returns>
        // --------------------------------------------------------------------------------
        protected virtual int OnIsDirty(out int pfIsDirty)
        {
            pfIsDirty = _IsDirty ? 1 : 0;
            return VSConstants.S_OK;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Close the IVsPersistDocData object.
        /// </summary>
        /// <returns>S_OK if the function succeeds.</returns>
        // --------------------------------------------------------------------------------
        protected virtual int OnCloseEditor()
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region Public properties

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Gets the ID of the editor factory creating instances of this editor pane.
        /// </summary>
        // --------------------------------------------------------------------------------
        public Guid FactoryGuid
        {
            get { return typeof(TFactory).GUID; }
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Gets the file extension used by the editor.
        /// </summary>
        // --------------------------------------------------------------------------------
        public string FileExtensionUsed
        {
            get { return _FileExtensionUsed; }
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Gets the UI control associated with the editor.
        /// </summary>
        // --------------------------------------------------------------------------------
        public TUIControl UIControl
        {
            get { return _UIControl; }
        }

        /// <summary>
        /// This is used to get the <see cref="ICommonCommandSupport"/> interface
        /// </summary>
        /// <value>If the control supports it, its interface will be returned.  If not but the pane
        /// supports it, it will be returned.  If neither support it, null is returned.</value>
        public ICommonCommandSupport CommonCommandSupport
        {
            get
            {
                var commandSupport = _UIControl as ICommonCommandSupport;

                if(commandSupport == null)
                    commandSupport = this as ICommonCommandSupport;

                return commandSupport;
            }
        }

        /// <summary>
        /// This read-only property returns the dirty state of the file
        /// </summary>
        public bool IsDirty
        {
            get { return _IsDirty; }
        }

        /// <summary>
        /// This read-only property returns the file node associated with the document being edited
        /// </summary>
        public FileNode FileNode
        {
            get
            {
                IVsHierarchy hierarchy;
                uint itemID;
                IntPtr docData;
                uint docCookie = 0;
                object node = null;

                // --- Make sure that we have a file name
                if(_FileName.Length == 0)
                    return null;

                // --- Get a reference to the Running Document Table
                IVsRunningDocumentTable runningDocTable = (IVsRunningDocumentTable)GetService(
                    typeof(SVsRunningDocumentTable));

                if(runningDocTable == null)
                    return null;

                try
                {
                    // --- Lock the document
                    ErrorHandler.ThrowOnFailure(runningDocTable.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_ReadLock,
                        _FileName, out hierarchy,out itemID, out docData, out docCookie));

                    ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(itemID, (int)__VSHPROPID.VSHPROPID_ExtObject,
                        out node));
                }
                finally
                {
                    // --- Unlock the document.
                    // Note that we have to unlock the document even if the previous call failed.
                    if(runningDocTable != null)
                        ErrorHandler.ThrowOnFailure(runningDocTable.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock,
                            docCookie));
                }

                if(node == null)
                    return null;

                return ((OAFileItem)node).Object as FileNode;
            }
        }
        #endregion

        #region IOleCommandTarget Members

        // --------------------------------------------------------------------------------
        /// <summary>
        /// The shell calls this function to know if a menu item should be visible and
        /// if it should be enabled/disabled. This function will only be called when an 
        /// instance of this editor is open.
        /// </summary>
        /// <param name="pguidCmdGroup">
        /// Guid describing which set of command the current command(s) belong to.
        /// </param>
        /// <param name="cCmds">
        /// Number of command which status are being asked for.
        /// </param>
        /// <param name="prgCmds">Information for each command.</param>
        /// <param name="pCmdText">Used to dynamically change the command text.</param>
        /// <returns>S_OK if the method succeeds.</returns> 
        // --------------------------------------------------------------------------------
        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds,
          IntPtr pCmdText)
        {
            // --- Validate parameters
            if(prgCmds == null || cCmds != 1)
                return VSConstants.E_INVALIDARG;

            // --- Wrap parameters into argument type instance
            QueryStatusArgs statusArgs = new QueryStatusArgs(pguidCmdGroup);
            statusArgs.CommandCount = cCmds;
            statusArgs.Commands = prgCmds;
            statusArgs.PCmdText = pCmdText;

            // --- By default all commands are supported
            OLECMDF cmdf = OLECMDF.OLECMDF_SUPPORTED;
            var commandSupport = this.CommonCommandSupport;

            if(pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
            {
                // --- Process standard Commands
                switch(prgCmds[0].cmdID)
                {
                    case (uint)VSConstants.VSStd97CmdID.SelectAll:
                        if(commandSupport != null && commandSupport.SupportsSelectAll)
                            cmdf |= OLECMDF.OLECMDF_ENABLED;
                        break;

                    case (uint)VSConstants.VSStd97CmdID.Copy:
                        if(commandSupport != null && commandSupport.SupportsCopy)
                            cmdf |= OLECMDF.OLECMDF_ENABLED;
                        break;

                    case (uint)VSConstants.VSStd97CmdID.Cut:
                        if(commandSupport != null && commandSupport.SupportsCut)
                            cmdf |= OLECMDF.OLECMDF_ENABLED;
                        break;

                    case (uint)VSConstants.VSStd97CmdID.Paste:
                        if(commandSupport != null && commandSupport.SupportsPaste)
                            cmdf |= OLECMDF.OLECMDF_ENABLED;
                        break;

                    case (uint)VSConstants.VSStd97CmdID.Redo:
                        if(commandSupport != null && commandSupport.SupportsRedo)
                            cmdf |= OLECMDF.OLECMDF_ENABLED;
                        break;

                    case (uint)VSConstants.VSStd97CmdID.Undo:
                        if(commandSupport != null && commandSupport.SupportsUndo)
                            cmdf |= OLECMDF.OLECMDF_ENABLED;
                        break;

                    default:
                        if(!SupportsVSStd97Command(prgCmds[0].cmdID, ref cmdf))
                            return (int)(Constants.OLECMDERR_E_NOTSUPPORTED);
                        break;
                }

                // --- Pass back the commmand support flag
                prgCmds[0].cmdf = (uint)cmdf;
                return VSConstants.S_OK;
            }

            // --- Check for commands owned by the editor
            else if(pguidCmdGroup == _CommandSetGuid)
            {
                return SupportsOwnedCommand(statusArgs)
                         ? VSConstants.S_OK
                         : (int)(Constants.OLECMDERR_E_NOTSUPPORTED);
            }

            // --- Check for any other commands
            return SupportsCommand(statusArgs)
                     ? VSConstants.S_OK
                     : (int)(Constants.OLECMDERR_E_NOTSUPPORTED);
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Execute a specified command.
        /// </summary>
        /// <param name="pguidCmdGroup">
        /// Guid describing which set of command the current command(s) belong to.
        /// </param>
        /// <param name="nCmdID">Command that should be executed.</param>
        /// <param name="nCmdexecopt">Options for the command.</param>
        /// <param name="pvaIn">Pointer to input arguments.</param>
        /// <param name="pvaOut">Pointer to command output.</param>
        /// <returns>
        /// S_OK if the method succeeds or OLECMDERR_E_NOTSUPPORTED on unsupported command.
        /// </returns> 
        /// <remarks>
        /// Typically, only the first 2 arguments are used (to identify which command 
        /// should be run).
        /// </remarks>
        // --------------------------------------------------------------------------------
        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt,
          IntPtr pvaIn, IntPtr pvaOut)
        {
            // --- Wrap parameters into argument type instance
            ExecArgs execArgs = new ExecArgs(pguidCmdGroup, nCmdID);
            execArgs.CommandExecOpt = nCmdexecopt;
            execArgs.PvaIn = pvaIn;
            execArgs.PvaOut = pvaOut;

            var commandSupport = this.CommonCommandSupport;

            if(pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
            {
                // --- Process standard Visual Studio Commands
                switch(nCmdID)
                {
                    case (uint)VSConstants.VSStd97CmdID.Copy:
                        if(commandSupport != null)
                            commandSupport.DoCopy();
                        return VSConstants.S_OK;

                    case (uint)VSConstants.VSStd97CmdID.Cut:
                        if(commandSupport != null)
                            commandSupport.DoCut();
                        return VSConstants.S_OK;

                    case (uint)VSConstants.VSStd97CmdID.Paste:
                        if(commandSupport != null)
                            commandSupport.DoPaste();
                        return VSConstants.S_OK;

                    case (uint)VSConstants.VSStd97CmdID.Redo:
                        if(commandSupport != null)
                            commandSupport.DoRedo();
                        return VSConstants.S_OK;

                    case (uint)VSConstants.VSStd97CmdID.Undo:
                        if(commandSupport != null)
                            commandSupport.DoUndo();
                        return VSConstants.S_OK;

                    case (uint)VSConstants.VSStd97CmdID.SelectAll:
                        if(commandSupport != null)
                            commandSupport.DoSelectAll();
                        return VSConstants.S_OK;

                    default:
                        return ExecuteVSStd97Command(execArgs)
                          ? VSConstants.S_OK
                          : (int)(Constants.OLECMDERR_E_NOTSUPPORTED);
                }
            }

            // --- Execute commands owned by the editor
            else if(pguidCmdGroup == _CommandSetGuid)
            {
                return ExecuteOwnedCommand(execArgs)
                  ? VSConstants.S_OK
                  : (int)(Constants.OLECMDERR_E_NOTSUPPORTED);
            }

            // --- Execute any other command
            return ExecuteCommand(execArgs)
              ? VSConstants.S_OK
              : (int)(Constants.OLECMDERR_E_NOTSUPPORTED);
        }

        #endregion

        #region IPersist Members

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the class identifier (CLSID) of an object.
        /// </summary>
        /// <param name="pClassID">
        /// Class identifier of the object.
        /// </param>
        /// <returns>S_OK if the method succeeds.</returns>
        // --------------------------------------------------------------------------------
        int IPersist.GetClassID(out Guid pClassID)
        {
            pClassID = FactoryGuid;
            return VSConstants.S_OK;
        }

        #endregion

        #region IPersistFileFormat Members

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the class identifier (CLSID) of an object.
        /// </summary>
        /// <param name="pClassID">
        /// Class identifier of the object.
        /// </param>
        /// <returns>S_OK if the method succeeds.</returns>
        // --------------------------------------------------------------------------------
        int IPersistFileFormat.GetClassID(out Guid pClassID)
        {
            pClassID = FactoryGuid;
            return VSConstants.S_OK;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Determines whether an object has changed since being saved to its current file.
        /// </summary>
        /// <param name="pfIsDirty">true if the document has changed.</param>
        /// <returns>S_OK if the method succeeds.</returns>
        /// <remarks>
        /// Override the <see cref="OnIsDirty"/> method to change the behaviour.
        /// </remarks>
        // --------------------------------------------------------------------------------
        int IPersistFileFormat.IsDirty(out int pfIsDirty)
        {
            return OnIsDirty(out pfIsDirty);
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Initialization for the object.
        /// </summary>
        /// <param name="nFormatIndex">
        /// Zero based index into the list of formats that indicates the current format
        /// of the file.
        /// </param>
        /// <returns>S_OK if the method succeeds.</returns>
        /// <remarks>
        /// Override the <see cref="OnInitNew"/> method to change the behaviour.
        /// </remarks>
        // --------------------------------------------------------------------------------
        int IPersistFileFormat.InitNew(uint nFormatIndex)
        {
            return OnInitNew(nFormatIndex);
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Loads the file content into the editor (into the controls representing the UI).
        /// </summary>
        /// <param name="pszFilename">
        /// Pointer to the full path name of the file to load.
        /// </param>
        /// <param name="grfMode">File format mode.</param>
        /// <param name="fReadOnly">
        /// determines if the file should be opened as read only.
        /// </param>
        /// <returns>S_OK if the method succeeds.</returns>
        // --------------------------------------------------------------------------------
        int IPersistFileFormat.Load(string pszFilename, uint grfMode, int fReadOnly)
        {
            // --- A valid file name is required.
            if((pszFilename == null) && ((_FileName == null) || (_FileName.Length == 0)))
                throw new ArgumentNullException("pszFilename");

            _Loading = true;
            int hr = VSConstants.S_OK;
            try
            {
                // --- If the new file name is null, then this operation is a reload
                bool isReload = false;
                if(pszFilename == null)
                {
                    isReload = true;
                }

                // --- Show the wait cursor while loading the file
                Utility.GetServiceFromPackage<IVsUIShell, SVsUIShell>(true).SetWaitCursor();

                // --- Set the new file name
                if(!isReload)
                {
                    // --- Unsubscribe from the notification of the changes in the previous file.
                    _FileName = pszFilename;
                }
                // --- Load the file
                LoadFile(_FileName);
                _IsDirty = false;

                // --- Notify the load or reload
                NotifyDocChanged();
            }
            finally
            {
                _Loading = false;
            }
            return hr;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Save the contents of the editor into the specified file. If doing the save 
        /// on the same file, we need to suspend notifications for file changes during 
        /// the save operation.
        /// </summary>
        /// <param name="pszFilename">
        /// Pointer to the file name. If the pszFilename parameter is a null reference 
        /// we need to save using the current file.
        /// </param>
        /// <param name="fRemember">
        /// Boolean value that indicates whether the pszFileName parameter is to be used 
        /// as the current working file.
        /// If remember != 0, pszFileName needs to be made the current file and the 
        /// dirty flag needs to be cleared after the save. Also, file notifications need 
        /// to be enabled for the new file and disabled for the old file.
        /// If remember == 0, this save operation is a Save a Copy As operation. In this 
        /// case, the current file is unchanged and dirty flag is not cleared.
        /// </param>
        /// <param name="nFormatIndex">
        /// Zero based index into the list of formats that indicates the format in which 
        /// the file will be saved.
        /// </param>
        /// <returns>S_OK if the method succeeds.</returns>
        // --------------------------------------------------------------------------------
        int IPersistFileFormat.Save(string pszFilename, int fRemember, uint nFormatIndex)
        {
            // --- switch into the NoScribble mode
            _NoScribbleMode = true;
            try
            {
                // --- If file is null or same --> SAVE
                if(pszFilename == null || pszFilename == _FileName)
                {
                    SaveFile(_FileName);
                    _IsDirty = false;
                }
                else
                {
                    // --- If remember --> SaveAs 
                    if(fRemember != 0)
                    {
                        _FileName = pszFilename;
                        SaveFile(_FileName);
                        _IsDirty = false;
                    }
                    else // --- Else, Save a Copy As
                    {
                        SaveFile(pszFilename);
                    }
                }
            }
            finally
            {
                // --- Switch into the Normal mode
                _NoScribbleMode = false;
            }
            return VSConstants.S_OK;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Notifies the object that it has concluded the Save transaction.
        /// </summary>
        /// <param name="pszFilename">Pointer to the file name.</param>
        /// <returns>S_OK if the function succeeds.</returns>
        /// <remarks>
        /// Override the <see cref="OnSaveCompleted"/> method to change the behaviour.
        /// </remarks>
        // --------------------------------------------------------------------------------
        int IPersistFileFormat.SaveCompleted(string pszFilename)
        {
            return OnSaveCompleted(pszFilename);
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Returns the path to the object's current working file.
        /// </summary>
        /// <param name="ppszFilename">Pointer to the file name.</param>
        /// <param name="pnFormatIndex">
        /// Value that indicates the current format of the file as a zero based index into 
        /// the list of formats. Since we support only a single format, we need to 
        /// return zero. Subsequently, we will return a single element in the format list 
        /// through a call to GetFormatList.
        /// </param>
        /// <returns>S_OK if the function succeeds.</returns>
        /// <remarks>
        /// Override the <see cref="OnGetCurFile"/> method to change the behaviour.
        /// </remarks>
        // --------------------------------------------------------------------------------
        int IPersistFileFormat.GetCurFile(out string ppszFilename, out uint pnFormatIndex)
        {
            return OnGetCurFile(out ppszFilename, out pnFormatIndex);
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Provides the caller with the information necessary to open the standard 
        /// common "Save As" dialog box. 
        /// </summary>
        /// <param name="ppszFormatList">
        /// Pointer to a string that contains pairs of format filter strings.
        /// </param>
        /// <returns>S_OK if the method succeeds.</returns>
        /// <remarks>
        /// Override the <see cref="OnGetFormatList"/> method to change the behaviour.
        /// </remarks>
        // --------------------------------------------------------------------------------
        int IPersistFileFormat.GetFormatList(out string ppszFormatList)
        {
            return OnGetFormatList(out ppszFormatList);
        }

        #endregion

        #region IVsPersistDocData Members

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Returns the Guid of the editor factory that created the IVsPersistDocData 
        /// object.
        /// </summary>
        /// <param name="pClassID">
        /// Pointer to the class identifier of the editor type.
        /// </param>
        /// <returns>S_OK if the method succeeds.</returns>
        // --------------------------------------------------------------------------------
        int IVsPersistDocData.GetGuidEditorType(out Guid pClassID)
        {
            pClassID = FactoryGuid;
            return VSConstants.S_OK;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Used to determine if the document data has changed since the last time it was 
        /// saved.
        /// </summary>
        /// <param name="pfDirty">Will be set to 1 if the data has changed.</param>
        /// <returns>S_OK if the function succeeds.</returns>
        // --------------------------------------------------------------------------------
        int IVsPersistDocData.IsDocDataDirty(out int pfDirty)
        {
            return ((IPersistFileFormat)this).IsDirty(out pfDirty);
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Used to set the initial name for unsaved, newly created document data.
        /// </summary>
        /// <param name="pszDocDataPath">String containing the path to the document.
        /// We need to ignore this parameter.
        /// </param>
        /// <returns>S_OK if the method succeeds.</returns>
        // --------------------------------------------------------------------------------
        int IVsPersistDocData.SetUntitledDocPath(string pszDocDataPath)
        {
            return ((IPersistFileFormat)this).InitNew(FileFormatIndex);
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Loads the document data from the file specified.
        /// </summary>
        /// <param name="pszMkDocument">
        /// Path to the document file which needs to be loaded.
        /// </param>
        /// <returns>S_OK if the method succeeds.</returns>
        // --------------------------------------------------------------------------------
        int IVsPersistDocData.LoadDocData(string pszMkDocument)
        {
            return ((IPersistFileFormat)this).Load(pszMkDocument, 0, 0);
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Saves the document data. Before actually saving the file, we first need to 
        /// indicate to the environment that a file is about to be saved. This is done 
        /// through the "SVsQueryEditQuerySave" service. We call the "QuerySaveFile" 
        /// function on the service instance and then proceed depending on the result 
        /// returned as follows:
        /// 
        /// If result is QSR_SaveOK - We go ahead and save the file and the file is not 
        /// read only at this point.
        /// 
        /// If result is QSR_ForceSaveAs - We invoke the "Save As" functionality which will 
        /// bring up the Save file name dialog.
        /// 
        /// If result is QSR_NoSave_Cancel - We cancel the save operation and indicate that 
        /// the document could not be saved by setting the "pfSaveCanceled" flag.
        /// 
        /// If result is QSR_NoSave_Continue - Nothing to do here as the file need not be 
        /// saved.
        /// </summary>
        /// <param name="dwSave">Flags which specify the file save options:
        /// VSSAVE_Save        - Saves the current file to itself.
        /// VSSAVE_SaveAs      - Prompts the User for a filename and saves the file to 
        ///                      the file specified.
        /// VSSAVE_SaveCopyAs  - Prompts the user for a filename and saves a copy of the 
        ///                      file with a name specified.
        /// VSSAVE_SilentSave  - Saves the file without prompting for a name or confirmation.  
        /// </param>
        /// <param name="pbstrMkDocumentNew">Pointer to the path to the new document.</param>
        /// <param name="pfSaveCanceled">Value 1 if the document could not be saved.</param>
        /// <returns>S_OK if the method succeeds.</returns>
        // --------------------------------------------------------------------------------
        [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
        int IVsPersistDocData.SaveDocData(VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
        {
            pbstrMkDocumentNew = null;
            pfSaveCanceled = 0;
            int hr;

            switch(dwSave)
            {
                case VSSAVEFLAGS.VSSAVE_Save:
                case VSSAVEFLAGS.VSSAVE_SilentSave:
                    {
                        IVsQueryEditQuerySave2 queryEditQuerySave =
                          (IVsQueryEditQuerySave2)GetService(typeof(SVsQueryEditQuerySave));

                        // Call QueryEditQuerySave
                        uint result;
                        hr = queryEditQuerySave.QuerySaveFile(
                          _FileName, // filename
                          0, // flags
                          null, // file attributes
                          out result); // result

                        if(ErrorHandler.Failed(hr))
                        {
                            return hr;
                        }

                        // Process according to result from QuerySave
                        switch((tagVSQuerySaveResult)result)
                        {
                            case tagVSQuerySaveResult.QSR_NoSave_Cancel:
                                // Note that this is also case tagVSQuerySaveResult.QSR_NoSave_UserCanceled because these
                                // two tags have the same value.
                                pfSaveCanceled = ~0;
                                break;

                            case tagVSQuerySaveResult.QSR_SaveOK:
                                {
                                    // Call the shell to do the save for us
                                    hr = Utility.GetServiceFromPackage<IVsUIShell, SVsUIShell>(true).SaveDocDataToFile(
                                        dwSave, this, _FileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                                    if(ErrorHandler.Failed(hr))
                                        return hr;
                                }
                                break;

                            case tagVSQuerySaveResult.QSR_ForceSaveAs:
                                {
                                    // Call the shell to do the SaveAS for us
                                    hr = Utility.GetServiceFromPackage<IVsUIShell, SVsUIShell>(true).SaveDocDataToFile(
                                        VSSAVEFLAGS.VSSAVE_SaveAs, this, _FileName, out pbstrMkDocumentNew,
                                        out pfSaveCanceled);
                                    if(ErrorHandler.Failed(hr))
                                        return hr;
                                }
                                break;

                            case tagVSQuerySaveResult.QSR_NoSave_Continue:
                                // In this case there is nothing to do.
                                break;

                            default:
                                throw new InvalidOperationException("Unsupported result from Query Edit/Query Save");
                        }
                        break;
                    }
                case VSSAVEFLAGS.VSSAVE_SaveAs:
                case VSSAVEFLAGS.VSSAVE_SaveCopyAs:
                    {
                        // Make sure the file name as the right extension
                        if(String.Compare(FileExtensionUsed, Path.GetExtension(_FileName), true, CultureInfo.CurrentCulture) != 0)
                        {
                            _FileName += FileExtensionUsed;
                        }
                        // Call the shell to do the save for us
                        hr = Utility.GetServiceFromPackage<IVsUIShell, SVsUIShell>(true).SaveDocDataToFile(dwSave,
                            this, _FileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                        if(ErrorHandler.Failed(hr))
                            return hr;
                        break;
                    }
                default:
                    throw new ArgumentException("Unsupported save flag value");
            }
            return VSConstants.S_OK;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Close the IVsPersistDocData object.
        /// </summary>
        /// <returns>S_OK if the function succeeds.</returns>
        /// <remarks>
        /// Override the <see cref="OnCloseEditor"/> method to change the behaviour.
        /// </remarks>
        // --------------------------------------------------------------------------------
        int IVsPersistDocData.Close()
        {
            return OnCloseEditor();
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Called by the Running Document Table when it registers the document data. 
        /// </summary>
        /// <param name="docCookie">Handle for the document to be registered.</param>
        /// <param name="pHierNew">Pointer to the IVsHierarchy interface.</param>
        /// <param name="itemidNew">
        /// Item identifier of the document to be registered from VSITEM.
        /// </param>
        /// <returns>S_OK if the method succeeds.</returns>
        // --------------------------------------------------------------------------------
        int IVsPersistDocData.OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew,
          uint itemidNew)
        {
            return VSConstants.S_OK;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Renames the document data.
        /// </summary>
        /// <param name="grfAttribs">
        /// File attribute of the document data to be renamed. See the data type 
        /// __VSRDTATTRIB.
        /// </param>
        /// <param name="pHierNew">
        /// Pointer to the IVsHierarchy interface of the document being renamed.
        /// </param>
        /// <param name="itemidNew">
        /// Item identifier of the document being renamed. See the data type VSITEMID.
        /// </param>
        /// <param name="pszMkDocumentNew">Path to the document being renamed.</param>
        /// <returns>S_OK if the method succeeds.</returns>
        // --------------------------------------------------------------------------------
        int IVsPersistDocData.RenameDocData(uint grfAttribs, IVsHierarchy pHierNew,
          uint itemidNew, string pszMkDocumentNew)
        {
            return VSConstants.S_OK;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Determines if it is possible to reload the document data.
        /// </summary>
        /// <param name="pfReloadable">set to 1 if the document can be reloaded.</param>
        /// <returns>S_OK if the method succeeds.</returns>
        // --------------------------------------------------------------------------------
        int IVsPersistDocData.IsDocDataReloadable(out int pfReloadable)
        {
            // --- Allow file to be reloaded
            pfReloadable = 1;
            return VSConstants.S_OK;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Reloads the document data.
        /// </summary>
        /// <param name="grfFlags">
        /// Flag indicating whether to ignore the next file change when reloading the 
        /// document data. This flag should not be set for us since we implement the 
        /// "IVsDocDataFileChangeControl" interface in order to indicate ignoring of file 
        /// changes.
        /// </param>
        /// <returns>S_OK if the method succeeds.</returns>
        // --------------------------------------------------------------------------------
        int IVsPersistDocData.ReloadDocData(uint grfFlags)
        {
            return ((IPersistFileFormat)this).Load(null, grfFlags, 0);
        }

        #endregion

        #region Private methods

        // --------------------------------------------------------------------------------
        /// <summary>
        /// This function asks to the QueryEditQuerySave service if it is possible to
        /// edit the file.
        /// </summary>
        /// <returns>
        /// True if the editing of the file are enabled, otherwise returns false.
        /// </returns>
        // --------------------------------------------------------------------------------
        private bool CanEditFile()
        {
            // --- Check the status of the recursion guard
            if(_GettingCheckoutStatus)
            {
                return false;
            }

            try
            {
                // Set the recursion guard
                _GettingCheckoutStatus = true;

                // Get the QueryEditQuerySave service
                IVsQueryEditQuerySave2 queryEditQuerySave = (IVsQueryEditQuerySave2)GetService(typeof(SVsQueryEditQuerySave));

                // Now call the QueryEdit method to find the edit status of this file
                string[] documents = { _FileName };
                uint result;
                uint outFlags;

                // This function can pop up a dialog to ask the user to checkout the file.
                // When this dialog is visible, it is possible to receive other request to change
                // the file and this is the reason for the recursion guard.
                int hr = queryEditQuerySave.QueryEditFiles(
                  0, // Flags
                  1, // Number of elements in the array
                  documents, // Files to edit
                  null, // Input flags
                  null, // Input array of VSQEQS_FILE_ATTRIBUTE_DATA
                  out result, // result of the checkout
                  out outFlags // Additional flags
                  );
                if(ErrorHandler.Succeeded(hr) && (result == (uint)tagVSQueryEditResult.QER_EditOK))
                {
                    // In this case (and only in this case) we can return true from this function.
                    return true;
                }
            }
            finally
            {
                _GettingCheckoutStatus = false;
            }
            return false;
        }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Gets an instance of the RunningDocumentTable (RDT) service which manages the 
        /// set of currently open documents in the environment and then notifies the 
        /// client that an open document has changed.
        /// </summary>
        // --------------------------------------------------------------------------------
        private void NotifyDocChanged()
        {
            IVsHierarchy hierarchy;
            uint itemID;
            IntPtr docData;
            uint docCookie = 0;

            // --- Make sure that we have a file name
            if(_FileName.Length == 0)
                return;

            // --- Get a reference to the Running Document Table
            IVsRunningDocumentTable runningDocTable = (IVsRunningDocumentTable)GetService(
                typeof(SVsRunningDocumentTable));

            try
            {
                // --- Lock the document
                ErrorHandler.ThrowOnFailure(runningDocTable.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_ReadLock,
                    _FileName, out hierarchy,out itemID, out docData, out docCookie));

                // --- Send the notification
                ErrorHandler.ThrowOnFailure(runningDocTable.NotifyDocumentChanged(docCookie,
                    (uint)__VSRDTATTRIB.RDTA_DocDataReloaded));
            }
            finally
            {
                // --- Unlock the document.
                // Note that we have to unlock the document even if the previous call failed.
                if(runningDocTable != null)
                    ErrorHandler.ThrowOnFailure(runningDocTable.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock,
                        docCookie));
            }
        }

        #endregion Other methods

        #region Method overrides
        //=====================================================================

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
    }
}