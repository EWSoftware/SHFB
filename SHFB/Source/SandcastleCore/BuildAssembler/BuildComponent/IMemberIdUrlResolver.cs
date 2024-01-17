//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IMemberIdUrlResolver.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/16/2024
// Note    : Copyright 2019-2024, Eric Woodruff, All rights reserved
//
// This file contains a class that defines an interface used to resolve an API member ID to an online help
// website URL.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/15/2019  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;

namespace Sandcastle.Core.BuildAssembler.BuildComponent
{
    /// <summary>
    /// This defines the interface used to resolve an API member ID to an online help website URL
    /// </summary>
    public interface IMemberIdUrlResolver : IDisposable
    {
        /// <summary>
        /// This read-only property returns the base URL for the links
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// This read-only property can be used to determine whether or not the resolver has been disposed
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// This read-only property returns the URL cache
        /// </summary>
        /// <remarks>The key is the member ID, the value is the member URL</remarks>
        IDictionary<string, string> CachedUrls { get; }

        /// <summary>
        /// This is used to get the help website URL for the given member ID
        /// </summary>
        /// <param name="id">The member ID to convert to a URL</param>
        /// <param name="fragmentId">The ID to use for the URL fragment or null if there isn't one</param>
        /// <returns>The URL for the member ID or null if it could not be resolved</returns>
        string ResolveUrlForId(string id, string fragmentId);
    }
}
