//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : NuGetPackage.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/23/2025
// Note    : Copyright 2021-2025, Eric Woodruff, All rights reserved
//
// This file contains a class that is used to contain the details about a NuGet package
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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Xml.Linq;

namespace SandcastleBuilder.WPF.NuGet
{
    /// <summary>
    /// This class is used to contain the details about a NuGet package
    /// </summary>
    public class NuGetPackage : INotifyPropertyChanged
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the package ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// This read-only property returns the package title
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// This read-only property returns the latest version number
        /// </summary>
        public string LatestVersion { get; }

        /// <summary>
        /// This read-only property returns the package description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// This read-only property returns the package icon URL
        /// </summary>
        public string IconUrl { get; }

        /// <summary>
        /// This read-only property returns the package license URL
        /// </summary>
        public string LicenseUrl { get; }

        /// <summary>
        /// This read-only property returns the package project URL
        /// </summary>
        public string ProjectUrl { get; }

        /// <summary>
        /// This read-only property returns the package authors
        /// </summary>
        public string Authors { get; }

        /// <summary>
        /// This read-only property returns the total number of downloads for the package
        /// </summary>
        public long TotalDownloads { get; }

        /// <summary>
        /// This read-only property returns the available versions of the package
        /// </summary>
        public IEnumerable<string> Versions { get; }

        /// <summary>
        /// This read-only property returns an enumerable list of the tags for the package
        /// </summary>
        public IEnumerable<string> Tags { get; }

        /// <summary>
        /// This read-only property returns true if the package information came from an online source or false
        /// if it came from a local package cache.
        /// </summary>
        public bool IsFromOnlineSource { get; }

        /// <summary>
        /// This read-only property returns the URL for the package on NuGet.org if the package information came
        /// from an online source.
        /// </summary>
        public string NuGetUrl => $"https://www.nuget.org/packages/{this.Id}";

        /// <summary>
        /// This is used to get or set whether or not the package is installed
        /// </summary>
        public bool IsInstalled
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(IsLatestVersion));
            }
        }

        /// <summary>
        /// This is used to get or set the installed version
        /// </summary>
        public string InstalledVersion
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(IsLatestVersion));
            }
        }

        /// <summary>
        /// This read-only property returns whether or not the installed version is the latest version
        /// </summary>
        public bool IsLatestVersion => !this.IsInstalled || this.InstalledVersion == this.LatestVersion;

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// This creates a new instance from a JSON element typically obtained by querying an online package source
        /// </summary>
        /// <param name="packageInfo">The JSON element from which to get the NuGet package information</param>
        /// <overloads>There are three overloads for the constructor</overloads>
        public NuGetPackage(JsonElement packageInfo)
        {
            this.IsFromOnlineSource = true;
            this.Id = packageInfo.GetProperty("id").GetString();
            this.Description = packageInfo.GetProperty("description").GetString();
            this.LatestVersion = packageInfo.GetProperty("version").GetString();
            this.Authors = String.Join(", ", packageInfo.GetProperty("authors").EnumerateArray().Select(a => a.GetString()));
            this.TotalDownloads = packageInfo.GetProperty("totalDownloads").GetInt64();

            var versions = new HashSet<string>(packageInfo.GetProperty("versions").EnumerateArray().Select(
                v => v.GetProperty("version").GetString()), StringComparer.OrdinalIgnoreCase).ToList();
            versions.Sort((v1, v2) => CompareSemanticVersion(v2, v1));

            this.Versions = versions;

            if(packageInfo.TryGetProperty("title", out JsonElement title))
                this.Title = title.GetString();

            if(packageInfo.TryGetProperty("projectUrl", out JsonElement projectUrl))
                this.ProjectUrl = projectUrl.GetString();

            if(packageInfo.TryGetProperty("iconUrl", out JsonElement iconUrl))
                this.IconUrl = iconUrl.GetString();

            if(packageInfo.TryGetProperty("licenseUrl", out JsonElement licenseUrl))
                this.LicenseUrl = licenseUrl.GetString();

            if(packageInfo.TryGetProperty("tags", out JsonElement tags))
            {
                this.Tags = new HashSet<string>(tags.EnumerateArray().Select(t => t.GetString()),
                    StringComparer.OrdinalIgnoreCase);
            }
            else
                this.Tags = new HashSet<string>();
        }

        /// <summary>
        /// This creates a new instance from an XElement typically obtained by querying a local package source
        /// containing expanded packages or just the package files themselves.
        /// </summary>
        /// <param name="nuSpecContent">The .nuspec content from which to get the NuGet package information</param>
        /// <param name="versions">An enumerable list of the package versions</param>
        public NuGetPackage(XDocument nuSpecContent, IEnumerable<string> versions)
        {
            if(nuSpecContent == null)
                throw new ArgumentNullException(nameof(nuSpecContent));

            if(versions == null)
                throw new ArgumentNullException(nameof(versions));

            var nugetNS = nuSpecContent.Root.GetDefaultNamespace();
            var metadata = nuSpecContent.Root.Element(nugetNS + "metadata");

            this.Id = metadata.Element(nugetNS + "id").Value;
            this.Description = metadata.Element(nugetNS + "description").Value;
            this.LatestVersion = metadata.Element(nugetNS + "version").Value;
            this.Authors = metadata.Element(nugetNS + "authors").Value;

            var sortVersions = new HashSet<string>(versions, StringComparer.OrdinalIgnoreCase).ToList();
            sortVersions.Sort((v1, v2) => CompareSemanticVersion(v2, v1));

            this.Versions = sortVersions;

            this.Title = (string)metadata.Element(nugetNS + "title");
            this.ProjectUrl = (string)metadata.Element(nugetNS + "projectUrl");
            this.IconUrl = (string)metadata.Element(nugetNS + "iconUrl");
            this.LicenseUrl = (string)metadata.Element(nugetNS + "licenseUrl");
            this.Tags = new HashSet<string>(((string)metadata.Element(nugetNS + "tags") ?? String.Empty).Split(
                [' ', '\t', ',', ';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries),
                StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// This creates a new instance (clone) from an existing instance
        /// </summary>
        /// <param name="package">The package to clone</param>
        public NuGetPackage(NuGetPackage package)
        {
            if(package == null)
                throw new ArgumentNullException(nameof(package));

            this.Id = package.Id;
            this.Description = package.Description;
            this.LatestVersion = package.LatestVersion;
            this.Authors = package.Authors;

            var sortVersions = new HashSet<string>(package.Versions, StringComparer.OrdinalIgnoreCase).ToList();
            sortVersions.Sort((v1, v2) => CompareSemanticVersion(v2, v1));

            this.Versions = sortVersions;

            this.Title = package.Title;
            this.ProjectUrl = package.ProjectUrl;
            this.IconUrl = package.IconUrl;
            this.LicenseUrl = package.LicenseUrl;
            this.Tags = new HashSet<string>(package.Tags, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// This is used to create a dummy entry for an installed package that no longer exists in any of the
        /// package sources.
        /// </summary>
        /// <param name="id">The ID of the package</param>
        /// <param name="version">The package version</param>
        public NuGetPackage(string id, string version)
        {
            if(id == null)
                throw new ArgumentNullException(nameof(id));

            this.Id = this.Description = id;
            this.LatestVersion = version;
            this.Title = this.Authors = "(Package not found)";
            this.Versions = [version];
        }
        #endregion

        #region INotifyPropertyChanged implementation
        //=====================================================================

        /// <summary>
        /// The property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This raises the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="propertyName">The property name that changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Combine the version information from the given package with this one
        /// </summary>
        /// <param name="otherVersions">An enumerable list of other versions to combine with this package</param>
        public void CombineVersions(IEnumerable<string> otherVersions)
        {
            var allVersions = (List<string>)this.Versions;

            if(otherVersions != null && otherVersions.Except(allVersions).Any())
            {
                allVersions.AddRange(otherVersions.Except(allVersions));
                allVersions.Sort((v1, v2) => CompareSemanticVersion(v2, v1));
            }
        }

        /// <summary>
        /// Compare two semantic version numbers
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static int CompareSemanticVersion(string first, string second)
        {
            if(first == null)
                throw new ArgumentNullException(nameof(first));

            if(second == null)
                throw new ArgumentNullException(nameof(second));

            int pos = 0;

            while(pos < first.Length && ((first[pos] >= '0' && first[pos] <= '9') || first[pos] == '.'))
                pos++;

            Version v1 = new(first.Substring(0, pos));

            pos = 0;

            while(pos < second.Length && ((second[pos] >= '0' && second[pos] <= '9') || second[pos] == '.'))
                pos++;

            Version v2 = new(second.Substring(0, pos));

            if(v1 == v2)
                return String.Compare(first, second, StringComparison.OrdinalIgnoreCase);

            if(v1 > v2)
                return 1;

            return -1;
        }
        #endregion
    }
}
