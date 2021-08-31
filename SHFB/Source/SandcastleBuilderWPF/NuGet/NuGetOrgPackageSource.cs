//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : NuGetOrgPackageSource.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/18/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains a class that implements the package source for the NuGet.org online package source
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

// Ignore Spelling: prerelease

using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;

namespace SandcastleBuilder.WPF.NuGet
{
    /// <summary>
    /// This implements the package source for the NuGet.org online package source
    /// </summary>
    public class NuGetOrgPackageSource : INuGetPackageSource
    {
        #region Private data members
        //=====================================================================

        private string searchQueryServiceUrl;
        private int skipCount;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the NuGet search query service URL by getting it from the NuGet
        /// service index.
        /// </summary>
        private string SearchQueryServiceUrl
        {
            get
            {
                if(String.IsNullOrWhiteSpace(searchQueryServiceUrl))
                {
                    try
                    {
                        using(var client = new WebClient())
                        {
                            string result = client.DownloadString(this.PackageSourceLocation);

                            if(!String.IsNullOrWhiteSpace(result))
                            {
                                var options = new JsonDocumentOptions
                                {
                                    AllowTrailingCommas = true,
                                    CommentHandling = JsonCommentHandling.Skip
                                };

                                using(JsonDocument document = JsonDocument.Parse(result, options))
                                {
                                    var root = document.RootElement;

                                    if(root.ValueKind == JsonValueKind.Object &&
                                      root.TryGetProperty("resources", out JsonElement resources))
                                    {
                                        foreach(var element in resources.EnumerateArray())
                                        {
                                            if(element.TryGetProperty("@type", out JsonElement type) &&
                                              type.GetString() == "SearchQueryService")
                                            {
                                                searchQueryServiceUrl = element.GetProperty("@id").GetString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        // Ignore exceptions.  If not available, we'll try again later.
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                }

                return searchQueryServiceUrl;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="packageSourceName">The package source name</param>
        /// <param name="packageSourceLocation">The package source location (the NuGet.org URL)</param>
        public NuGetOrgPackageSource(string packageSourceName, string packageSourceLocation)
        {
            this.PackageSourceName = packageSourceName;
            this.PackageSourceLocation = packageSourceLocation;
        }
        #endregion

        #region INuGetPackageSource implementation
        //=====================================================================

        /// <inheritdoc />
        public string PackageSourceName { get; }

        /// <inheritdoc />
        public string PackageSourceLocation { get; }

        /// <inheritdoc />
        /// <remarks>This always returns true as the online package source doesn't require indexing</remarks>
        public bool IsIndexed => true;

        /// <inheritdoc />
        public IEnumerable<NuGetPackage> SearchForPackages(IEnumerable<string> searchTerms, bool newSearch,
          bool includePreRelease)
        {
            if(newSearch)
                skipCount = 0;

            using(var client = new WebClient())
            {
                int returned = 0;
                string result = null;

                try
                {
                    result = client.DownloadString(
                        $"{this.SearchQueryServiceUrl}?q={WebUtility.UrlEncode(String.Join(" ", searchTerms))}&skip={skipCount}&prerelease={includePreRelease}");
                }
                catch(Exception ex)
                {
                    // Ignore exceptions.  If not available, we'll try again later.
                    System.Diagnostics.Debug.WriteLine(ex);
                }

                if(!String.IsNullOrWhiteSpace(result))
                {
                    var options = new JsonDocumentOptions
                    {
                        AllowTrailingCommas = true,
                        CommentHandling = JsonCommentHandling.Skip
                    };

                    using(JsonDocument document = JsonDocument.Parse(result, options))
                    {
                        var root = document.RootElement;

                        foreach(var element in root.GetProperty("data").EnumerateArray())
                        {
                            returned++;
                            yield return new NuGetPackage(element);
                        }
                    }
                }

                skipCount += returned;
            }
        }

        /// <inheritdoc />
        /// <remarks>Since this package source doesn't require indexing, it does nothing</remarks>
        public void IndexPackageSource()
        {
        }
        #endregion
    }
}
