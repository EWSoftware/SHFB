//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : LocalPackageSource.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/24/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains a class that implements the package source for local file-based package sources
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/06/2021  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace SandcastleBuilder.WPF.NuGet
{
    /// <summary>
    /// This implements the package source for local file-based package sources
    /// </summary>
    public class LocalPackageSource : INuGetPackageSource
    {
        #region NuGet package index entry
        //=====================================================================

        /// <summary>
        /// This is used to create the package index
        /// </summary>
        private class IndexEntry
        {
            /// <summary>
            /// The package ID
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// The package version
            /// </summary>
            public string Version { get; set; }

            /// <summary>
            /// The package file used to create the index entry
            /// </summary>
            public string PackageFile { get; set; }
            
            /// <summary>
            /// The content of the .nuspec file containing the package metadata
            /// </summary>
            public XDocument NuSpecContent { get; set; }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private readonly List<NuGetPackage> packages;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="packageSourceName">The package source name</param>
        /// <param name="packageSourceLocation">The package source location (the path to the package cache)</param>
        public LocalPackageSource(string packageSourceName, string packageSourceLocation)
        {
            this.PackageSourceName = packageSourceName;
            this.PackageSourceLocation = packageSourceLocation;

            packages = new List<NuGetPackage>();
        }
        #endregion

        #region INuGetPackageSource implementation
        //=====================================================================

        /// <inheritdoc />
        public string PackageSourceName { get; }

        /// <inheritdoc />
        public string PackageSourceLocation { get; }

        /// <inheritdoc />
        public bool IsIndexed { get; private set; }

        /// <inheritdoc />
        /// <remarks>Since this is local, it always returns all results and will always return an empty set on
        /// subsequent calls when <paramref name="newSearch"/> is false.  This mimics the online search as
        /// closely as possible.  The special prefix "PackageId:" on a search term is supported to limit a search
        /// term to the package ID for an exact case-insensitive match.  The special prefix "id:" on a search
        /// term is supported to limit the search to any part of the ID.  Terms with any other prefix are
        /// ignored.  The "owner:" prefix is not supported here as we don't have access to that property in the
        /// local package sources.  The <paramref name="includePreRelease"/> option is currently ignored.  All
        /// versions will be returned including pre-release versions.</remarks>
        public IEnumerable<NuGetPackage> SearchForPackages(IEnumerable<string> searchTerms, bool newSearch,
          bool includePreRelease)
        {
            if(!newSearch)
                yield break;

            if(searchTerms == null)
                throw new ArgumentNullException(nameof(searchTerms));

            HashSet<string> packageIdTerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                idTerms = idTerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                generalTerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach(string term in searchTerms)
            {
                if(term.StartsWith("PackageId:", StringComparison.OrdinalIgnoreCase))
                    packageIdTerms.Add(term.Substring(10).Replace("\"", String.Empty).Trim());
                else
                {
                    if(term.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
                    {
                        // The behavior online appears to be the ID or any part of the ID so it may return other
                        // packages that match any part of the given ID as well.  For example "EWSoftware.SHFB"
                        // will match "EWSoftware.SHFB" as well as all other "EWSoftware.*" packages.  Quote
                        // marks around the ID don't appear to make a difference.
                        string t = term.Substring(3).Replace("\"", String.Empty).Trim();

                        packageIdTerms.Add(t);
                        packageIdTerms.UnionWith(t.Split(new[] { ' ', '\t', ',', ';', '.' },
                            StringSplitOptions.RemoveEmptyEntries));
                    }
                    else
                    {
                        if(term.IndexOf(':') == -1)
                            generalTerms.Add(term);
                    }
                }
            }

            foreach(var p in packages)
            {
                // If used in conjunction, the terms are restrictive.  So, if any condition doesn't find a match,
                // we skip it.
                if(packageIdTerms.Count != 0 && !packageIdTerms.Contains(p.Id))
                    continue;

                if(idTerms.Count != 0 && !idTerms.Any(t => p.Id.IndexOf(t, StringComparison.OrdinalIgnoreCase) != -1))
                    continue;

                var searchTags = new HashSet<string>(p.Tags.Concat(new[] { p.Id }).Concat(
                    p.Id.Split(new[] { ' ', '\t', ',', ';', '.' }, StringSplitOptions.RemoveEmptyEntries)).Concat(
                    (p.Title ?? String.Empty).Split(new[] { ' ', '\t', ',', ';', '\r', '\n' },
                        StringSplitOptions.RemoveEmptyEntries)).Concat(p.Description.Split(
                            new[] { ' ', '\t', ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)),
                                StringComparer.OrdinalIgnoreCase);

                // Return a clone, not the cached instance as the caller may update the properties to indicate
                // whether or not it is installed etc.
                if(generalTerms.Count == 0 || generalTerms.All(t => searchTags.Contains(t)))
                    yield return new NuGetPackage(p);
            }
        }

        /// <inheritdoc />
        public void IndexPackageSource()
        {
            if(!this.IsIndexed)
            {
                var index = new List<IndexEntry>();

                // Search for .nuspec files first.  If found, this is probably the global cache and the files will
                // already have been extracted.
                foreach(string filename in Directory.EnumerateFiles(this.PackageSourceLocation, "*.nuspec",
                  SearchOption.AllDirectories))
                {
                    var nuspec = XDocument.Load(filename);
                    var nugetNS = nuspec.Root.GetDefaultNamespace();
                    var metadata = nuspec.Root.Element(nugetNS + "metadata");

                    index.Add(new IndexEntry
                    {
                        Id = metadata.Element(nugetNS + "id").Value,
                        Version = metadata.Element(nugetNS + "version").Value,
                        PackageFile = filename,
                        NuSpecContent = nuspec
                    });
                }

                // If none are found, search for .nupkg files as it's probably a local package source containing
                // the packages.
                if(index.Count == 0)
                {
                    foreach(string filename in Directory.EnumerateFiles(this.PackageSourceLocation, "*.nupkg",
                      SearchOption.AllDirectories))
                    {
                        using(var nupkg = ZipFile.OpenRead(filename))
                        {
                            var nuspecFile = nupkg.Entries.FirstOrDefault(e => e.Name.EndsWith(".nuspec",
                                StringComparison.OrdinalIgnoreCase));

                            if(nuspecFile != null)
                            {
                                using(var sr = new StreamReader(nuspecFile.Open()))
                                {
                                    var nuspec = XDocument.Parse(sr.ReadToEnd());
                                    var nugetNS = nuspec.Root.GetDefaultNamespace();
                                    var metadata = nuspec.Root.Element(nugetNS + "metadata");

                                    index.Add(new IndexEntry
                                    {
                                        Id = metadata.Element(nugetNS + "id").Value,
                                        Version = metadata.Element(nugetNS + "version").Value,
                                        PackageFile = filename,
                                        NuSpecContent = nuspec
                                    });
                                }
                            }
                        }
                    }
                }

                // Group the packages by ID and consolidate the version information
                foreach(var g in index.GroupBy(i => i.Id))
                {
                    var first = g.OrderByDescending(v => v.Version).First();

                    packages.Add(new NuGetPackage(first.NuSpecContent, g.Select(v => v.Version)));
                }

                this.IsIndexed = true;
            }
        }
        #endregion
    }
}
