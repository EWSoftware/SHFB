//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : SdkVersion.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/12/2025
// Note    : Copyright 2012-2025, Eric Woodruff, All rights reserved
//
// This file contains a class used to parse SDK version numbers
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/12/2025  EFW  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Core.Reflection;

/// <summary>
/// This class is used to parse SDK version numbers
/// </summary>
/// <remarks>It separates the version from any supplemental information following it in order to obtain
/// a <see cref="Version"/> instance that can be used to properly sort the SDK versions.</remarks>
internal sealed class SdkVersion
{
    #region Properties
    //=====================================================================

    /// <summary>
    /// This is used to get the SDK version number
    /// </summary>
    public Version Version { get; }

    /// <summary>
    /// This is used to get any supplemental version information
    /// </summary>
    public string SupplementalVersionInfo { get; }

    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="version">The version number to parse</param>
    public SdkVersion(string version)
    {
        if(String.IsNullOrWhiteSpace(version))
        {
            this.Version = new Version();
            this.SupplementalVersionInfo = String.Empty;
        }
        else
        {
            int pos = 0;

            // Separate any supplemental information from the version number (i.e. 10.0.0-rc.1.25451.107)
            while(pos < version.Length && (Char.IsDigit(version[pos]) || version[pos] == '.'))
                pos++;

            if(pos < version.Length)
            {
                if(Version.TryParse(version.Substring(0, pos), out Version v))
                    this.Version = v;
                else
                {
                    this.Version = new Version();
                    pos = 0;
                }

                this.SupplementalVersionInfo = version.Substring(pos);
            }
            else
            {
                if(Version.TryParse(version, out Version v))
                {
                    this.Version = v;
                    this.SupplementalVersionInfo = String.Empty;
                }
                else
                {
                    this.Version = new Version();
                    this.SupplementalVersionInfo = version;
                }
            }
        }
    }
    #endregion
}
