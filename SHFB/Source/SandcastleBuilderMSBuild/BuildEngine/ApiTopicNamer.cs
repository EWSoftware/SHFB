//===============================================================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : ApiTopicNamer.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/21/2025
// Note    : Copyright 2021-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to generate API member topic filenames based on the selected naming method
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/01/2021  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using Sandcastle.Core;
using Sandcastle.Core.BuildEngine;
using Sandcastle.Core.Project;

namespace SandcastleBuilder.MSBuild.BuildEngine
{
    /// <summary>
    /// This is used to generate topic filenames for API members based on the selected naming method
    /// </summary>
    public sealed class ApiTopicNamer : IDisposable
    {
        #region Private data members
        //=====================================================================

        private readonly IBuildProcess buildProcess;
        private readonly HashSet<string> filenames;

        private HashAlgorithm md5;

        private static readonly Regex reInvalidChars = new("[ :.`@#{}<>*?|]");

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildProcess">The build process using the topic namer</param>
        public ApiTopicNamer(IBuildProcess buildProcess)
        {
            this.buildProcess = buildProcess ?? throw new ArgumentNullException(nameof(buildProcess));
            filenames = [];
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the instance if not done explicitly
        /// with <see cref="Dispose()"/>.
        /// </summary>
        ~ApiTopicNamer()
        {
            this.Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            md5?.Dispose();

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to get an API topic filename for the given API member ID
        /// </summary>
        /// <param name="memberId">The member ID for which to get a topic filename</param>
        /// <returns>The topic filename for the member ID based on the specified naming method</returns>
        public string ToTopicFileName(string memberId)
        {
            if(memberId == null)
                throw new ArgumentNullException(nameof(memberId));

            switch(buildProcess.CurrentProject.NamingMethod)
            {
                case NamingMethod.Guid:
                    return this.ToMd5Hash(memberId);

                case NamingMethod.MemberName:
                    return this.ToMemberName(memberId);

                case NamingMethod.HashedMemberName:
                    return this.ToHashedMemberName(memberId);

                default:
                    throw new InvalidOperationException("Unknown naming method");
            }
        }

        /// <summary>
        /// This generates an MD5 hash of the member ID and returns it in GUID form
        /// </summary>
        /// <param name="memberId">The member ID</param>
        /// <returns>The MD5 hash of the member ID in GUID form</returns>
        private string ToMd5Hash(string memberId)
        {
            // Create on first use to prevent issues if FIPS is enabled
            if(md5 == null)
            {
                try
                {
                    md5 = MD5.Create();
                }
                catch(Exception ex)
                {
                    // The MD5 hashing algorithm can be blocked if the FIPS policy is enabled.  In such cases,
                    // tell the user to switch to Member Name or Hashed Member Name as the naming method.  This
                    // is rare and using the alternate naming method is easier than replacing the MD5 hashing
                    // which would be a significant breaking change after all these years.
                    Exception fipsEx = ex;

                    while(fipsEx != null)
                    {
                        if(fipsEx is InvalidOperationException && fipsEx.Message.IndexOf(" FIPS ",
                          StringComparison.Ordinal) != -1)
                        {
                            break;
                        }

                        fipsEx = fipsEx.InnerException;
                    }

                    if(fipsEx == null)
                        throw;
                    
                    throw new BuilderException("BE0036", "The FIPS validated cryptographic algorithms policy " +
                        "appears to be enabled.  This prevents the MD5 hashing algorithm from being used to " +
                        "generate GUID topic filenames.  Change the project's Naming Method property to " +
                        "either Member Name or Hashed Member Name which do not rely on the MD5 hashing " +
                        "algorithm.", ex);
                }
            }

            byte[] input = Encoding.UTF8.GetBytes(memberId);
            byte[] output = md5.ComputeHash(input);
            Guid guid = new(output);

            return guid.ToString();
        }

        /// <summary>
        /// This modifies the member ID so that it can be used as a filename
        /// </summary>
        /// <param name="memberId">The member ID</param>
        /// <returns>The member ID in a form suitable for use as a filename.  If a duplicate is found, the name
        /// is made unique by adding a numeric suffix.</returns>
        private string ToMemberName(string memberId)
        {
            bool duplicate;
            string memberName = memberId;

            // Remove parameters
            int idx = memberName.IndexOf('(');

            if(idx != -1)
                memberName = memberName.Substring(0, idx);

            // Replace invalid filename characters with an underscore
            string newName = memberName = reInvalidChars.Replace(memberName, "_");

            idx = 0;

            do
            {
                // Check for a duplicate (i.e. an overloaded member).  These will be made unique by adding a
                // counter to the end of the name.
                duplicate = filenames.Contains(newName);

                if(duplicate)
                {
                    idx++;
                    newName = $"{memberName}_{idx}";
                }

            } while(duplicate);

            // Log duplicates that had unique names created
            if(idx != 0)
                buildProcess.ReportProgress("    Unique name {0} generated for {1}", newName, memberId);

            filenames.Add(newName);

            return newName;
        }

        /// <summary>
        /// This returns the hash for the member ID to use as a filename.  Hash codes can be used to shorten
        /// extremely long type and member names.
        /// </summary>
        /// <param name="memberId">The member ID</param>
        /// <returns>The hash for the member ID.  If a duplicate is found, the name is made unique by adding a
        /// numeric suffix.</returns>
        private string ToHashedMemberName(string memberId)
        {
            bool duplicate;
            string memberName = memberId;

            // Remove parameters
            int idx = memberName.IndexOf('(');

            if(idx != -1)
                memberName = memberName.Substring(0, idx);

            string newName = memberName;

            idx = 0;

            do
            {
                newName = newName.GetHashCodeDeterministic().ToString("X", CultureInfo.InvariantCulture);

                // Check for a duplicate (i.e. an overloaded member).  These will be made unique by adding a
                // counter to the end of the name.
                duplicate = filenames.Contains(newName);

                if(duplicate)
                {
                    idx++;
                    newName = $"{memberName}_{idx}";
                }

            } while(duplicate);

            // Log duplicates that had unique names created
            if(idx != 0)
                buildProcess.ReportProgress("    Unique name {0} generated for {1}", newName, memberId);

            filenames.Add(newName);

            return newName;
        }
        #endregion
    }
}
