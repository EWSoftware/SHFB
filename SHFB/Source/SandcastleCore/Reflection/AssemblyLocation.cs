//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : AssemblyLocation.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/06/2021
// Note    : Copyright 2012-2021, Eric Woodruff, All rights reserved
//
// This file contains a class that is used to contain information about a location and the assemblies for a
// specific location.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/09/2012  EFW  Created the code
// 01/02/2014  EFW  Moved the frameworks code to Sandcastle.Core
// 06/21/2015  EFW  Moved to the Reflection namespace and reworked for use with the Reflection Data Manager
// 09/20/2015  EFW  Added support for .NETCore 5.0 assemblies which are in a different folder format
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using IOPath = System.IO.Path;

namespace Sandcastle.Core.Reflection
{
    /// <summary>
    /// This class defines the settings for a specific assembly location
    /// </summary>
    public sealed class AssemblyLocation : INotifyPropertyChanged
    {
        #region Private data members
        //=====================================================================

        private string storedPath;

        private readonly BindingList<AssemblyDetails> assemblyDetails;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the path to the assembly location
        /// </summary>
        /// <value>The path may contain environment variable references</value>
        /// <remarks>When the stored path is changed, the set of assemblies in it is updated as well</remarks>
        public string StoredPath
        {
            get => storedPath;
            set
            {
                if(storedPath != value)
                {
                    storedPath = value;

                    this.DetermineAssemblyDetails(true);
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(IsCoreLocation));
                }
            }
        }

        /// <summary>
        /// This read-only property returns the actual path to the assembly location
        /// </summary>
        /// <remarks>This returns the path with any environment variable references replaced with the actual
        /// path elements.</remarks>
        public string Path
        {
            get
            {
                string path = Environment.ExpandEnvironmentVariables(storedPath ?? Directory.GetCurrentDirectory());

                // If x86 but it didn't exist, assume it's a 32-bit system and change the name
                if(path.IndexOf("%ProgramFiles(x86)%", StringComparison.Ordinal) != -1)
                    path = Environment.ExpandEnvironmentVariables(path.Replace("(x86)", String.Empty));

                return path;
            }
        }

        /// <summary>
        /// This read-only property is used to determine if this entry represents a core framework location
        /// </summary>
        /// <value>True if it is the core location, false if not</value>
        /// <remarks>The core location is determined by searching for <c>mscorlib</c> in the assembly set</remarks>
        public bool IsCoreLocation => assemblyDetails.Any(a =>
            a.Name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase) ||
            a.Name.Equals("System.Runtime", StringComparison.OrdinalIgnoreCase) ||
            a.Name.Equals("netstandard", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// This read-only property returns a bindable list of all assembly details for this location
        /// </summary>
        public IList<AssemblyDetails> AllAssemblies => assemblyDetails;

        /// <summary>
        /// This read-only property returns an enumerable list of only the included assembly details for this
        /// location.
        /// </summary>
        public IEnumerable<AssemblyDetails> IncludedAssemblies => assemblyDetails.Where(a => a.IsIncluded);

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Default constructor
        /// </summary>
        public AssemblyLocation()
        {
            assemblyDetails = new BindingList<AssemblyDetails>();
            assemblyDetails.ListChanged += (s, e) => this.OnPropertyChanged("Assemblies");
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="storedPath">The stored path to use</param>
        private AssemblyLocation(string storedPath) : this()
        {
            this.storedPath = storedPath;
        }
        #endregion

        #region INotifyPropertyChanged Members
        //=====================================================================

        /// <summary>
        /// The property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This raises the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="propertyName">The property name that changed</param>
        private void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Methods used to convert to/from XML
        //=====================================================================

        /// <summary>
        /// This is used to load the settings for an assembly location from an XML element
        /// </summary>
        /// <param name="location">The XML element containing the settings</param>
        /// <returns>The new assembly location item</returns>
        /// <remarks>If the location element is empty, the assembly details will be created by scanning the
        /// location for assemblies.</remarks>
        internal static AssemblyLocation FromXml(XElement location)
        {
            AssemblyLocation al = new AssemblyLocation(location.Attribute("Path").Value);

            foreach(var a in location.Descendants("AssemblyDetails"))
                al.assemblyDetails.Add(AssemblyDetails.FromXml(al.Path, a));

            return al;
        }

        /// <summary>
        /// This is used to convert the assembly location to an XML element
        /// </summary>
        /// <returns>The assembly location as an XML element</returns>
        internal XElement ToXml()
        {
            XElement e = new XElement("Location", new XAttribute("Path", storedPath));

            e.Add(assemblyDetails.OrderBy(a => a.Name).Select(a => a.ToXml(this.Path)));

            return e;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This can be used to load an empty location with information about the assemblies it contains
        /// </summary>
        /// <param name="clearAndRefresh">True to clear and refresh all file information or false to only remove
        /// assemblies that no longer exist and add new assemblies.</param>
        public void DetermineAssemblyDetails(bool clearAndRefresh)
        {
            string version, assemblyName;

            if(clearAndRefresh)
                assemblyDetails.Clear();

            if(!String.IsNullOrWhiteSpace(this.Path) && Directory.Exists(this.Path))
            {
                HashSet<string> assemblyDescs = new HashSet<string>(assemblyDetails.Select(a => a.Description));

                // Remove entries that are not there anymore
                foreach(var d in assemblyDetails.ToList())
                    if(!File.Exists(d.Filename))
                        assemblyDetails.Remove(d);

                // Add missing entries
                foreach(string assembly in Directory.EnumerateFiles(this.Path, "*.dll").Concat(
                  Directory.EnumerateFiles(this.Path, "*.winmd")))
                {
                    try
                    {
                        var details = AssemblyDetails.FromAssemblyName(AssemblyName.GetAssemblyName(assembly));

                        if(!assemblyDescs.Contains(details.Description))
                            assemblyDetails.Add(details);
                    }
                    catch(BadImageFormatException ex)
                    {
                        // Ignore, not a .NET assembly
                        System.Diagnostics.Debug.WriteLine(ex.FileName);
                    }
                }

                // The SDK folders for .NETCore 5.0 are typically in one of two formats so this gets a bit ugly.
                foreach(string sdkFolder in Directory.EnumerateDirectories(this.Path))
                {
                    // RootFolder\AssemblyName\Version\ref\dotnet\
                    version = Directory.EnumerateDirectories(sdkFolder).Where(
                        d => Char.IsDigit(d[d.LastIndexOf('\\') + 1])).OrderBy(d => d).LastOrDefault();

                    if(version != null)
                    {
                        assemblyName = IOPath.Combine(version, @"ref\dotnet",
                            sdkFolder.Substring(sdkFolder.LastIndexOf('\\') + 1));

                        if(File.Exists(assemblyName + ".dll"))
                            assemblyName += ".dll";
                        else
                            if(File.Exists(assemblyName + ".winmd"))
                                assemblyName += ".winmd";
                            else
                                assemblyName = null;

                        if(assemblyName != null)
                        {

                            var details = AssemblyDetails.FromAssemblyName(
                                AssemblyName.GetAssemblyName(assemblyName));

                            if(!assemblyDescs.Contains(details.Description))
                                assemblyDetails.Add(details);

                            continue;
                        }
                    }

                    // RootFolder\AssemblyName\Version\
                    version = Directory.EnumerateDirectories(sdkFolder).Where(
                        d => Char.IsDigit(d[d.LastIndexOf('\\') + 1])).OrderBy(d => d).LastOrDefault();

                    if(version != null)
                    {
                        assemblyName = IOPath.Combine(version, sdkFolder.Substring(sdkFolder.LastIndexOf('\\') + 1));

                        if(File.Exists(assemblyName + ".dll"))
                            assemblyName += ".dll";
                        else
                            if(File.Exists(assemblyName + ".winmd"))
                                assemblyName += ".winmd";
                            else
                                assemblyName = null;

                        if(assemblyName != null)
                        {
                            var details = AssemblyDetails.FromAssemblyName(
                                AssemblyName.GetAssemblyName(assemblyName));

                            if(!assemblyDescs.Contains(details.Description))
                                assemblyDetails.Add(details);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
