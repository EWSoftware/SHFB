//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ComponentAssemblyResolver.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/09/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains a class used to resolve assembly dependencies when loading component assemblies with MEF
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/09/2021  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sandcastle.Core
{
    /// <summary>
    /// This is used to resolve assembly dependencies when loading component assemblies with MEF
    /// </summary>
    public sealed class ComponentAssemblyResolver : IDisposable
    {
        #region Private data members
        //=====================================================================

        private readonly HashSet<string> componentFolders;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ComponentAssemblyResolver()
        {
            // This will be used to track folders containing components so that we can search them for dependency
            // assemblies later on.  We'll search the tools folder as well.
            componentFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            };

            // When ran as an MSBuild task, it won't always find dependent assemblies so we must find them
            // manually.
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the resolver if not done explicitly
        /// with <see cref="Dispose()"/>.
        /// </summary>
        ~ComponentAssemblyResolver()
        {
            this.Dispose();
        }

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of the resolver
        /// </summary>
        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Add a folder to check for dependencies
        /// </summary>
        /// <param name="folder">The folder to check</param>
        public void AddFolder(string folder)
        {
            componentFolders.Add(folder);
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This is handled to resolve dependent assemblies and load them when necessary
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="args">The event arguments</param>
        /// <returns>The loaded assembly or null if not found</returns>
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string[] nameInfo = args.Name.Split(new char[] { ',' });
            string resolveName = nameInfo[0];

            // See if it has already been loaded
            Assembly asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);

            // If not already loaded, check for dependency assemblies in the component and build tasks folders
            if(asm == null)
            {
                foreach(string folder in componentFolders)
                {
                    try
                    {
                        string foundAssembly = Directory.EnumerateFiles(folder, "*.dll").FirstOrDefault(
                            f => resolveName.Equals(Path.GetFileNameWithoutExtension(f), StringComparison.OrdinalIgnoreCase));

                        if(foundAssembly != null)
                        {
                            asm = Assembly.LoadFile(foundAssembly);
                            break;
                        }
                    }
                    catch(Exception ex)
                    {
                        // Just ignore any exceptions here
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                }
            }

            return asm;
        }
        #endregion
    }
}
