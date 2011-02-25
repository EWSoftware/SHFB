//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ConvertToMSBuildFormat.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/15/2011
// Note    : Copyright 2008-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a abstract base class used to convert a project in
// another format to the new MSBuild format project files.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  07/23/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml;

using Microsoft.Build.BuildEngine;

namespace SandcastleBuilder.Utils.Conversion
{
    /// <summary>
    /// This class is an abstract base class used to convert a project in
    /// another format to the new MSBuild format project files used by
    /// SHFB 1.8.0.0 and later.
    /// </summary>
    public abstract class ConvertToMSBuildFormat
    {
        #region Private data members
        //=====================================================================

        private string projectFolder, oldProject, oldFolder;
        private SandcastleProject project;
        private PropertyInfo[] propertyCache;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// Get the XML text reader used for the conversion
        /// </summary>
        protected internal abstract XmlTextReader Reader
        {
            get;
        }

        /// <summary>
        /// Get the new project folder
        /// </summary>
        protected internal string ProjectFolder
        {
            get { return projectFolder; }
        }

        /// <summary>
        /// Get the old project folder
        /// </summary>
        protected internal string OldFolder
        {
            get { return oldFolder; }
        }

        /// <summary>
        /// Get the old project filename
        /// </summary>
        protected internal string OldProjectFile
        {
            get { return oldProject; }
        }

        /// <summary>
        /// Get the new project
        /// </summary>
        protected internal SandcastleProject Project
        {
            get { return project; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="oldProjectFile">The old project filename</param>
        /// <param name="folder">The folder in which to place the new project
        /// and its related files.  This cannot be the same folder as the
        /// old project file.</param>
        protected ConvertToMSBuildFormat(string oldProjectFile, string folder)
        {
            string projectFilename;

            oldProject = oldProjectFile;
            oldFolder = Path.GetDirectoryName(Path.GetFullPath(
                oldProjectFile));

            projectFolder = FolderPath.TerminatePath(folder);
            oldFolder = FolderPath.TerminatePath(oldFolder);

            if(folder == oldFolder)
                throw new ArgumentException("The new project folder cannot " +
                    "be the same as the old project file's folder", "folder");

            if(!Directory.Exists(projectFolder))
                Directory.CreateDirectory(projectFolder);

            projectFilename = Path.Combine(projectFolder,
                Path.GetFileNameWithoutExtension(oldProjectFile) + ".shfbproj");

            if(File.Exists(projectFilename))
                File.Delete(projectFilename);

            project = new SandcastleProject(projectFilename, false);
        }
        #endregion

        #region Conversion methods
        //=====================================================================

        /// <summary>
        /// This is used to perform the actual conversion
        /// </summary>
        /// <returns>The new project filename on success.  An exception is
        /// thrown if the conversion fails.</returns>
        public abstract string ConvertProject();
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This can be called after all additional content has been added
        /// to create the <b>Folder</b> build items in the project.
        /// </summary>
        protected void CreateFolderItems()
        {
            List<string> buildActions = new List<string>(Enum.GetNames(typeof(BuildAction)));
            List<string> folderNames = new List<string>();
            string name;
            
            buildActions.Remove("Folder");

            foreach(BuildItem item in project.MSBuildProject.EvaluatedItems)
                if(buildActions.IndexOf(item.Name) != -1)
                {
                    name = Path.GetDirectoryName(item.Include);

                    if(name.Length > 0 && folderNames.IndexOf(name) == -1)
                        folderNames.Add(name);
                }

            foreach(string folder in folderNames)
                project.AddFolderToProject(folder);
        }

        /// <summary>
        /// This is used to set the named property to the specified value
        /// using Reflection.
        /// </summary>
        /// <param name="name">The name of the property to set</param>
        /// <param name="value">The value to which it is set</param>
        /// <remarks>Property name matching is case insensitive as are the
        /// values themselves.  This is used to allow setting of simple project
        /// properties (non-collection) using command line parameters in the
        /// console mode builder.</remarks>
        /// <exception cref="ArgumentNullException">This is thrown if the
        /// name parameter is null or an empty string.</exception>
        /// <exception cref="BuilderException">This is thrown if an error
        /// occurs while trying to set the named property.</exception>
        /// <returns>The parsed object value to which the property was set.</returns>
        protected object SetProperty(string name, string value)
        {
            TypeConverter tc;
            object parsedValue;

            if(String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            // Get all public instance properties for the project class
            if(propertyCache == null)
                propertyCache = project.GetType().GetProperties(
                    BindingFlags.Public | BindingFlags.Instance);

            foreach(PropertyInfo property in propertyCache)
                if(String.Compare(name, property.Name,
                  StringComparison.OrdinalIgnoreCase) == 0 && property.CanWrite)
                {
                    try
                    {
                        if(property.PropertyType.IsEnum)
                            parsedValue = Enum.Parse(property.PropertyType, value, true);
                        else
                            if(property.PropertyType == typeof(Version))
                                parsedValue = new Version(value);
                            else
                                if(property.PropertyType == typeof(FilePath))
                                    parsedValue = new FilePath(value, project);
                                else
                                    if(property.PropertyType == typeof(FolderPath))
                                        parsedValue = new FolderPath(value, project);
                                    else
                                    {
                                        tc = TypeDescriptor.GetConverter(property.PropertyType);
                                        parsedValue = tc.ConvertFromString(value);
                                    }
                    }
                    catch(Exception ex)
                    {
                        throw new BuilderException("CVT0001", "Unable to parse value '" + value +
                            "' for property '" + name + "'", ex);
                    }

                    property.SetValue(project, parsedValue, null);
                    return parsedValue;
                }

            throw new BuilderException("CVT0002", "An attempt was made to " +
                "set an unknown or read-only property: " + name + "   Value: " + value);
        }

        /// <summary>
        /// This converts a relative path to a full path using the old
        /// project's folder.
        /// </summary>
        /// <param name="path">The path to convert</param>
        /// <returns>The fully qualified path</returns>
        protected internal string FullPath(string path)
        {
            if(!Path.IsPathRooted(path))
                return FilePath.GetFullPath(Path.Combine(oldFolder, path));

            return path;
        }

        /// <summary>
        /// This is used to expand a wildcard into all matching files
        /// </summary>
        /// <param name="wildcard">The wildcard to expand.</param>
        /// <param name="includeSubFolders">True to include subfolders, false
        /// to only include the given folder.</param>
        /// <returns>An enumerable list of matching files.</returns>
        protected static IEnumerable<string> ExpandWildcard(string wildcard, bool includeSubFolders)
        {
            SearchOption searchOpt = SearchOption.TopDirectoryOnly;
            string dirName;

            dirName = Path.GetDirectoryName(wildcard);

            if(Directory.Exists(dirName))
            {
                if(wildcard.IndexOfAny(new char[] { '*', '?' }) != -1 && includeSubFolders)
                    searchOpt = SearchOption.AllDirectories;

                foreach(string file in Directory.GetFiles(dirName, Path.GetFileName(wildcard), searchOpt))
                    yield return file;
            }
        }
        #endregion
    }
}
