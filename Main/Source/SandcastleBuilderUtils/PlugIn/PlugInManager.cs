//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : PlugInManager.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/09/2011
// Note    : Copyright 2007-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that manages the set of known plug-ins.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.2.0  09/09/2007  EFW  Created the code
// 1.6.0.2  11/06/2007  EFW  Added new component config merge build step
// 1.8.0.0  10/06/2008  EFW  Changed the default location of custom plug-ins
// 1.8.0.3  07/04/2009  EFW  Merged build component and plug-in folder
//=============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

// All classes go in the SandcastleBuilder.Utils.PlugIn namespace
namespace SandcastleBuilder.Utils.PlugIn
{
    /// <summary>
    /// This class is used to manage the set of known plug-ins.
    /// </summary>
    public static class PlugInManager
    {
        #region Private data members
        //=====================================================================
        // Private data members

        private static Dictionary<string, PlugInInfo> plugIns;
        #endregion

        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// This returns a dictionary containing the loaded plug-ins.
        /// </summary>
        /// <value>The dictionary keys are the plug-in names.</value>
        public static Dictionary<string, PlugInInfo> PlugIns
        {
            get
            {
                if(plugIns == null || plugIns.Count == 0)
                    LoadPlugIns();

                return plugIns;
            }
        }
        #endregion

        /// <summary>
        /// Load the build plug-ins found in the Components and Plug-Ins
        /// folder and its subfolders.
        /// </summary>
        private static void LoadPlugIns()
        {
            List<string> allFiles = new List<string>();
            Assembly asm;
            Type[] types;
            PlugInInfo info;
            string shfbFolder, plugInsFolder, componentPath;

            plugIns = new Dictionary<string, PlugInInfo>();
            asm = Assembly.GetExecutingAssembly();

            shfbFolder = asm.Location;
            shfbFolder = shfbFolder.Substring(0, shfbFolder.LastIndexOf('\\') + 1);
            plugInsFolder = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData),
                Constants.ComponentsAndPlugInsFolder);

            // Give precedence to plug-ins in the optional SHFBCOMPONENTROOT
            // environment variable folder.
            componentPath = Environment.ExpandEnvironmentVariables("%SHFBCOMPONENTROOT%");

            if(!String.IsNullOrEmpty(componentPath) && Directory.Exists(componentPath))
                allFiles.AddRange(Directory.EnumerateFiles(componentPath, "*.plugins",
                    SearchOption.AllDirectories));

            // Add the standard plug-ins file and any third-party plug-in
            // files in the installation folder too.  This allows for XCOPY
            // deployments of SHFB to build servers.
            allFiles.AddRange(Directory.EnumerateFiles(shfbFolder, "*.plugins",
                SearchOption.AllDirectories));

            // Finally, check the common app data build components folder
            if(Directory.Exists(plugInsFolder))
                allFiles.AddRange(Directory.EnumerateFiles(plugInsFolder, "*.plugins",
                    SearchOption.AllDirectories));

            foreach(string file in allFiles)
            {
                // Any exceptions that occur during the loading of a plug-in
                // will be handled by the caller.
                asm = Assembly.LoadFrom(file);
                types = asm.GetTypes();

                foreach(Type t in types)
                    if(t.IsPublic && !t.IsAbstract && typeof(IPlugIn).IsAssignableFrom(t))
                    {
                        info = new PlugInInfo(t);

                        if(!plugIns.ContainsKey(info.Name))
                            plugIns.Add(info.Name, info);
                    }
            }
        }

        /// <summary>
        /// This is used to determine if a plug-in exists with the specified
        /// key and can be used.
        /// </summary>
        /// <param name="key">The key for the plug-in</param>
        /// <returns>True if the plug-in exits and can be used.</returns>
        public static bool IsSupported(string key)
        {
            PlugInInfo info;

            // Does it exist?
            if(!PlugInManager.PlugIns.TryGetValue(key, out info))
                return false;

            return true;
        }
    }
}
