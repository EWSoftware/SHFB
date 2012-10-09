// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Services.Protocols;

namespace Microsoft.Ddue.Tools {

    public class MsdnResolver {

        public MsdnResolver () {
            msdnService = new ContentService();
            msdnService.appIdValue = new appId();
            msdnService.appIdValue.value = "Sandcastle";
            msdnService.SoapVersion = SoapProtocolVersion.Soap11;
        }

        public bool IsDisabled {
            get {
                return ((msdnService == null));
            }
        }

        public string Locale {
            get {
                return (locale);
            }
            set {
                locale = value;
            }
        }

        public string GetMsdnUrl (string id) {

            if (cachedMsdnUrls.ContainsKey(id)) return (String.Format(urlFormat, locale, cachedMsdnUrls[id]));

            if (msdnService == null) return(null);

            getContentRequest msdnRequest = new getContentRequest();
            msdnRequest.contentIdentifier = "AssetId:" + id;
            msdnRequest.locale = locale;

            string endpoint = null;
            try {
                getContentResponse msdnResponse = msdnService.GetContent(msdnRequest);
                endpoint = msdnResponse.contentId;
            } catch (WebException) {
                msdnService = null;
            } catch (SoapException) {
                // lookup failed
            }

            cachedMsdnUrls[id] = endpoint;

            if (String.IsNullOrEmpty(endpoint)) {
                return (null);
            } else {
                return (String.Format(urlFormat, locale, endpoint));
            }
        }

        private ContentService msdnService;

        private string locale = "en-us";

        private static string urlFormat = "http://msdn2.microsoft.com/{0}/library/{1}";

        private Dictionary<string, string> cachedMsdnUrls = new Dictionary<string, string>();

    }

}
