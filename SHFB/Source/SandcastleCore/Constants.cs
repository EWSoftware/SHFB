//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : Constants.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/04/2014
// Note    : Copyright 2006-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains various constants for the help file builder applications
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.0.0.0  08/02/2006  EFW  Created the code
// 1.8.0.0  10/05/2008  EFW  Moved to SandcastleBuilder.Utils and added some new items
// 1.8.0.3  07/04/2009  EFW  Merged build component and plug-in folder
// 1.9.6.0  11/13/2012  EFW  Added support for third-party presentation styles
// -------  01/02/2014  EFW  Moved the constants to Sandcastle.Core
//===============================================================================================================

using System;

namespace Sandcastle.Core
{
    /// <summary>
    /// This class holds a set of constants that define various application values such as the application name,
    /// common data folder locations, etc.
    /// </summary>
    public static class Constants
    {
        //=====================================================================

        /// <summary>The company name</summary>
        public const string CompanyName = "EWSoftware";

        /// <summary>The application name</summary>
        public const string AppName = "Sandcastle Help File Builder";

        /// <summary>The default presentation style</summary>
        public const string DefaultPresentationStyle = "VS2013";

        /// <summary>
        /// The application folder in which the component and user data files are stored.
        /// </summary>
        /// <remarks>This folder will be located under the <see cref="Environment.SpecialFolder">CommonApplicationData</see>
        /// or <see cref="Environment.SpecialFolder">LocalApplicationData</see> folder.</remarks>
        public const string ProgramDataFolder = CompanyName + "\\" + AppName;

        /// <summary>
        /// This folder is located under the <see cref="Environment.SpecialFolder">CommonApplicationData</see>
        /// folder and contains custom build components that can be added to a project.
        /// </summary>
        public const string ComponentsAndPlugInsFolder = ProgramDataFolder + "\\" + "Components and Plug-Ins";
    }
}
