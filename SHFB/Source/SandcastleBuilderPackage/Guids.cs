//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : Guids.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/28/2015
// Note    : Copyright 2011-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains various GUIDs for the package
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://github.com/EWSoftware/SHFB
// This notice, the author's name, and all copyright notices must remain intact in all applications,
// documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/18/2011  EFW  Created the code
// 12/26/2011  EFW  Added the GUIDs for the new file editors
//===============================================================================================================

using System;

namespace SandcastleBuilder.Package
{
    /// <summary>
    /// This class contains various GUID values for the package
    /// </summary>
    static partial class GuidList
    {
        /// <summary> Package GUID (string form) </summary>
        public const string guidSandcastleBuilderPackagePkgString = "c997d569-ee8e-4947-a46f-9a0717ce39a0";

        /// <summary> Command set GUID (string form) </summary>
        public const string guidSandcastleBuilderCommandSetString = "811682ce-64b4-4a7a-a298-4eb8093f96ba";

        /// <summary>Command set GUID</summary>
        public static readonly Guid guidSandcastleBuilderPackageCmdSet = new Guid(guidSandcastleBuilderCommandSetString);

        /// <summary>The package project factory GUID in string form</summary>
        public const string guidSandcastleBuilderProjectFactoryString = "7CF6DF6D-3B04-46f8-A40B-537D21BCA0B4";

        /// <summary>The package project factory GUID</summary>
        public static readonly Guid guidSandcastleBuilderProjectFactory = new Guid(guidSandcastleBuilderProjectFactoryString);

        /// <summary>Content layout editor factory GUID string</summary>
        public const string guidContentLayoutEditorFactoryString = "7AAD2922-72A2-42C1-A077-85F5097A8FA7";

        /// <summary>Resource item editor factory GUID string</summary>
        public const string guidResourceItemEditorFactoryString = "1C79180C-BB93-46D2-B4D3-F22E7015A6F1";

        /// <summary>Site map editor factory GUID string</summary>
        public const string guidSiteMapEditorFactoryString = "DED740F1-EB91-48E3-9A41-4E4942FE53C1";

        /// <summary>Token editor factory GUID string</summary>
        public const string guidTokenEditorFactoryString = "D481FB70-9BF0-4868-9D4C-5DB33C6565E1";
    };
}
