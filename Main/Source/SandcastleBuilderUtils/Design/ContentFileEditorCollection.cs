//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : ContentFileEditorCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/27/2009
// Note    : Copyright 2007-2009, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a collection class used to hold the content file editor
// definitions.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.0.2  07/02/2007  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This collection class is used to hold the additional content file
    /// editor definitions.
    /// </summary>
    [Serializable]
    public class ContentFileEditorCollection : BindingList<ContentFileEditor>
    {
        #region Private data members
        //=====================================================================
        // Private data members

        // This is used to store the application-wide editors collection
        private static ContentFileEditorCollection globalEditors;

        private Exception lastError;
        #endregion

        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// This returns the reference to the global content file editor
        /// collection.
        /// </summary>
        public static ContentFileEditorCollection GlobalEditors
        {
            get
            {
                if(globalEditors == null)
                    globalEditors = new ContentFileEditorCollection();

                return globalEditors;
            }
        }

        /// <summary>
        /// If <see cref="LaunchEditorFor"/> returns false, this can be used to
        /// retrieve the exception describing why it failed.
        /// </summary>
        public Exception LastError
        {
            get { return lastError; }
        }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <overloads>There are two overloads for the constructor</overloads>
        public ContentFileEditorCollection()
        {
        }

        /// <summary>
        /// Clone the items in another collection to create this one.
        /// </summary>
        /// <param name="editors">The collection to clone</param>
        public ContentFileEditorCollection(ContentFileEditorCollection editors)
        {
            foreach(ContentFileEditor e in editors)
                base.Add((ContentFileEditor)e.Clone());
        }

        /// <summary>
        /// Add a range of items from another collection
        /// </summary>
        /// <param name="range">The range of items to add</param>
        /// <remarks>The entries are not cloned.</remarks>
        public void AddRange(ContentFileEditorCollection range)
        {
            if(range != null)
                foreach(ContentFileEditor editor in range)
                    base.Add(editor);
        }

        /// <summary>
        /// Sort the collection by editor description
        /// </summary>
        public void Sort()
        {
            ((List<ContentFileEditor>)base.Items).Sort(
                delegate(ContentFileEditor x, ContentFileEditor y)
                {
                    return String.Compare(x.EditorDescription,
                        y.EditorDescription,
                        StringComparison.CurrentCultureIgnoreCase);
                });
        }

        /// <summary>
        /// Search the list of content file editors and launch the appropriate
        /// one for the specified file.
        /// </summary>
        /// <param name="filename">The filename to edit</param>
        /// <param name="projectFile">The fully qualified name of the current
        /// project file.</param>
        /// <returns>True if successful, false if not.  <see cref="LastError"/>
        /// can be used to obtain details if it does fail.</returns>
        /// <remarks>If an editor is not defined, an attempt is made to launch
        /// the file using the OS shell with the Edit verb.</remarks>
        public bool LaunchEditorFor(string filename, string projectFile)
        {
            Process process = null;
            ContentFileEditor match = null;
            string extension = Path.GetExtension(filename);

            lastError = null;

            foreach(ContentFileEditor editor in this)
                if(editor.IsEditorFor(extension))
                {
                    match = editor;
                    break;
                }

            try
            {
                process = new Process();

                ProcessStartInfo psi = process.StartInfo;

                // If a match was found, use that to start the editor.  If not,
                // try starting the file with the Edit verb.
                if(match != null)
                {
                    psi.FileName = match.ApplicationPath;
                    psi.Arguments = match.ActualArguments(filename, projectFile);
                    psi.WorkingDirectory =
                        (match.StartupFolder.Path.Length == 0) ?
                            Path.GetDirectoryName(projectFile) : match.StartupFolder;
                    psi.UseShellExecute = false;
                }
                else
                {
                    psi.FileName = filename;
                    psi.WorkingDirectory = Path.GetDirectoryName(projectFile);
                    psi.UseShellExecute = true;
                    psi.Verb = "edit";
                }

                process.Start();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                lastError = ex;
            }
            finally
            {
                if(process != null)
                    process.Dispose();
            }

            return (lastError == null);
        }
    }
}
