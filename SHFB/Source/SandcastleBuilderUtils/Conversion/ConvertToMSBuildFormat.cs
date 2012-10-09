//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ConvertToMSBuildFormat.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/26/2012
// Note    : Copyright 2008-2012, Eric Woodruff, All rights reserved
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
// 1.9.1.0  07/09/2010  EFW  Updated for use with .NET 4.0 and MSBuild 4.0.
// 1.9.3.4  01/08/2012  EFW  Added constructor to support use from VSPackage
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml;

using Microsoft.Build.Evaluation;

namespace SandcastleBuilder.Utils.Conversion
{
    /// <summary>
    /// This class is an abstract base class used to convert a project in
    /// another format to the new MSBuild format project files used by
    /// SHFB 1.8.0.0 and later.
    /// </summary>
    public abstract class ConvertToMSBuildFormat : IDisposable
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

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="oldProjectFile">The old project filename</param>
        /// <param name="folder">The folder in which to place the new project and its related files.  This
        /// cannot be the same folder as the old project file.</param>
        /// <overloads>There are two overloads for the constructor</overloads>
        protected ConvertToMSBuildFormat(string oldProjectFile, string folder)
        {
            string projectFilename;

            oldProject = oldProjectFile;
            oldFolder = FolderPath.TerminatePath(Path.GetDirectoryName(Path.GetFullPath(oldProjectFile)));
            projectFolder = FolderPath.TerminatePath(folder);

            if(folder == oldFolder)
                throw new ArgumentException("The new project folder cannot be the same as the old project " +
                    "file's folder", "folder");

            if(!Directory.Exists(projectFolder))
                Directory.CreateDirectory(projectFolder);

            projectFilename = Path.Combine(projectFolder,
                Path.GetFileNameWithoutExtension(oldProjectFile) + ".shfbproj");

            if(File.Exists(projectFilename))
                File.Delete(projectFilename);

            project = new SandcastleProject(projectFilename, false);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="oldProjectFile">The old project filename</param>
        /// <param name="newProject">The new project into which the converted elements are inserted</param>
        protected ConvertToMSBuildFormat(string oldProjectFile, SandcastleProject newProject)
        {
            oldProject = oldProjectFile;
            oldFolder = FolderPath.TerminatePath(Path.GetDirectoryName(Path.GetFullPath(oldProjectFile)));
            projectFolder = FolderPath.TerminatePath(Path.GetDirectoryName(newProject.Filename));

            if(projectFolder == oldFolder)
                throw new ArgumentException("The new project folder cannot be the same as the old project " +
                    "file's folder", "newProject");

            project = new SandcastleProject(newProject.MSBuildProject);
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the
        /// Sandcastle project if not done explicity with <see cref="Dispose()"/>.
        /// </summary>
        ~ConvertToMSBuildFormat()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of
        /// the Sandcastle project object.
        /// </summary>
        /// <overloads>There are two overloads for this method.</overloads>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This can be overridden by derived classes to add their own
        /// disposal code if necessary.
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed
        /// and unmanaged resources or false to just dispose of the
        /// unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if(project != null)
                project.Dispose();
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

            foreach(ProjectItem item in project.MSBuildProject.AllEvaluatedItems)
                if(buildActions.IndexOf(item.ItemType) != -1)
                {
                    if(!Path.IsPathRooted(item.EvaluatedInclude))
                        name = Path.GetDirectoryName(item.EvaluatedInclude);
                    else
                    {
                        // Convert fully qualified paths to relative paths
                        name = FolderPath.TerminatePath(Path.GetDirectoryName(item.EvaluatedInclude));

                        // Ignore fully qualified paths outside of the project folder
                        if(!name.StartsWith(projectFolder, StringComparison.OrdinalIgnoreCase))
                            continue;

                        name = name.Substring(projectFolder.Length);
                    }

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
                        // Ignore exceptions for the Language property.  A few people have had an environment
                        // variable with that name that gets picked up as a default and the value isn't typically
                        // valid for a culture name.
                        if(!name.Equals("Language", StringComparison.OrdinalIgnoreCase))
                            throw new BuilderException("CVT0001", "Unable to parse value '" + value +
                                "' for property '" + name + "'", ex);

                        parsedValue = null;
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

                foreach(string file in Directory.EnumerateFiles(dirName, Path.GetFileName(wildcard), searchOpt))
                    yield return file;
            }
        }
        #endregion
    }
}
