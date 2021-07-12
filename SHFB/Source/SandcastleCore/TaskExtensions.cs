// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 02/16/2012 - EFW - Added support for setting a verbosity level.  Messages with a log level below
// the current verbosity level are ignored.
// 12/10/2013 - EFW - Added support for MSBuild logging
// 12/21/2013 - EFW - Moved class to Sandcastle.Core assembly
// 01/06/2015 - EFW - Updated WriteBanner() to use file version info to be consistent with the other tools

using System;
using System.Diagnostics;
using System.Reflection;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Sandcastle.Core
{
    /// <summary>
    /// This class contains extension methods used by the MSBuild tasks
    /// </summary>
    public static class TaskExtensions
    {
        #region Methods
        //=====================================================================

        /// <summary>
        /// Write the name, version, and copyright information of the calling task
        /// </summary>
        /// <param name="task">The task for which to write the banner message</param>
        public static void WriteBanner(this Task task)
        {
            if(task == null)
                throw new ArgumentNullException(nameof(task));

            Assembly application = Assembly.GetCallingAssembly();
            AssemblyName applicationData = application.GetName();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(application.Location);

            task.Log.LogMessage("{0} (v{1})", applicationData.Name, fvi.ProductVersion);

            object[] copyrightAttributes = application.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);

            foreach(AssemblyCopyrightAttribute copyrightAttribute in copyrightAttributes)
                task.Log.LogMessage(copyrightAttribute.Copyright);
        }

        /// <summary>
        /// Write a formatted message to the task log with the given parameters
        /// </summary>
        /// <param name="task">The task for which to write the message</param>
        /// <param name="level">The log level of the message</param>
        /// <param name="format">The message format string</param>
        /// <param name="args">The list of arguments to format into the message</param>
        public static void WriteMessage(this Task task, LogLevel level, string format, params object[] args)
        {
            if(task == null)
                throw new ArgumentNullException(nameof(task));

            switch(level)
            {
                case LogLevel.Diagnostic:
                    task.Log.LogMessage(MessageImportance.High, format, args);
                    break;

                case LogLevel.Warn:
                    task.Log.LogWarning(null, null, null, task.GetType().Name, 0, 0, 0, 0, format, args);
                    break;

                case LogLevel.Error:
                    task.Log.LogError(null, null, null, task.GetType().Name, 0, 0, 0, 0, format, args);
                    break;

                default:     // Info or unknown level
                    task.Log.LogMessage(format, args);
                    break;
            }
        }
        #endregion
    }
}
