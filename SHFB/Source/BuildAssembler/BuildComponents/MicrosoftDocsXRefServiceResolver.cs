//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : MicrosoftDocsXRefServiceResolver.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/11/2021
// Note    : Copyright 2019-2021, Eric Woodruff, All rights reserved
//
// This file contains a class used to perform lookups using the Microsoft Docs cross-reference service on .NET
// Framework member IDs and return the URL for them.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/12/2019  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Threading;

using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Sandcastle.Tools.BuildComponents
{
    /// <summary>
    /// This is used to perform lookups using the Microsoft Docs cross-reference service on .NET Framework member
    /// IDs and return the URL for them.
    /// </summary>
    public sealed class MicrosoftDocsXRefServiceResolver : IMemberIdUrlResolver
    {
        #region Private data members
        //=====================================================================

        private WebClient client;
        private readonly bool isCacheShared;

        // These are static as multiple instances of this component may exist and we need to know if any of them
        // add values to the shared cache.
        private static int cacheItemsAdded, lookupErrors;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get the cross-reference URL used to perform the lookup
        /// </summary>
        /// <value>It should contain a single format parameter ({0}) that will be replaced by the member ID.
        /// If not set, the default is "https://xref.docs.microsoft.com/query?uid={0}"</value>
        public string CrossReferenceUrlFormat { get; set; }

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <remarks>The default constructor creates a simple dictionary to hold the cached URLs</remarks>
        /// <overloads>There are two overloads for the constructor</overloads>
        public MicrosoftDocsXRefServiceResolver()
        {
            client = new WebClient();

            this.CrossReferenceUrlFormat = "https://xref.docs.microsoft.com/query?uid={0}";
            this.CachedUrls = new Dictionary<string, string>();
        }

        /// <summary>
        /// This constructor is used to create the resolver using an existing cache
        /// </summary>
        /// <param name="urlCache">A cache of existing member ID URLs</param>
        /// <param name="isShared">True if the cache is shared, false if not.  If not shared, the cache will
        /// be disposed of when this instance is disposed of</param>
        /// <remarks>This constructor allows you to pass in a persistent cache with preloaded values that will
        /// save looking up values that have already been determined.</remarks>
        public MicrosoftDocsXRefServiceResolver(IDictionary<string, string> urlCache, bool isShared) : this()
        {
            this.CachedUrls = urlCache;
            isCacheShared = isShared;
        }
        #endregion

        #region IMemberIdResolver implementation
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>This is not used by this resolver</remarks>
        public string Locale { get; set; }

        /// <inheritdoc />
        public bool IsDisposed { get; private set; }

        /// <inheritdoc />
        public bool IsDisabled => client == null;

        /// <inheritdoc />
        public string DisabledReason { get; private set; }

        /// <inheritdoc />
        public IDictionary<string, string> CachedUrls { get; }

        /// <inheritdoc />
        public int CacheItemsAdded => (lookupErrors == 0) ? cacheItemsAdded : 0;

        /// <inheritdoc />
        public void Dispose()
        {
            if(!this.IsDisposed)
            {
                this.IsDisposed = true;

                if(client != null)
                    client.Dispose();

                // If not shared and the dictionary type implements IDisposable, dispose of it too
                if(!isCacheShared)
                {
                    if(this.CachedUrls is IDisposable d)
                        d.Dispose();
                }

                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// This is used to get the Microsoft Docs URL for the given .NET Framework member ID
        /// </summary>
        /// <param name="id">The member ID to look up</param>
        /// <returns>The URL for the member ID or null if not found</returns>
        public string ResolveUrlForId(string id)
        {
            if(id == null)
                throw new ArgumentNullException(id);

            string url = null;
            bool success = true;

            if(client != null && !this.CachedUrls.TryGetValue(id, out url))
            {
                try
                {
                    // The cross reference service doesn't include the member type prefix
                    string idWithoutPrefix = id;

                    if(idWithoutPrefix.Length > 2 && (idWithoutPrefix[1] == ':' || idWithoutPrefix.StartsWith(
                      "Overload:", StringComparison.Ordinal)))
                    {
                        idWithoutPrefix = idWithoutPrefix.Substring(idWithoutPrefix.IndexOf(':') + 1);
                    }

                    string result = client.DownloadString(String.Format(CultureInfo.InvariantCulture,
                        this.CrossReferenceUrlFormat, Uri.EscapeDataString(idWithoutPrefix)));

                    if(!String.IsNullOrWhiteSpace(result))
                    {
                        var options = new JsonDocumentOptions { AllowTrailingCommas = true,
                            CommentHandling = JsonCommentHandling.Skip };

                        using(JsonDocument document = JsonDocument.Parse(result, options))
                        {
                            var root = document.RootElement;

                            if(root.ValueKind == JsonValueKind.Array && root.GetArrayLength() != 0 &&
                              root[0].TryGetProperty("href", out JsonElement href))
                            {
                                url = href.GetString();
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    success = false;

                    // Ignore failures, just turn off the service and note the last error for the caller
                    client.Dispose();
                    client = null;

                    this.DisabledReason = ex.Message;
                    Exception innerEx = ex.InnerException;

                    while(innerEx != null)
                    {
                        this.DisabledReason += "\r\n" + innerEx.Message;
                        innerEx = innerEx.InnerException;
                    }

                    // Don't save changes to the cache
                    Interlocked.Increment(ref lookupErrors);
                }

                // We'll cache the result but will only mark the cache as changed if successful so as not to
                // save null results from failures caused by issues other than not being found.
                this.CachedUrls[id] = url;

                if(success)
                    Interlocked.Increment(ref cacheItemsAdded);
            }

            return url;
        }
        #endregion
    }
}
