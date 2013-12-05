// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 09/14/2012 - EFW - Added support for the new platform element attributes that specify a framework type
// and version in order to correctly document other frameworks such as Silverlight and the Portable Library
// frameworks.  Added support for expanding environment variables in dependency and assembly name command line
// values.
// 11/20/2013 - EFW - Deprecated support for /internal command line option as the new visibility settings in the
// configuration file provide finer grained control over the members included in the output.  Merged code from
// Stazzz to implement namespace grouping support

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using System.Compiler;
using Microsoft.Ddue.Tools.CommandLine;
using Microsoft.Ddue.Tools.Reflection;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This parses assemblies and outputs an XML file containing reflection data about the API members
    /// </summary>
    public static class MRefBuilder
    {
        /// <summary>
        /// Main program entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Zero on success or non-zero on failure</returns>
        public static int Main(string[] args)
        {
            string path, version, framework, assemblyPath, typeName;

            // Write banner
            ConsoleApplication.WriteBanner();

            // Specify options
            OptionCollection options = new OptionCollection();
            options.Add(new SwitchOption("?", "Show this help page."));
            options.Add(new StringOption("out", "Specify an output file. If unspecified, output goes to the " +
                "console.", "outputFilePath"));
            options.Add(new StringOption("config", "Specify a configuration file. If unspecified, " +
                "MRefBuilder.config is used", "configFilePath"));
            options.Add(new ListOption("dep", "Specify assemblies to load for dependencies.",
                "dependencyAssembly"));
            options.Add(new BooleanOption("internal", "Specify whether to document internal as well as " +
                "externally exposed APIs.  *** DEPRECATED:  Use the visibility settings in the MRefBuilder.config " +
                "file instead which provide finer grained control over the exposed API members."));
            options.Add(new BooleanOption("groupNamespaces", "Specify whether to add special 'grouping' " +
                "namespaces to group logically nested namespaces.  The default is false."));
// TODO: Remove?            options.Add(new StringOption( "groupingNSSeparator", "Specify a string separating logically nested" +
//               " namespace components. The default is \"" + NSGrouping.NSGroupingAlgorithms.DEFAULT_GROUPING_SEPARATOR + "\"." ) );

            // Process options
            ParseArgumentsResult results = options.ParseArguments(args);

            if(results.Options["?"].IsPresent)
            {
                Console.WriteLine("MRefBuilder [options] assemblies");
                options.WriteOptionSummary(Console.Out);
                return 0;
            }

            // Check for invalid options
            if(!results.Success)
            {
                results.WriteParseErrors(Console.Out);
                return 1;
            }

            // Check for missing or extra assembly directories
            if(results.UnusedArguments.Count < 1)
            {
                Console.WriteLine("Specify at least one assembly to reflect.");
                return 1;
            }

            // Load the configuration file
            XPathDocument config;
            string configDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string configFile = Path.Combine(configDirectory, "MRefBuilder.config");

            if(results.Options["config"].IsPresent)
            {
                configFile = (string)results.Options["config"].Value;
                configDirectory = Path.GetDirectoryName(configFile);
            }

            try
            {
                config = new XPathDocument(configFile);
            }
            catch(IOException e)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, "An error occurred while attempting to read " +
                    "the configuration file '{0}'. The error message is: {1}", configFile, e.Message);
                return 1;
            }
            catch(UnauthorizedAccessException e)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, "An error occurred while attempting to read " +
                    "the configuration file '{0}'. The error message is: {1}", configFile, e.Message);
                return 1;
            }
            catch(XmlException e)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, "The configuration file '{0}' is not " +
                    "well-formed. The error message is: {1}", configFile, e.Message);
                return 1;
            }

            // Adjust the target platform
            XPathNodeIterator platformNodes = config.CreateNavigator().Select("/configuration/dduetools/platform");

            if(platformNodes.MoveNext())
            {
                XPathNavigator platformNode = platformNodes.Current;
                version = platformNode.GetAttribute("version", String.Empty);
                path = platformNode.GetAttribute("path", String.Empty);

                // !EFW - Added support for the new platform attributes and the framework XML file
                if(!String.IsNullOrEmpty(version) && !String.IsNullOrEmpty(path))
                {
                    // Set the framework using the legacy attributes.  If set properly, they will document
                    // other framework types but it uses the standard .NET assemblies which contain more
                    // classes and methods that are not relevant to the other frameworks.
                    path = Environment.ExpandEnvironmentVariables(path);
                
                    if(!Directory.Exists(path))
                    {
                        ConsoleApplication.WriteMessage(LogLevel.Error, "The specified target platform " +
                            "directory '{0}' does not exist.", path);
                        return 1;
                    }
                
                    if(version == "2.0")
                        TargetPlatform.SetToV2(path);
                    else
                        if(version == "1.1")
                            TargetPlatform.SetToV1_1(path);
                        else
                            if(version == "1.0")
                                TargetPlatform.SetToV1(path);
                            else
                            {
                                ConsoleApplication.WriteMessage(LogLevel.Error, "Unknown target platform " +
                                    "version '{0}'.", version);
                                return 1;
                            }
                }
                else
                {
                    // Use the new framework definition file
                    framework = platformNode.GetAttribute("framework", String.Empty);

                    if(!String.IsNullOrEmpty(framework) && !String.IsNullOrEmpty(version))
                        TargetPlatform.SetFrameworkInformation(framework, version);
                    else
                    {
                        ConsoleApplication.WriteMessage(LogLevel.Error, "Unknown target framework " +
                            "version '{0} {1}'.", framework, version);
                        return 1;
                    }
                }
            }

            // Create an API member namer
            ApiNamer namer = new OrcasNamer();
            XPathNavigator namerNode = config.CreateNavigator().SelectSingleNode("/configuration/dduetools/namer");

            if(namerNode != null)
            {
                assemblyPath = namerNode.GetAttribute("assembly", String.Empty);
                typeName = namerNode.GetAttribute("type", String.Empty);

                assemblyPath = Environment.ExpandEnvironmentVariables(assemblyPath);

                if(!Path.IsPathRooted(assemblyPath))
                    assemblyPath = Path.Combine(configDirectory, assemblyPath);

                try
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    namer = (ApiNamer)assembly.CreateInstance(typeName);

                    if(namer == null)
                    {
                        ConsoleApplication.WriteMessage(LogLevel.Error, "The type '{0}' was not found in the " +
                            "component assembly '{1}'.", typeName, assemblyPath);
                        return 1;
                    }
                }
                catch(IOException e)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "A file access error occurred while " +
                        "attempting to load the component assembly '{0}'. The error message is: {1}",
                        assemblyPath, e.Message);
                    return 1;
                }
                catch(UnauthorizedAccessException e)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "A file access error occurred while " +
                        "attempting to load the component assembly '{0}'. The error message is: {1}",
                        assemblyPath, e.Message);
                    return 1;
                }
                catch(BadImageFormatException)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "The component assembly '{0}' is not a " +
                        "valid managed assembly.", assemblyPath);
                    return 1;
                }
                catch(TypeLoadException)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "The type '{0}' was not found in the " +
                        "component assembly '{1}'.", typeName, assemblyPath);
                    return 1;
                }
                catch(MissingMethodException)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "No appropriate constructor exists for " +
                        "the type'{0}' in the component assembly '{1}'.", typeName, assemblyPath);
                    return 1;
                }
                catch(TargetInvocationException e)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "An error occurred while initializing the " +
                        "type '{0}' in the component assembly '{1}'. The error message and stack trace " +
                        "follows: {2}", typeName, assemblyPath, e.InnerException);
                    return 1;
                }
                catch(InvalidCastException)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "The type '{0}' in the component assembly " +
                        "'{1}' is not a component type.", typeName, assemblyPath);
                    return 1;
                }
            }

            // Create a resolver
            AssemblyResolver resolver = new AssemblyResolver();
            XPathNavigator resolverNode = config.CreateNavigator().SelectSingleNode("/configuration/dduetools/resolver");

            if(resolverNode != null)
            {
                assemblyPath = resolverNode.GetAttribute("assembly", String.Empty);
                typeName = resolverNode.GetAttribute("type", String.Empty);

                assemblyPath = Environment.ExpandEnvironmentVariables(assemblyPath);
                if(!Path.IsPathRooted(assemblyPath))
                    assemblyPath = Path.Combine(configDirectory, assemblyPath);

                try
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    resolver = (AssemblyResolver)assembly.CreateInstance(typeName, false, BindingFlags.Public |
                        BindingFlags.Instance, null, new object[1] { resolverNode }, null, null);

                    if(resolver == null)
                    {
                        ConsoleApplication.WriteMessage(LogLevel.Error, "The type '{0}' was not found in the " +
                            "component assembly '{1}'.", typeName, assemblyPath);
                        return 1;
                    }
                }
                catch(IOException e)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "A file access error occurred while " +
                        "attempting to load the component assembly '{0}'. The error message is: {1}",
                        assemblyPath, e.Message);
                    return 1;
                }
                catch(UnauthorizedAccessException e)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "A file access error occurred while " +
                        "attempting to load the component assembly '{0}'. The error message is: {1}",
                        assemblyPath, e.Message);
                    return 1;
                }
                catch(BadImageFormatException)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "The component assembly '{0}' is not a " +
                        "valid managed assembly.", assemblyPath);
                    return 1;
                }
                catch(TypeLoadException)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "The type '{0}' was not found in the " +
                        "component assembly '{1}'.", typeName, assemblyPath);
                    return 1;
                }
                catch(MissingMethodException)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "No appropriate constructor exists for " +
                        "the type'{0}' in the component assembly '{1}'.", typeName, assemblyPath);
                    return 1;
                }
                catch(TargetInvocationException e)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "An error occurred while initializing the " +
                        "type '{0}' in the component assembly '{1}'. The error message and stack trace " +
                        "follows: {2}", typeName, assemblyPath, e.InnerException);
                    return 1;
                }
                catch(InvalidCastException)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "The type '{0}' in the component assembly " +
                        "'{1}' is not a component type.", typeName, assemblyPath);
                    return 1;
                }
            }

            resolver.UnresolvedAssemblyReference += UnresolvedAssemblyReferenceHandler;

            // Get a text writer for output
            TextWriter output = Console.Out;

            if(results.Options["out"].IsPresent)
            {
                string file = (string)results.Options["out"].Value;

                try
                {
                    output = new StreamWriter(file, false, Encoding.UTF8);
                }
                catch(IOException e)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "An error occurred while attempting to " +
                        "create an output file. The error message is: {0}", e.Message);
                    return 1;
                }
                catch(UnauthorizedAccessException e)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "An error occurred while attempting to " +
                        "create an output file. The error message is: {0}", e.Message);
                    return 1;
                }
            }

            // Dependency directory
            string[] dependencies = new string[0];

            if(results.Options["dep"].IsPresent)
                dependencies = (string[])results.Options["dep"].Value;

            try
            {
                // Create a builder
                ManagedReflectionWriter builder = new ManagedReflectionWriter(output, namer, resolver,
                    new ApiFilter(config.CreateNavigator().SelectSingleNode("/configuration/dduetools")));

// TODO: Add                builder.GroupNamespaces = (results.Options["groupNamespaces"].IsPresent && (bool)results.Options["groupNamespaces"].Value);
// TODO: Remove?                builder.GroupingNamespaceSeparater = (results.Options["groupingNSSeparator"].IsPresent ?
//                    (string)results.Options["groupingNSSeparator"].Value : null);

                // If the deprecated /internal+ option is used, expose everything via the filter to mimic the
                // behavior of prior versions.
                if(results.Options["internal"].IsPresent && (bool)results.Options["internal"].Value)
                    builder.ApiFilter.IncludeAttributes =
                        builder.ApiFilter.IncludeExplicitInterfaceImplementations =
                        builder.ApiFilter.IncludePrivates =
                        builder.ApiFilter.IncludePrivateFields =
                        builder.ApiFilter.IncludeInternals =
                        builder.ApiFilter.IncludeProtected =
                        builder.ApiFilter.IncludeSealedProtected =
                        builder.ApiFilter.IncludeInheritedMembers =
                        builder.ApiFilter.IncludeInheritedFrameworkMembers =
                        builder.ApiFilter.IncludeInheritedFrameworkPrivateMembers =
                        builder.ApiFilter.IncludeInheritedFrameworkInternalMembers =
                        builder.ApiFilter.IncludeNoPIATypes = true;

                // Register add-ins to the builder
                XPathNodeIterator addinNodes = config.CreateNavigator().Select("/configuration/dduetools/addins/addin");

                foreach(XPathNavigator addinNode in addinNodes)
                {
                    assemblyPath = addinNode.GetAttribute("assembly", String.Empty);
                    typeName = addinNode.GetAttribute("type", String.Empty);

                    assemblyPath = Environment.ExpandEnvironmentVariables(assemblyPath);

                    if(!Path.IsPathRooted(assemblyPath))
                        assemblyPath = Path.Combine(configDirectory, assemblyPath);

                    try
                    {
                        Assembly assembly = Assembly.LoadFrom(assemblyPath);
                        MRefBuilderAddIn addin = (MRefBuilderAddIn)assembly.CreateInstance(typeName, false,
                            BindingFlags.Public | BindingFlags.Instance, null,
                            new object[2] { builder, addinNode }, null, null);

                        if(addin == null)
                        {
                            ConsoleApplication.WriteMessage(LogLevel.Error, "The type '{0}' was not found in " +
                                "the add-in assembly '{1}'.", typeName, assemblyPath);
                            return 1;
                        }
                    }
                    catch(IOException e)
                    {
                        ConsoleApplication.WriteMessage(LogLevel.Error, "A file access error occurred while " +
                            "attempting to load the add-in assembly '{0}'. The error message is: {1}",
                            assemblyPath, e.Message);
                        return 1;
                    }
                    catch(BadImageFormatException)
                    {
                        ConsoleApplication.WriteMessage(LogLevel.Error, "The add-in assembly '{0}' is not a " +
                            "valid managed assembly.", assemblyPath);
                        return 1;
                    }
                    catch(TypeLoadException)
                    {
                        ConsoleApplication.WriteMessage(LogLevel.Error, "The type '{0}' was not found in the " +
                            "add-in assembly '{1}'.", typeName, assemblyPath);
                        return 1;
                    }
                    catch(MissingMethodException)
                    {
                        ConsoleApplication.WriteMessage(LogLevel.Error, "No appropriate constructor exists " +
                            "for the type '{0}' in the add-in assembly '{1}'.", typeName, assemblyPath);
                        return 1;
                    }
                    catch(TargetInvocationException e)
                    {
                        ConsoleApplication.WriteMessage(LogLevel.Error, "An error occurred while initializing " +
                            "the type '{0}' in the add-in assembly '{1}'. The error message and stack trace " +
                            "follows: {2}", typeName, assemblyPath, e.InnerException);
                        return 1;
                    }
                    catch(InvalidCastException)
                    {
                        ConsoleApplication.WriteMessage(LogLevel.Error, "The type '{0}' in the add-in " +
                            "assembly '{1}' is not an MRefBuilderAddIn type.", typeName, assemblyPath);
                        return 1;
                    }
                }

                try
                {
                    // Load dependencies
                    foreach(string dependency in dependencies)
                    {
                        try
                        {
                            // Expand environment variables
                            path = Environment.ExpandEnvironmentVariables(dependency);

                            // If x86 but it didn't exist, assume it's a 32-bit system and change the name
                            if(path.IndexOf("%ProgramFiles(x86)%", StringComparison.Ordinal) != -1)
                                path = Environment.ExpandEnvironmentVariables(path.Replace("(x86)", String.Empty));

                            builder.LoadAccessoryAssemblies(path);
                        }
                        catch(IOException e)
                        {
                            ConsoleApplication.WriteMessage(LogLevel.Error, "An error occurred while loading " +
                                "dependency assemblies. The error message is: {0}", e.Message);
                            return 1;
                        }
                    }

                    // Parse the assemblies
                    foreach(string dllPath in results.UnusedArguments)
                    {
                        try
                        {
                            // Expand environment variables
                            path = Environment.ExpandEnvironmentVariables(dllPath);

                            // If x86 but it didn't exist, assume it's a 32-bit system and change the name
                            if(path.IndexOf("%ProgramFiles(x86)%", StringComparison.Ordinal) != -1)
                                path = Environment.ExpandEnvironmentVariables(path.Replace("(x86)", String.Empty));

                            builder.LoadAssemblies(path);
                        }
                        catch(IOException e)
                        {
                            ConsoleApplication.WriteMessage(LogLevel.Error, "An error occurred while loading " +
                                "assemblies for reflection. The error message is: {0}", e.Message);
                            return 1;
                        }
                    }

                    ConsoleApplication.WriteMessage(LogLevel.Info, "Loaded {0} assemblies for reflection and " +
                        "{1} dependency assemblies.", builder.Assemblies.Count(),
                        builder.AccessoryAssemblies.Count());

                    builder.VisitApis();

                    ConsoleApplication.WriteMessage(LogLevel.Info, "Wrote information on {0} namespaces, " +
                        "{1} types, and {2} members", builder.Namespaces.Count(), builder.Types.Count(),
                        builder.Members.Count());
                }
                finally
                {
                    builder.Dispose();
                }
            }
            finally
            {
                if(results.Options["out"].IsPresent)
                    output.Close();
            }

            return 0;
        }

        /// <summary>
        /// This is used to report unresolved assembly information
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>An unresolved assembly reference will terminate the application</remarks>
        private static void UnresolvedAssemblyReferenceHandler(object sender, AssemblyReferenceEventArgs e)
        {
            ConsoleApplication.WriteMessage(LogLevel.Error, "Unresolved assembly reference: {0} ({1}) " +
                "required by {2}", e.Reference.Name, e.Reference.StrongName, e.Referrer.Name);

            // If in the debugger, break so that we can see what was missed
            if(System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();

            Environment.Exit(1);
        }
    }
}
