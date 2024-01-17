//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : MicrosoftLearnMemberIdUrlResolver.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/16/2024
// Note    : Copyright 2024, Eric Woodruff, All rights reserved
//
// This file contains a class used to convert member IDs for .NET Framework class members to their Microsoft
// Learn URL.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/16/2024  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Text;

using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Sandcastle.Tools.BuildComponents
{
    /// <summary>
    /// This class is used to convert member IDs for .NET Framework class members to their Microsoft Learn URL
    /// </summary>
    public sealed class MicrosoftLearnMemberIdUrlResolver : IMemberIdUrlResolver
    {
        #region Private data members
        //=====================================================================

        private readonly bool isCacheShared;
        private readonly StringBuilder urlTarget;

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <remarks>The default constructor creates a simple dictionary to hold the cached URLs</remarks>
        /// <overloads>There are two overloads for the constructor</overloads>
        public MicrosoftLearnMemberIdUrlResolver()
        {
            this.CachedUrls = new Dictionary<string, string>();

            urlTarget = new StringBuilder(2048);
        }

        /// <summary>
        /// This constructor is used to create the resolver using an existing cache
        /// </summary>
        /// <param name="urlCache">A cache of existing member ID URLs</param>
        /// <param name="isShared">True if the cache is shared, false if not.  If not shared, the cache will
        /// be disposed of when this instance is disposed of</param>
        /// <remarks>This constructor allows you to pass in a persistent cache with preloaded values that will
        /// save looking up values that have already been determined.</remarks>
        public MicrosoftLearnMemberIdUrlResolver(IDictionary<string, string> urlCache, bool isShared) : this()
        {
            this.CachedUrls = urlCache;
            isCacheShared = isShared;
        }
        #endregion

        #region IMemberIdResolver implementation
        //=====================================================================

        // TODO: This can changed based on the containing assembly.  Most are in the .NET API root URL though.
        // May add support for other base URLs later if needed.
        /// <summary>
        /// This read-only property returns the base URL for the links
        /// </summary>
        public string BaseUrl { get; } = "https://learn.microsoft.com/dotnet/api/";

        /// <inheritdoc />
        public bool IsDisposed { get; private set; }

        /// <inheritdoc />
        public IDictionary<string, string> CachedUrls { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            if(!this.IsDisposed)
            {
                this.IsDisposed = true;

                // If not shared and the dictionary type implements IDisposable, dispose of it too
                if(!isCacheShared)
                {
                    if(this.CachedUrls is IDisposable d)
                        d.Dispose();
                }

                GC.SuppressFinalize(this);
            }
        }

        /// <inheritdoc />
        public string ResolveUrlForId(string id, string fragmentId)
        {
            if(String.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(id);

            if(!this.CachedUrls.TryGetValue(fragmentId ?? id, out string url))
            {
                // Ignore the ID type
                string urlId = id.ToLowerInvariant();
                int typeSeparatorPos = id.IndexOf(':');

                if(typeSeparatorPos != -1)
                    urlId = urlId.Substring(typeSeparatorPos + 1);

                switch(id[0])
                {
                    case 'N':
                    case 'T':
                        url = $"{this.BaseUrl}{urlId.Replace('`', '-')}";
                        break;

                    case 'E':
                    case 'F':
                    case 'M':
                    case 'O':
                    case 'P':
                        urlTarget.Clear();

                        // Ignore parameters
                        int parametersIndex = urlId.IndexOf('(');

                        if(parametersIndex != -1)
                            urlId = urlId.Substring(0, parametersIndex);

                        // Ignore method generic type parameters but keep type generic type parameters
                        int nameIndex = urlId.LastIndexOf('.');
                        parametersIndex = urlId.Substring(nameIndex + 1).IndexOf('`');
                        
                        if(parametersIndex != -1)
                            urlId = urlId.Substring(0, nameIndex + parametersIndex + 1);

                        // Convert invalid URL characters to dashes
                        foreach(var c in urlId)
                        {
                            switch(c)
                            {
                                case '#':
                                case '{':
                                case '}':
                                case '`':
                                case '@':
                                    urlTarget.Append('-');
                                    break;

                                default:
                                    urlTarget.Append(c);
                                    break;
                            }
                        }

                        url = $"{this.BaseUrl}{urlTarget}";
                        break;

                    default:
                        break;
                }

                if(!String.IsNullOrWhiteSpace(fragmentId))
                {
                    urlTarget.Clear();
                    urlTarget.Append('#');

                    urlId = fragmentId.ToLowerInvariant();

                    for(int i = urlId.IndexOf(':') + 1; i < urlId.Length; i++)
                    {
                        char ch = urlId[i];

                        // Ignore or translate characters as needed
                        switch(ch)
                        {
                            case '"':
                            case '\'':
                            case '%':
                            case '^':
                            case '\\':
                                continue;

                            case '<':
                            case '[':
                            case '(':
                                urlTarget.Append('(');
                                break;

                            case '>':
                            case ']':
                            case ')':
                                urlTarget.Append(')');
                                break;

                            case '{':
                                urlTarget.Append("((");
                                break;

                            case '}':
                                urlTarget.Append("))");
                                break;

                            default:
                                if(ch == '*' || ch == '@' || (ch >= 'a' && ch <= 'z') ||
                                  (ch >= '0' && ch <= '9'))
                                {
                                    urlTarget.Append(ch);
                                }
                                else
                                {
                                    if(urlTarget.Length != 0 && urlTarget[urlTarget.Length - 1] != '-')
                                        urlTarget.Append('-');
                                }
                                break;
                        }
                    }

                    // Remove trailing dashes
                    while(urlTarget.Length > 1 && urlTarget[urlTarget.Length - 1] == '-')
                        urlTarget.Remove(urlTarget.Length - 1, 1);

                    if(urlTarget.Length != 1)
                        url += urlTarget;
                }

                this.CachedUrls.Add(fragmentId ?? id, url);
            }

            return url;
        }
        #endregion
    }
}
