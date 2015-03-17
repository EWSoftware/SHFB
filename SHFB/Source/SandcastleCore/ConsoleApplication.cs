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
using System.Xml.XPath;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Sandcastle.Core
{
    /// <summary>
    /// This class contains various utility methods used by console applications
    /// </summary>
    public static class ConsoleApplication
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// The verbosity level for the <see cref="O:Microsoft.Ddue.Tools.CommandLine.WriteMessage"/>
        /// methods.
        /// </summary>
        /// <value>The default level is <see cref="LogLevel.Info"/> so that all messages are displayed.
        /// Setting it to a higher level will suppress messages below the given level.</value>
        public static LogLevel VerbosityLevel { get; set; }

        /// <summary>
        /// This is used to set the MSBuild log to use for all messages output via the message writing methods
        /// </summary>
        /// <value>If null, the messages are written to the console</value>
        public static TaskLoggingHelper Log { get; set; }

        /// <summary>
        /// This is used to set the tool name reported in warning and error messages when running under MSBuild
        /// </summary>
        public static string ToolName { get; set; }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Get a configuration file for the currently executing (the calling) assembly
        /// </summary>
        /// <returns>An XML document containing the configuration</returns>
        /// <remarks>This returns the configuration from the actual location of the assembly, where it was found
        /// versus where it is executing from, which may be different if shadow copied.</remarks>
        public static XPathDocument GetConfigurationFile()
        {
            string location = null;

            var assembly = Assembly.GetCallingAssembly();

            try
            {
                Uri codeBase = new Uri(assembly.CodeBase);

                if(codeBase.IsFile)
                    location = codeBase.LocalPath;
                else
                    location = assembly.Location;
            }
            catch
            {
                // Ignore errors.  If there are any, just return the Location value.
                if(assembly != null)
                    location = assembly.Location;
            }

            return GetConfigurationFile(location + ".config");
        }

        /// <summary>
        /// Load the specified configuration file
        /// </summary>
        /// <returns>An XML document containing the configuration</returns>
        public static XPathDocument GetConfigurationFile(string file)
        {
            return new XPathDocument(file);
        }

        /// <summary>
        /// Write the name, version, and copyright information of the calling assembly
        /// </summary>
        public static void WriteBanner()
        {
            Assembly application = Assembly.GetCallingAssembly();
            AssemblyName applicationData = application.GetName();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(application.Location);
            
            if(Log != null)
                Log.LogMessage("{0} (v{1})", applicationData.Name, fvi.ProductVersion);
            else
                Console.WriteLine("{0} (v{1})", applicationData.Name, fvi.ProductVersion);

            Object[] copyrightAttributes = application.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);

            foreach(AssemblyCopyrightAttribute copyrightAttribute in copyrightAttributes)
                if(Log != null)
                    Log.LogMessage(copyrightAttribute.Copyright);
                else
                    Console.WriteLine(copyrightAttribute.Copyright);
        }

        /// <summary>
        /// Write a message to the console
        /// </summary>
        /// <param name="level">The log level of the message</param>
        /// <param name="message">The message string to display</param>
        public static void WriteMessage(LogLevel level, string message)
        {
            if(level >= VerbosityLevel)
                if(Log != null)
                    LogMessage(level, message);
                else
                    Console.WriteLine("{0}: {1}", level, message);
        }

        /// <summary>
        /// Write a formatted message to the console with the given parameters
        /// </summary>
        /// <param name="level">The log level of the message</param>
        /// <param name="format">The message format string</param>
        /// <param name="args">The list of arguments to format into the message</param>
        public static void WriteMessage(LogLevel level, string format, params object[] args)
        {
            if(level >= VerbosityLevel)
                if(Log != null)
                    LogMessage(level, format, args);
                else
                {
                    Console.Write("{0}: ", level);
                    Console.WriteLine(format, args);
                }
        }

        /// <summary>
        /// Write a formatted message to the MSBuild logger with the given parameters
        /// </summary>
        /// <param name="level">The log level of the message</param>
        /// <param name="format">The message format string</param>
        /// <param name="args">The list of arguments to format into the message</param>
        private static void LogMessage(LogLevel level, string format, params object[] args)
        {
            switch(level)
            {
                case LogLevel.Diagnostic:
                    Log.LogMessage(MessageImportance.High, format, args);
                    break;

                case LogLevel.Warn:
                    Log.LogWarning(null, null, null, ToolName, 0, 0, 0, 0, format, args);
                    break;

                case LogLevel.Error:
                    Log.LogError(null, null, null, ToolName, 0, 0, 0, 0, format, args);
                    break;

                default:     // Info or unknown level
                    Log.LogMessage(format, args);
                    break;
            }
        }
        #endregion
    }
}
