// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history
// 12/27/2012 - EFW - Regenerated the MTPS Content Service classes.  I decided to stay with the web service as
// the authentication issues have well-known solutions (see BuildAssembler.exe.config).  Switching to a service
// type may introduce other authentication issues and since the web service works well enough, I left it as-is.
// 12/28/2012 - EFW - General code clean-up.  Added code to report the reason for MSDN service failures.  Exposed
// the MSDN ID cache via a property.
// 12/29/2012 - EFW - Change the cache type to IDictionary(string, string) and added a constructor to allow
// specification of an existing cache.  Added the CacheItemsAdded property to allow the owner to determine if
// items were added to the cache in the latest run.
// 12/31/2012 - EFW - Implemented IDisposable
// 05/23/2014 - EFW - Added code to disable lookups if the service fails for reasons other than not finding the
// content ID.  The error will be reported in the log by the caller.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Web.Services.Protocols;
using System.Linq;
using Microsoft.Ddue.Tools.MtpsContentService;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This is used to perform lookups using the Microsoft/TechNet Publishing System (MTPS) content service
    /// on .NET Framework member IDs and return the MSDN URL for them.
    /// </summary>
    public sealed class MsdnResolver : IDisposable
    {
        #region Private data members
        //=====================================================================

        private ContentService msdnService;
        private IDictionary<string, string> cachedMsdnIds;
        private bool isCacheShared;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property can be used to determine whether or not the resolver has been disposed
        /// </summary>
        public bool IsDisposed { get; private set;  }

        /// <summary>
        /// This read-only property indicates whether or not the MSDN resolver is disabled
        /// </summary>
        /// <value>If true, reference links cannot be looked up</value>
        public bool IsDisabled
        {
            get { return (msdnService == null); }
        }

        /// <summary>
        /// If disabled, this returns the reason
        /// </summary>
        public string DisabledReason { get; private set; }

        /// <summary>
        /// This is used to get or set the locale for the reference links
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// This read-only property returns the MSDN content ID cache
        /// </summary>
        /// <remarks>The key is the member ID, the value is the content ID</remarks>
        public IDictionary<string, string> MsdnContentIdCache
        {
            get { return cachedMsdnIds; }
        }

        /// <summary>
        /// This read-only property is used to determine if items were added to the cache during the latest run
        /// </summary>
        /// <value>Returns true if items were added, false if not.  This can be used to determine if the cache
        /// should be persisted in some fashion.</value>
        public bool CacheItemsAdded { get; private set; }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <remarks>The default constructor creates a simple dictionary to hold the cached MSDN content IDs</remarks>
        public MsdnResolver()
        {
            cachedMsdnIds = new Dictionary<string, string>();

            this.Locale = "en-us";

            msdnService = new ContentService();
            msdnService.appIdValue = new appId();
            msdnService.appIdValue.value = "Sandcastle";
            msdnService.SoapVersion = SoapProtocolVersion.Soap11;
        }

        /// <summary>
        /// This constructor is used to create the MSDN resolver using given existing cache
        /// </summary>
        /// <param name="msdnIdCache">A cache of existing MSDN content IDs</param>
        /// <param name="isShared">True if the cache is shared, false if not.  If not shared, the cache will
        /// be disposed of when this instance is disposed of</param>
        /// <remarks>This constructor allows you to pass in a persistent cache with preloaded values that will
        /// save looking up values that have already been determined.</remarks>
        public MsdnResolver(IDictionary<string, string> msdnIdCache, bool isShared) : this()
        {
            cachedMsdnIds = msdnIdCache;
            isCacheShared = isShared;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to get the MSDN URL for the given .NET Framework member ID
        /// </summary>
        /// <param name="id">The member ID to look up</param>
        /// <returns>The MSDN URL for the member ID or null if not found</returns>
        public string GetMsdnUrl(string id)
        {
            string endPoint = null;
            bool success = false;

            if(msdnService != null && !cachedMsdnIds.TryGetValue(id, out endPoint))
            {
                getContentRequest msdnRequest = new getContentRequest();
                msdnRequest.contentIdentifier = "AssetId:" + id;
                msdnRequest.locale = this.Locale;

                try
                {
                    getContentResponse msdnResponse = msdnService.GetContent(msdnRequest);
                    endPoint = msdnResponse.contentId;
                    success = true;
                }
                catch(WebException ex)
                {
                    // Ignore failures, just turn off the service and note the last error for the caller
                    msdnService.Dispose();
                    msdnService = null;

                    this.DisabledReason = ex.Message;
                    Exception innerEx = ex.InnerException;

                    while(innerEx != null)
                    {
                        this.DisabledReason += "\r\n" + innerEx.Message;
                        innerEx = innerEx.InnerException;
                    }

                    // Don't save changes to the cache
                    this.CacheItemsAdded = false;
                }
                catch(SoapException ex)
                {
                    if(ex.Message.IndexOf("content identifier not found", StringComparison.OrdinalIgnoreCase) != -1 ||
                      ex.Detail.OuterXml.IndexOf("mtpsContentIdentifierNotFound", StringComparison.Ordinal) != -1)
                    {
                        // Lookup failed (ID not found).  Cache the result though since it isn't there.
                        success = true;
                    }
                    else
                    {
                        // Ignore failures, just turn off the service and note the last error for the caller
                        msdnService.Dispose();
                        msdnService = null;

                        this.DisabledReason = ex.Message;
                        Exception innerEx = ex.InnerException;

                        while(innerEx != null)
                        {
                            this.DisabledReason += "\r\n" + innerEx.Message;
                            innerEx = innerEx.InnerException;
                        }

                        // Don't save changes to the cache
                        this.CacheItemsAdded = false;
                    }
                }

                // We'll cache the result but will only mark the cache as changed if successful so as not to
                // save null results from failures caused by issues other than not being found.
                cachedMsdnIds[id] = endPoint;

                if(success)
                    this.CacheItemsAdded = true;
            }

            if(String.IsNullOrEmpty(endPoint))
                return null;

            return String.Format(CultureInfo.InvariantCulture, "http://msdn2.microsoft.com/{0}/library/{1}",
                this.Locale, endPoint);
        }
        #endregion

        #region IDisposable members
        //=====================================================================

        /// <inheritdoc />
        public void Dispose()
        {
            if(!this.IsDisposed)
            {
                this.IsDisposed = true;

                if(msdnService != null)
                    msdnService.Dispose();

                // If not shared and the dictionary type implements IDisposable, dispose of it too
                if(!isCacheShared)
                {
                    IDisposable d = cachedMsdnIds as IDisposable;

                    if(d != null)
                        d.Dispose();
                }

                GC.SuppressFinalize(this);
            }
        }
        #endregion
    }
}
