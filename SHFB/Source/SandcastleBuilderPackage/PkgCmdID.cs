//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : PkgCmdID.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/24/2015
// Note    : Copyright 2013-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains various command IDs for the package
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://github.com/EWSoftware/SHFB
// This notice, the author's name, and all copyright notices must remain intact in all applications,
// documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/20/2013  EFW  Created the code
//===============================================================================================================

namespace SandcastleBuilder.Package
{
    /// <summary>
    /// This class defines the command IDs for the package
    /// </summary>
    static class PkgCmdIDList
    {
        /// <summary>Add documentation source</summary>
		public const uint AddDocSource = 0x0001;
        /// <summary>View help file output</summary>
        public const uint ViewHelpFile = 0x0006;
        /// <summary>View HTML Help output</summary>
        public const uint ViewHtmlHelp = 0x0007;
        /// <summary>View Help 2 output</summary>
        public const uint ViewHxSHelp = 0x0008;
        /// <summary>View MS Help View output</summary>
        public const uint ViewMshcHelp = 0x0009;
        /// <summary>View ASP.NET website output (development web server)</summary>
        public const uint ViewAspNetWebsite = 0x000B;
        /// <summary>View HTML web site output (file system)</summary>
        public const uint ViewHtmlWebsite = 0x000C;
        /// <summary>View FAQ topic in help file</summary>
        public const uint ViewFaq = 0x0019;
        /// <summary>View SHFB help file</summary>
        public const uint ViewShfbHelp = 0x001A;
        /// <summary>Open project in the standalone GUI</summary>
        public const uint OpenInStandaloneGUI = 0x001C;
        /// <summary>View the build log</summary>
        public const uint ViewBuildLog = 0x0020;
        /// <summary>Open the Entity References tool window</summary>
        public const uint EntityReferencesWindow = 0x002A;
        /// <summary>Open the Topic Previewer tool window</summary>
        public const uint TopicPreviewerWindow = 0x0030;
        /// <summary>HTML encode selection</summary>
        public const uint HtmlEncode = 0x0032;
        /// <summary>View Open XML help output</summary>
        public const uint ViewDocxHelp = 0x0038;
    };
}
