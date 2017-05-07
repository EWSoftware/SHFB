//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ReflectionDataSet.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/06/2017
// Note    : Copyright 2012-2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to contain information used to obtain reflection data and comments for a
// specific set of assemblies.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/09/2012  EFW  Created the code
// 01/03/2013  EFW  Added method to get referenced namespaces
// 01/02/2014  EFW  Moved the frameworks code to Sandcastle.Core
// 06/24/2015  EFW  Changed the framework settings classes to reflection data to be more general in nature
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Sandcastle.Core.Reflection
{
    /// <summary>
    /// This class contains information used to obtain reflection data and comments for a specific set of
    /// assemblies.
    /// </summary>
    public sealed class ReflectionDataSet : INotifyPropertyChanged
    {
        #region String wrapper class
        //=====================================================================

        /// <summary>
        /// This is used to create a bindable, editable list of string values
        /// </summary>
        internal class StringWrapper
        {
            /// <summary>
            /// The string value
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// This is used to handle an implicit conversion from a <see cref="StringWrapper"/> object to a string
            /// </summary>
            /// <param name="wrapper">The <see cref="StringWrapper"/> to convert</param>
            /// <returns>The value of the given wrapper as a string</returns>
            public static implicit operator String(StringWrapper wrapper)
            {
                return (wrapper == null) ? null : wrapper.Value;
            }
        }
        #endregion

        #region Binding redirection class
        //=====================================================================

        /// <summary>
        /// This class is used to edit binding redirection settings
        /// </summary>
        public class BindingRedirection
        {
            #region Properties
            //=====================================================================

            /// <summary>
            /// The assembly name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The assembly culture (optional)
            /// </summary>
            public CultureInfo Culture { get; set; }

            /// <summary>
            /// The public key token
            /// </summary>
            public string PublicKeyToken { get; set; }

            /// <summary>
            /// The old version
            /// </summary>
            public Version OldVersion { get; set; }

            /// <summary>
            /// The new version
            /// </summary>
            public Version NewVersion { get; set; }

            #endregion

            #region Constructor
            //=====================================================================

            /// <summary>
            /// Constructor
            /// </summary>
            public BindingRedirection()
            {
                this.Name = "Identity";
                this.OldVersion = new Version(1, 0);
                this.NewVersion = new Version(2, 0);
            }
            #endregion

            #region Methods
            //=====================================================================

            /// <summary>
            /// Create an instance from an XML element
            /// </summary>
            /// <param name="redirection">The binding redirection settings</param>
            /// <returns>The binding redirection settings created from the XML element</returns>
            public static BindingRedirection FromXml(XElement redirection)
            {
                var br = new BindingRedirection
                {
                    Name = redirection.Attribute("Name").Value,
                    PublicKeyToken = (string)redirection.Attribute("PublicKeyToken"),
                    OldVersion = new Version(redirection.Attribute("OldVersion").Value),
                    NewVersion = new Version(redirection.Attribute("NewVersion").Value),
                };

                if(redirection.Attribute("Culture") != null)
                    try
                    {
                        br.Culture = new CultureInfo((string)redirection.Attribute("Culture"));
                    }
                    catch
                    {
                        // If not valid, ignore the culture
                    }

                return br;
            }

            /// <summary>
            /// This is used to convert the binding redirection to an XML element
            /// </summary>
            /// <returns>The binding redirection as an XML element</returns>
            public XElement ToXml()
            {
                return new XElement("BindingRedirection",
                    new XAttribute("Name", this.Name),
                    (this.Culture == null) ? null : new XAttribute("Culture", this.Culture.Name),
                    (this.PublicKeyToken == null) ? null : new XAttribute("PublicKeyToken", this.PublicKeyToken),
                    new XAttribute("OldVersion", this.OldVersion.ToString()),
                    new XAttribute("NewVersion", this.NewVersion.ToString())
                );
            }

            /// <summary>
            /// This is used to convert the binding redirection entry to an MRefBuilder configuration file
            /// assembly binding redirection element.
            /// </summary>
            /// <returns>The binding redirection as an MRefBuilder configuration element</returns>
            public XElement ToBindingRedirectionElement()
            {
                return new XElement("dependentAssembly",
                    new XElement("assemblyIdentity",
                        new XAttribute("name", this.Name),
                        String.IsNullOrWhiteSpace(this.PublicKeyToken) ? null :
                            new XAttribute("publicKeyToken", this.PublicKeyToken),
                        (this.Culture == null) ? null : new XAttribute("culture", this.Culture.Name)),
                    new XElement("bindingRedirect",
                        new XAttribute("oldVersion", this.OldVersion.ToString()),
                        new XAttribute("newVersion", this.NewVersion.ToString())));
            }
            #endregion
        }
        #endregion

        #region Private data members
        //=====================================================================

        private string platform, title, notes;
        private bool allSystemTypesRedirected;
        private Version version;

        private BindingList<AssemblyLocation> assemblyLocations;
        private BindingList<StringWrapper> ignoredNamespaces, ignoredUnresolved;
        private BindingList<BindingRedirection> bindingRedirections;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the filename
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// This is used to get or set the platform associated with the reflection data set
        /// </summary>
        public string Platform
        {
            get { return platform; }
            set
            {
                if(platform != value)
                {
                    platform = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set the version number associated with the reflection data set if applicable
        /// </summary>
        public Version Version
        {
            get { return version; }
            set
            {
                if(version != value)
                {
                    version = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set the reflection data set title
        /// </summary>
        public string Title
        {
            get { return title; }
            set
            {
                if(title != value)
                {
                    title = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set notes about this reflection data set
        /// </summary>
        public string Notes
        {
            get { return notes; }
            set
            {
                if(notes != value)
                {
                    notes = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not all <c>System</c> types are redirected to other assemblies
        /// </summary>
        public bool AllSystemTypesRedirected
        {
            get { return allSystemTypesRedirected; }
            set
            {
                if(allSystemTypesRedirected != value)
                {
                    allSystemTypesRedirected = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This read-only property is used to determine if this entry represents a core framework
        /// </summary>
        /// <value>True if it is the core framework, false if not</value>
        /// <remarks>The core location is determined by searching for <c>mscorlib</c> in the assembly set</remarks>
        public bool IsCoreFramework
        {
            get
            {
                return assemblyLocations.Any(a => a.IsCoreLocation);
            }
        }

        /// <summary>
        /// This read-only property is used to get the core framework location if there is one
        /// </summary>
        /// <value>The core framework location or null if there isn't one</value>
        /// <remarks>The core location is determined by searching for <c>mscorlib</c> in the assembly set</remarks>
        public AssemblyLocation CoreFrameworkLocation
        {
            get
            {
                return assemblyLocations.FirstOrDefault(a => a.IsCoreLocation);
            }
        }

        /// <summary>
        /// This read-only property returns a bindable list of assembly locations
        /// </summary>
        public IBindingList AssemblyLocations
        {
            get { return assemblyLocations; }
        }

        /// <summary>
        /// This read-only property returns a bindable list of ignored namespaces used for building the
        /// reflection data.
        /// </summary>
        public IBindingList IgnoredNamespaces
        {
            get { return ignoredNamespaces; }
        }

        /// <summary>
        /// This read-only property returns a bindable list of ignored unresolved assembly identities used for
        /// building the reflection data.
        /// </summary>
        public IBindingList IgnoredUnresolved
        {
            get { return ignoredUnresolved; }
        }

        /// <summary>
        /// This read-only property returns a bindable list of binding redirections used for building the
        /// reflection data.
        /// </summary>
        public IBindingList BindingRedirections
        {
            get { return bindingRedirections; }
        }

        /// <summary>
        /// This read-only property can be used to determine if the reflection data set's core assemblies are
        /// present on the current system
        /// </summary>
        /// <returns>True if the core assembly folder exists and contains the first assembly, false if not.  If
        /// the first assembly is present, it is assumed that all of them are.</returns>
        public bool IsPresent
        {
            get
            {
                AssemblyLocation al = assemblyLocations.FirstOrDefault(l => l.IsCoreLocation);

                return (al != null && al.IncludedAssemblies.Any() && File.Exists(al.IncludedAssemblies.First().Filename));
            }
        }

        /// <summary>
        /// This read-only property returns an enumerable list of all included assemblies in the reflection data
        /// set.
        /// </summary>
        public IEnumerable<AssemblyDetails> IncludedAssemblies
        {
            get
            {
                foreach(var al in assemblyLocations)
                    foreach(var ad in al.IncludedAssemblies)
                        yield return ad;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ReflectionDataSet()
        {
            platform = PlatformType.DotNetFramework;
            title = "Custom reflection data";
            version = new Version();

            assemblyLocations = new BindingList<AssemblyLocation>();
            assemblyLocations.ListChanged += (s, e) =>
            {
                this.OnPropertyChanged("AssemblyLocations");
                this.OnPropertyChanged("IsCoreFramework");
            };

            ignoredNamespaces = new BindingList<StringWrapper>();
            ignoredNamespaces.ListChanged += (s, e) => this.OnPropertyChanged("IgnoredNamespaces");

            ignoredUnresolved = new BindingList<StringWrapper>();
            ignoredUnresolved.ListChanged += (s, e) => this.OnPropertyChanged("IgnoredUnresolved");

            bindingRedirections = new BindingList<BindingRedirection>();
            bindingRedirections.ListChanged += (s, e) => this.OnPropertyChanged("BindingRedirections");
        }

        /// <summary>
        /// File constructor
        /// </summary>
        /// <param name="filename">The filename from which to load the reflection data set information</param>
        public ReflectionDataSet(string filename)
            : this()
        {
            this.Filename = filename;

            XDocument doc = XDocument.Load(filename);
            XElement dataSet = doc.Root;

            platform = dataSet.Attribute("Platform").Value;

            if(dataSet.Attribute("Version") == null || !Version.TryParse(dataSet.Attribute("Version").Value, out version))
                version = new Version();

            title = dataSet.Attribute("Title").Value;
            allSystemTypesRedirected = ((bool?)dataSet.Attribute("AllSystemTypesRedirected") ?? false);
            notes = (string)dataSet.Element("Notes");

            foreach(var location in dataSet.Descendants("Location"))
                assemblyLocations.Add(AssemblyLocation.FromXml(location));

            foreach(var ignored in dataSet.Descendants("Namespace"))
                ignoredNamespaces.Add(new StringWrapper { Value = ignored.Value });

            foreach(var ignored in dataSet.Descendants("Unresolved"))
                ignoredUnresolved.Add(new StringWrapper { Value = ignored.Value });

            foreach(var br in dataSet.Descendants("BindingRedirection"))
                bindingRedirections.Add(BindingRedirection.FromXml(br));
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
            var handler = PropertyChanged;

            if(handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Save to an XML file
        //=====================================================================

        /// <summary>
        /// This is used to save the reflection data set information to an XML file
        /// </summary>
        public void Save()
        {
            if(String.IsNullOrWhiteSpace(this.Filename))
                throw new InvalidOperationException("A filename has not been specified");

            XDocument dataSet = new XDocument(
                new XElement("ReflectionDataSet",
                    new XAttribute("Platform", platform),
                    (version == null || (version.Major == 0 && version.Minor < 1 && version.Build < 1 &&
                        version.Revision < 1)) ? null : new XAttribute("Version", version),
                    new XAttribute("Title", (title ?? "Unknown")),
                    !allSystemTypesRedirected ? null : new XAttribute("AllSystemTypesRedirected", true),
                    String.IsNullOrWhiteSpace(notes) ? null : new XElement("Notes", notes)
            ));

            XElement locations = new XElement("AssemblyLocations");
            locations.Add(assemblyLocations.Select(l => l.ToXml()));
            dataSet.Root.Add(locations);

            XElement ignoredNS = new XElement("IgnoredNamespaces");
            ignoredNS.Add(ignoredNamespaces.Select(ns => new XElement("Namespace", ns.Value)));
            dataSet.Root.Add(ignoredNS);

            XElement unresolved = new XElement("IgnoredUnresolved");
            unresolved.Add(ignoredUnresolved.Select(ign => new XElement("Unresolved", ign.Value)));
            dataSet.Root.Add(unresolved);

            XElement br = new XElement("BindingRedirections");
            br.Add(bindingRedirections.Select(b => b.ToXml()));
            dataSet.Root.Add(br);

            dataSet.Save(this.Filename);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to return the <c>ignoreIfUnresolved</c> configuration entries for the MRefBuilder
        /// configuration file.
        /// </summary>
        /// <returns>The configuration entries</returns>
        public string IgnoreIfUnresolvedConfiguration()
        {
            return String.Join("\r\n", ignoredUnresolved.Select(i => String.Format(
                "<assemblyIdentity name=\"{0}\" />", i.Value)));
        }

        /// <summary>
        /// This is used to return the <c>assemblyBinding</c> configuration entries for the MRefBuilder
        /// configuration file.
        /// </summary>
        /// <returns>The configuration entries</returns>
        public string BindingRedirectionConfiguration()
        {
            if(bindingRedirections.Count == 0)
                return String.Empty;

            return "<assemblyBinding>\r\n" + String.Join("\r\n",
                bindingRedirections.Select(b => b.ToBindingRedirectionElement())) + "</assemblyBinding>";
        }

        /// <summary>
        /// This is used to return the API filter <c>namespace</c> configuration entries for the MRefBuilder
        /// configuration file.
        /// </summary>
        /// <returns>The configuration entries</returns>
        public string IgnoredNamespacesConfiguration()
        {
            return String.Join("\r\n", ignoredNamespaces.Select(i => String.Format(
                "<namespace name=\"{0}\" expose=\"false\" />", WebUtility.HtmlEncode(i.Value))));
        }

        /// <summary>
        /// This is used to return an enumerable list of comments file locations
        /// </summary>
        /// <param name="language">An optional language to use when searching for XML comments files, or
        /// null to use the current language.</param>
        /// <returns>An enumerable list of file paths to search for XML comments files</returns>
        public IEnumerable<string> CommentsFileLocations(CultureInfo language)
        {
            string path;

            foreach(var l in assemblyLocations)
                if(platform != PlatformType.DotNetPortable || version.Major != 4 || version.Minor > 0)
                {
                    // If localized, the comments files will be in a sub-folder based on the language
                    path = null;

                    // Check for localized versions using the specified language
                    if(language != null)
                        path = CultureSpecificCommentsFileFolder(l, language);

                    // If not found, check for localized versions using the current language but only if a
                    // specific language was not specified.
                    if(path == null && language == null)
                        path = CultureSpecificCommentsFileFolder(l, CultureInfo.CurrentCulture);

                    // If not found, check for localized versions using the default English language
                    if(path == null && !CultureInfo.CurrentCulture.Name.StartsWith("en",
                      StringComparison.OrdinalIgnoreCase))
                        path = CultureSpecificCommentsFileFolder(l, CultureInfo.GetCultureInfo("en"));

                    // If no culture-specific folder was found, try the same location as the assemblies
                    if(path == null && l.IncludedAssemblies.Any(a => File.Exists(Path.ChangeExtension(a.Filename, ".xml"))))
                        path = l.Path;

                    // On some systems, the .NET 4.6.2 XML comments files can appear in a generic v4.X folder
                    // outside of the standard assembly folder.  If they're not in the usual place look there.
                    if(path == null && platform == PlatformType.DotNetFramework)
                    {
                        string externalPath = String.Format("{0}\\v{1}.X", Path.GetDirectoryName(l.Path),
                            this.Version.Major);

                        if(Directory.Exists(externalPath) && Directory.EnumerateFiles(externalPath, "*.xml").Any())
                        {
                            if(language != null && Directory.Exists(Path.Combine(externalPath, language.Name)) &&
                              Directory.EnumerateFiles(Path.Combine(externalPath, language.Name), "*.xml").Any())
                            {
                                path = Path.Combine(externalPath, language.Name);
                            }
                            else
                                if(language != null && Directory.Exists(Path.Combine(externalPath,
                                  language.TwoLetterISOLanguageName)) && Directory.EnumerateFiles(
                                  Path.Combine(externalPath, language.TwoLetterISOLanguageName), "*.xml").Any())
                                {
                                    path = Path.Combine(externalPath, language.TwoLetterISOLanguageName);
                                }
                                else
                                    path = externalPath;
                        }
                    }

                    if(path != null)
                        yield return Path.Combine(path, "*.xml");
                }
                else
                    if(Directory.Exists(l.Path))
                    {
                        // The .NET Portable Library Framework 4.0 duplicates most of its comments files across
                        // all of the profile folders.  To minimize the duplication, we'll find the folder with
                        // the most files and return it followed by an entry for each unique file in subsequent
                        // folders.
                        var commentGroups = Directory.EnumerateFiles(l.Path, "*.xml",
                            SearchOption.AllDirectories).Where(d => File.Exists(
                                Path.ChangeExtension(d, ".dll"))).GroupBy(d =>
                                    Path.GetDirectoryName(d)).OrderByDescending(d => d.Count());

                        // Odd case but it appears that the folder does exist in some cases with no comments
                        // files in it for the assemblies.
                        if(commentGroups.Count() != 0)
                        {
                            HashSet<string> commentsFiles = new HashSet<string>(commentGroups.First().Select(
                                f => Path.GetFileName(f)));

                            yield return Path.Combine(commentGroups.First().Key, "*.xml");

                            foreach(var g in commentGroups)
                                foreach(var f in g.Where(f => !commentsFiles.Contains(Path.GetFileName(f))))
                                    yield return Path.Combine(g.Key, Path.GetFileName(f));
                        }
                    }
        }

        /// <summary>
        /// This is used to see if any comments files exist in a culture-specific framework location folder
        /// </summary>
        /// <param name="location">The framework location</param>
        /// <param name="language">The language used to check for a culture-specific folder</param>
        /// <returns>True if files were</returns>
        private static string CultureSpecificCommentsFileFolder(AssemblyLocation location, CultureInfo language)
        {
            string path = location.Path;

            if(language == null)
                return null;

            if(Directory.Exists(Path.Combine(path, language.Name)))
                path = Path.Combine(path, language.Name);
            else
                if(Directory.Exists(Path.Combine(path, language.TwoLetterISOLanguageName)))
                    path = Path.Combine(path, language.TwoLetterISOLanguageName);
                else
                    path = null;

            if(path != null && !location.IncludedAssemblies.Any(a => File.Exists(Path.ChangeExtension(
              Path.Combine(path, Path.GetFileName(a.Filename)), ".xml"))))
                path = null;

            return path;
        }

        /// <summary>
        /// This is used to see if the framework contains an assembly that uses the specified name
        /// </summary>
        /// <param name="assemblyName">The assembly name without a path or extension or a strong name value.
        /// If a strong name value is specified, a "starts with" comparison on the description is used to see if
        /// the assembly is present in the framework.  This allows for matches on strong names with processor
        /// architecture specified which we don't have.  If only a name is given, just the name is compared.
        /// Comparisons are case-insensitive.</param>
        /// <returns>True if the framework contains the named assembly, false if not.</returns>
        public bool ContainsAssembly(string assemblyName)
        {
            if(assemblyName.IndexOf(',') != -1)
                return assemblyLocations.Any(al => al.IncludedAssemblies.Any(a => assemblyName.StartsWith(
                    a.Description, StringComparison.OrdinalIgnoreCase) && File.Exists(a.Filename)));

            return assemblyLocations.Any(al => al.IncludedAssemblies.Any(a => assemblyName.Equals(a.Name,
                StringComparison.OrdinalIgnoreCase) && File.Exists(a.Filename)));
        }

        /// <summary>
        /// This is used to find an assembly by name
        /// </summary>
        /// <param name="assemblyName">The assembly name without a path or extension or a strong name value.
        /// If a strong name value is specified, a "starts with" comparison on the description is used to see if
        /// the assembly is present in the framework.  This allows for matches on strong names with processor
        /// architecture specified which we don't have.  If only a name is given, just the name is compared.
        /// Comparisons are case-insensitive.</param>
        /// <returns>The assembly if found or null if not found</returns>
        public AssemblyDetails FindAssembly(string assemblyName)
        {
            AssemblyDetails ad = null;
            bool strongName = (assemblyName.IndexOf(',') != -1);

            foreach(var al in assemblyLocations)
            {
                if(strongName)
                {
                    ad = al.IncludedAssemblies.FirstOrDefault(a => assemblyName.StartsWith(a.Description,
                        StringComparison.OrdinalIgnoreCase) && File.Exists(a.Filename));
                }
                else
                    ad = al.IncludedAssemblies.FirstOrDefault(a => assemblyName.Equals(a.Name,
                        StringComparison.OrdinalIgnoreCase) && File.Exists(a.Filename));

                if(ad != null)
                    break;
            }

            return ad;
        }

        /// <summary>
        /// This is used to get an enumerable list of unique namespaces referenced in the XML comments files of
        /// the given set of namespaces.
        /// </summary>
        /// <param name="language">The language to use when locating the XML comments files</param>
        /// <param name="searchNamespaces">An enumerable list of namespaces to search</param>
        /// <param name="validNamespaces">An enumerable list of valid namespaces</param>
        /// <returns>An enumerable list of unique namespaces in the related XML comments files</returns>
        public IEnumerable<string> GetReferencedNamespaces(CultureInfo language,
          IEnumerable<string> searchNamespaces, IEnumerable<string> validNamespaces)
        {
            HashSet<string> seenNamespaces = new HashSet<string>();
            string ns;

            foreach(string path in this.CommentsFileLocations(language))
                foreach(string file in Directory.EnumerateFiles(Path.GetDirectoryName(path),
                  Path.GetFileName(path)).Where(f => searchNamespaces.Contains(Path.GetFileNameWithoutExtension(f))))
                {
                    // Find all comments elements with a reference.  XML comments files may be ill-formed so
                    // ignore any elements without a cref attribute.
                    var crefs = ComponentUtilities.XmlStreamAxis(file, new[] { "event", "exception",
                        "inheritdoc", "permission", "see", "seealso" }).Select(
                        el => (string)el.Attribute("cref")).Where(c => c != null);

                    foreach(string refId in crefs)
                        if(refId.Length > 2 && refId[1] == ':' && refId.IndexOfAny(new[] { '.', '(' }) != -1)
                        {
                            ns = refId.Trim();

                            // Strip off member name?
                            if(!ns.StartsWith("R:", StringComparison.OrdinalIgnoreCase) &&
                              !ns.StartsWith("G:", StringComparison.OrdinalIgnoreCase) &&
                              !ns.StartsWith("N:", StringComparison.OrdinalIgnoreCase) &&
                              !ns.StartsWith("T:", StringComparison.OrdinalIgnoreCase))
                            {
                                if(ns.IndexOf('(') != -1)
                                    ns = ns.Substring(0, ns.IndexOf('('));

                                if(ns.IndexOf('.') != -1)
                                    ns = ns.Substring(0, ns.LastIndexOf('.'));
                            }

                            if(ns.IndexOf('.') != -1)
                                ns = ns.Substring(2, ns.LastIndexOf('.') - 2);
                            else
                                ns = ns.Substring(2);

                            if(validNamespaces.Contains(ns) && !seenNamespaces.Contains(ns))
                            {
                                seenNamespaces.Add(ns);
                                yield return ns;
                            }
                        }
                }
        }
        #endregion
    }
}
