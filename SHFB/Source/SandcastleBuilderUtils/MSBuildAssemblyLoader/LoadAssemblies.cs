//=============================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : LoadAssemblies.cs
// Author  : J. Ritchie Carroll (ritchiecarroll@gmail.com)
// Updated : 01/06/2020
// Note    : Copyright 2008-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// Pre-loads all MSBuild task assemblies in current folder
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: https://GitHub.com/EWSoftware/SHFB.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  01/16/2020  JRC  Created the code
// ============================================================================

using System;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuildAssemblyLoader
{
    /// <summary>
    /// Task to attempt pre-load of MSBuild assemblies.
    /// </summary>
    public class LoadAssemblies : Task
    {
        /// <summary>Must be implemented by derived class.</summary>
        /// <returns>true, if successful</returns>
        public override bool Execute()
        {
            string location = Path.GetDirectoryName(typeof(LoadAssemblies).Assembly.Location);
            string[] assemblies = Directory.GetFiles(location, "*.dll");

            foreach(string assembly in assemblies)
            {
                try
                {
                    Log.LogMessage(MessageImportance.Normal, $"Attempting to load \"{assembly}\"...");
                    Assembly.LoadFrom(assembly);
                    Log.LogMessage(MessageImportance.Normal, "Load success.");
                }
                catch(Exception ex)
                {
                    Log.LogMessage(MessageImportance.Normal, $"Load failed: {ex.Message}");
                }
            }

            return true;
        }
    }
}
