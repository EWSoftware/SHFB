//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IMemberIdUrlResolver.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/15/2019
// Note    : Copyright 2019, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
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
        /// This is used to get or set the locale for the reference links if applicable to the resolver
        /// </summary>
        string Locale { get; set; }

        /// <summary>
        /// This read-only property can be used to determine whether or not the resolver has been disposed
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// This read-only property indicates whether or not the resolver is disabled
        /// </summary>
        /// <value>If true, reference links cannot be looked up</value>
        bool IsDisabled { get; }

        /// <summary>
        /// This read-only property returns the reason the resolver is disabled if <see cref="IsDisabled"/>
        /// returns true.
        /// </summary>
        string DisabledReason { get; }

        /// <summary>
        /// This read-only property returns the URL cache
        /// </summary>
        /// <remarks>The key is the member ID, the value is the member URL</remarks>
        IDictionary<string, string> CachedUrls { get; }

        /// <summary>
        /// This read-only property is used to return the number of items added to the cache during the latest
        /// run.
        /// </summary>
        /// <value>Returns a non-zero number if items were added or zero if no new items were added or errors
        /// occurred that invalidate the updates made.  This can be used to determine if the cache should be
        /// persisted in some fashion.</value>
        int CacheItemsAdded { get; }

        /// <summary>
        /// This is used to get the help website URL for the given .NET Framework member ID
        /// </summary>
        /// <param name="id">The member ID to look up</param>
        /// <returns>The URL for the member ID or null if not found or an error occurred looking it up</returns>
        string ResolveUrlForId(string id);
    }
}
