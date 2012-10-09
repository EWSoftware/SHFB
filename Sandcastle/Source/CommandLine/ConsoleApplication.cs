// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 02/16/2012 - EFW - Added support for setting a verbosity level.  Messages with a log level below
// the current verbosity level are ignored.

using System;
using System.IO;
using System.Reflection;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools.CommandLine
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
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Get a configuration file for the currently executing assembly.
        /// </summary>
        /// <returns>An XML document containing the configuration</returns>
        public static XPathDocument GetConfigurationFile()
        {
            return GetConfigurationFile(Assembly.GetCallingAssembly().Location + ".config");
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
        /// This doesn't appear to be used anywhere and it's probably more efficient to use
        /// <see cref="Directory.EnumerateFiles"/> instead anyway.
        /// </summary>
        /// <param name="filePattern">The file pattern to use</param>
        /// <returns>An array of files matching the pattern</returns>
        [Obsolete("Use Directory.EnumerateFiles instead")]
        public static string[] GetFiles(string filePattern)
        {
            // get the full path to the relevent directory
            string directoryPath = Path.GetDirectoryName(filePattern);

            if((directoryPath == null) || (directoryPath.Length == 0))
                directoryPath = Environment.CurrentDirectory;

            directoryPath = Path.GetFullPath(directoryPath);

            // get the file name, which may contain wildcards
            string filePath = Path.GetFileName(filePattern);

            // look up the files and load them
            string[] files = Directory.GetFiles(directoryPath, filePath);

            return files;
        }

        /// <summary>
        /// Write the name, version, and copyright information of the calling assembly
        /// </summary>
        public static void WriteBanner()
        {
            Assembly application = Assembly.GetCallingAssembly();
            AssemblyName applicationData = application.GetName();

            Console.WriteLine("{0} (v{1})", applicationData.Name, applicationData.Version);

            Object[] copyrightAttributes = application.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);

            foreach(AssemblyCopyrightAttribute copyrightAttribute in copyrightAttributes)
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
                Console.WriteLine("{0}: {1}", level, message);
        }

        /// <summary>
        /// Write a formatted message to the console with the given parameters
        /// </summary>
        /// <param name="level">The log level of the message</param>
        /// <param name="format">The message format string</param>
        /// <param name="arg">The list of arguments to format into the message</param>
        public static void WriteMessage(LogLevel level, string format, params object[] arg)
        {
            if(level >= VerbosityLevel)
            {
                Console.Write("{0}: ", level);
                Console.WriteLine(format, arg);
            }
        }
        #endregion
    }
}
