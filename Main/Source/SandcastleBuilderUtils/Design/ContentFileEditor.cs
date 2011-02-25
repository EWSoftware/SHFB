//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : ContentFileEditor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/17/2009
// Note    : Copyright 2007-2009, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing an external application used to edit
// an additional content file.
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
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This class represents an external application that is used to edit
    /// an additional content file.
    /// </summary>
    [Serializable, DefaultProperty("Description")]
    public class ContentFileEditor : ICloneable
    {
        #region Private data members
        //=====================================================================
        // Private data members

        // These are used to parse the extension list and replace the macro
        // placeholders
        private static Regex reSplit = new Regex("[.,; ]");

        // There are only three so we won't bother with a match evaluator
        private static Regex reContentItem = new Regex("\\$ContentItem",
            RegexOptions.IgnoreCase);
        private static Regex reProjectFile = new Regex("\\$ProjectFile",
            RegexOptions.IgnoreCase);
        private static Regex reProjectFolder = new Regex("\\$ProjectFolder",
            RegexOptions.IgnoreCase);

        // Member fields
        private string description, arguments, extensions;
        private FilePath applicationPath;
        private FolderPath startupFolder;
        #endregion

        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// This is used to get or set a description of the editor application
        /// </summary>
        [Category("Definition"), RefreshProperties(RefreshProperties.All),
         Description("A description of the editor application")]
        public string Description
        {
            get { return description; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    description = "Editor";
                else
                    description = value.Trim();
            }
        }

        /// <summary>
        /// This is used to get or set the filename extensions for which this
        /// content file editor will be used.
        /// </summary>
        /// <value>Separate the extensions with space, semi-colon, or a comma</value>
        [Category("Definition"), RefreshProperties(RefreshProperties.All),
         Description("The filename extensions for which this content file " +
            "editor will be used.  Separate extensions with a space, " +
            "semi-colon, period, or a comma.")]
        public string Extensions
        {
            get { return extensions; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    extensions = "txt";
                else
                    extensions = value.Trim();
            }
        }

        /// <summary>
        /// This is used to get or set the parameters to pass to the
        /// application.
        /// </summary>
        /// <remarks>The following macros can be used to subsitute item
        /// and path values into the argument string.
        /// <list type="table">
        ///    <listheader>
        ///       <term>Macro Name</term>
        ///       <description>Value Used</description>
        ///    </listheader>
        ///    <item>
        ///       <term>$ContentItem</term>
        ///       <description>The full path to the content item to be
        /// edited</description>
        ///    </item>
        ///    <item>
        ///       <term>$ProjectFile</term>
        ///       <description>The fully qualified path and filename of the
        /// current project.</description>
        ///    </item>
        ///    <item>
        ///       <term>$ProjectFolder</term>
        ///       <description>The fully qualified path to the current
        /// project.</description>
        ///    </item>
        /// </list>
        /// <p/>The macro names are case-insensitive.  Enclose arguments in
        /// double quotes if they may contain spaces.</remarks>
        [Category("Editor"),
          Description("The parameters to pass to the application.  " +
            "$ContentItem = The content item to edit, $ProjectFile = Full " +
            "project path and filename, $ProjectFolder = Full project " +
            "folder name.  Enclose in quotes if necessary.")]
        public string Arguments
        {
            get { return arguments; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    arguments = "\"$ContentItem\"";
                else
                    arguments = value.Trim();
            }
        }

        /// <summary>
        /// This is used to get or set the source path to the editor
        /// application.
        /// </summary>
        [Category("Editor"), Description("The path to the additional " +
          "content editor application."),
          Editor(typeof(FilePathObjectEditor), typeof(UITypeEditor)),
          FileDialog("Select the additional content editor application",
            "Application (*.exe)|*.exe|All Files (*.*)|*.*",
              FileDialogType.FileOpen), XmlIgnore]
        public FilePath ApplicationPath
        {
            get { return applicationPath; }
            set
            {
                if(value == null || value.Path.Length == 0)
                    applicationPath = new FilePath(
                        @"%SystemRoot%\System32\notepad.exe", null);
                else
                    applicationPath = value;
            }
        }

        /// <summary>
        /// This is used to set or get the startup folder used when launching
        /// the content file editor.
        /// </summary>
        /// <value>If not set, the current project's path is used.</value>
        [Category("Editor"), Description("The startup folder used when " +
          "launching the content file editor.  If blank, the current " +
          "project's path is used."),
          Editor(typeof(FolderPathObjectEditor), typeof(UITypeEditor)),
          FolderDialog("Select the startup path when launching the editor",
            Environment.SpecialFolder.MyDocuments), XmlIgnore]
        public FolderPath StartupFolder
        {
            get { return startupFolder; }
            set
            {
                if(value == null)
                    startupFolder = new FolderPath(null);
                else
                    startupFolder = value;
            }
        }

        /// <summary>
        /// This returns a description of the entry suitable for display in a
        /// bound list control.
        /// </summary>
        [Category("Info"), Description("Editor description")]
        public string EditorDescription
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture,
                    "{0} ({1})", description, extensions);
            }
        }

        /// <summary>
        /// This property is used to serialize the application path
        /// </summary>
        /// <remarks><see cref="FilePath" /> is not serializable because it
        /// does not have a parameterless constructor.  This stands in for the
        /// <see cref="ApplicationPath" /> property when this class is
        /// serialized.</remarks>
        [Browsable(false)]
        public string ApplicationPathSerializable
        {
            get { return this.ApplicationPath.PersistablePath; }
            set { this.ApplicationPath = new FilePath(value, null); }
        }

        /// <summary>
        /// This property is used to serialize the startup folder
        /// </summary>
        /// <remarks><see cref="FolderPath" /> is not serializable because it
        /// does not have a parameterless constructor.  This stands in for the
        /// <see cref="StartupFolder" /> property when this class is
        /// serialized.</remarks>
        [Browsable(false)]
        public string StartupFolderSerializable
        {
            get { return this.StartupFolder.PersistablePath; }
            set { this.StartupFolder = new FolderPath(value, null); }
        }
        #endregion

        #region ICloneable implementation
        //=====================================================================
        // ICloneable implementation

        /// <summary>
        /// Clone this object
        /// </summary>
        /// <returns>A clone of the object</returns>
        public object Clone()
        {
            ContentFileEditor clone = new ContentFileEditor();
            clone.Description = description;
            clone.Arguments = arguments;
            clone.Extensions = extensions;
            clone.ApplicationPath = new FilePath(
                applicationPath.PersistablePath, null);
            clone.StartupFolder = new FolderPath(
                startupFolder.PersistablePath, null);

            return clone;
        }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public ContentFileEditor()
        {
            this.Description = this.Extensions = this.Arguments = null;
            this.ApplicationPath = null;
            this.StartupFolder = null;
        }

        /// <summary>
        /// See if the given extension is a match for this file editor.
        /// </summary>
        /// <param name="extension">The extension to check</param>
        /// <returns>True if this is the editor for the given file extension
        /// or false if not.</returns>
        public bool IsEditorFor(string extension)
        {
            string[] matchList = reSplit.Split(extensions);

            if(!String.IsNullOrEmpty(extension))
            {
                if(extension[0] == '.')
                    extension = extension.Substring(1);

                foreach(string entry in matchList)
                    if(entry.Length != 0 && String.Compare(entry, extension,
                      true, CultureInfo.CurrentCulture) == 0)
                        return true;
            }

            return false;
        }

        /// <summary>
        /// Substitute actual values for the macro place holders
        /// </summary>
        /// <param name="contentItem">The content item</param>
        /// <param name="projectFile">The current project file</param>
        /// <returns>A string containing the actual arguments to pass to the
        /// editor application.</returns>
        public string ActualArguments(string contentItem, string projectFile)
        {
            string actualArgs;

            actualArgs = reContentItem.Replace(arguments, contentItem);
            actualArgs = reProjectFile.Replace(actualArgs, projectFile);
            actualArgs = reProjectFolder.Replace(actualArgs,
                Path.GetDirectoryName(projectFile));

            return actualArgs;
        }
    }
}
