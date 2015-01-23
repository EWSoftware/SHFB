//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : HtmlEncodingCommandTarget.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/23/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a command target class that is used to handle the HTML encoding command
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.6.0  11/23/2012  EFW  Created the code
//===============================================================================================================

using System;
using System.Net;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace SandcastleBuilder.Package.Editors
{
    /// <summary>
    /// This command target class handles the HTML encoding command
    /// </summary>
    internal class HtmlEncodingCommandTarget : IOleCommandTarget
    {
        #region Private data members
        //=====================================================================

        private IWpfTextView textView;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This returns the next command target in the chain
        /// </summary>
        internal IOleCommandTarget NextTarget { get; set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="textView">The text view on which the command should operate</param>
        public HtmlEncodingCommandTarget(IWpfTextView textView)
        {
            this.textView = textView;
        }
        #endregion

        #region IOleCommandTarget implementation
        //=====================================================================

        /// <summary>
        /// Execute the command if applicable
        /// </summary>
        /// <param name="pguidCmdGroup">The unique identifier of the command group</param>
        /// <param name="nCmdID">The command ID to be executed</param>
        /// <param name="nCmdexecopt">Specifies how the object should execute the command</param>
        /// <param name="pvaIn">A structure with optional input arguments</param>
        /// <param name="pvaOut">A structure used to receive output arguments if any</param>
        /// <returns>Returns <c>S_OK</c> on success or a failure code if unsuccessful.</returns>
        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if(pguidCmdGroup == GuidList.guidSandcastleBuilderPackageCmdSet &&
              nCmdID == PkgCmdIDList.HtmlEncode)
            {
                this.HtmlEncodeSelection();
                return VSConstants.S_OK;
            }

            return this.NextTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        /// <summary>
        /// Query the status of the command
        /// </summary>
        /// <param name="pguidCmdGroup">The unique identifier of the command group</param>
        /// <param name="cCmds">The number of commands in the command array</param>
        /// <param name="prgCmds">An array of command structures being queried</param>
        /// <param name="pCmdText">A pointer to a structure used to return command text or status information</param>
        /// <returns>Returns <c>S_OK</c> on success or a failure code if unsuccessful.</returns>
        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if(pguidCmdGroup == GuidList.guidSandcastleBuilderPackageCmdSet &&
              prgCmds[0].cmdID == PkgCmdIDList.HtmlEncode)
            {
                if(!textView.Selection.IsEmpty)
                    prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                else
                    prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

                return VSConstants.S_OK;
            }

            return this.NextTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is called to HTML encode the current text selection
        /// </summary>
        private void HtmlEncodeSelection()
        {
            int start, end, length;
            string text;

            // Only encode a selection.  Ignore the request if there isn't one.
            if(!textView.Selection.IsEmpty)
            {
                ITextSnapshot snapshot = textView.TextSnapshot;

                if(snapshot == snapshot.TextBuffer.CurrentSnapshot)
                {
                    start = textView.Selection.Start.Position.Position;
                    end = textView.Selection.End.Position.Position;
                    length = end - start;

                    if(length > 0)
                        using(var edit = snapshot.TextBuffer.CreateEdit())
                        {
                            text = snapshot.GetText(start, length);

                            // HTML encode everything but single and double quotes as they're fine as-is
                            text = WebUtility.HtmlEncode(text).Replace("&quot;", "\"").Replace("&#39;", "'");

                            if(edit.Replace(start, length, text))
                                edit.Apply();
                        }
                }
            }
        }
        #endregion
    }
}
