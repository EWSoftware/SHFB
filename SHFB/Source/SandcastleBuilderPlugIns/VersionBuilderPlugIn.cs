//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : VersionBuilderPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/10/2022
// Note    : Copyright 2007-2022, Eric Woodruff, All rights reserved
//
// This file contains a plug-in designed to generate version information for assemblies in the current project
// and others related to the same product that can be merged into the current project's help file topics.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/01/2007  EFW  Created the code
// 08/13/2008  EFW  Updated to support the new project format
// 06/27/2010  EFW  Added support for /rip option
// 12/17/2013  EFW  Updated to use MEF for the plug-ins
// 12/28/2013  EFW  Updated to run VersionBuilder tool as an MSBuild task in TransformManifest.proj
// 06/16/2021  EFW  Merged the version builder code into the plug-in and removed the MSBuild task
//===============================================================================================================

// Ignore Spelling: Stazzz

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is designed to generate version information for assemblies in the current project and
    /// others related to the same product that can be merged into the current project's help file topics.
    /// </summary>
    [HelpFileBuilderPlugInExport("Version Builder", RunsInPartialBuild = true,
      Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
      Description = "This plug-in is used to generate version information for the current project and others " +
        "related to the same product and merge that information into a single help file for all of them.")]
    public sealed class VersionBuilderPlugIn : IPlugIn, IProgress<BuildProgressEventArgs>
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;

        private BuildProcess builder;
        private BuildStep lastBuildStep;

        // Plug-in configuration options
        private VersionSettings currentVersion;
        private List<VersionSettings> allVersions;
        private List<string> uniqueLabels;
        private bool ripOldApis;

        #endregion

        #region IPlugIn implementation
        //=====================================================================

        /// <summary>
        /// This read-only property returns a collection of execution points that define when the plug-in should
        /// be invoked during the build process.
        /// </summary>
        public IEnumerable<ExecutionPoint> ExecutionPoints
        {
            get
            {
                if(executionPoints == null)
                    executionPoints = new List<ExecutionPoint>
                    {
                        new ExecutionPoint(BuildStep.ApplyDocumentModel, ExecutionBehaviors.Before),
                        new ExecutionPoint(BuildStep.BuildTopics, ExecutionBehaviors.Before)
                    };

                return executionPoints;
            }
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the build process
        /// </summary>
        /// <param name="buildProcess">A reference to the current build process</param>
        /// <param name="configuration">The configuration data that the plug-in should use to initialize itself</param>
        /// <exception cref="BuilderException">This is thrown if the plug-in configuration is not valid</exception>
        public void Initialize(BuildProcess buildProcess, XElement configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            builder = buildProcess ?? throw new ArgumentNullException(nameof(buildProcess));
            allVersions = new List<VersionSettings>();
            uniqueLabels = new List<string>();

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            if(configuration.IsEmpty)
                throw new BuilderException("VBP0001", "The Version Builder plug-in has not been configured yet");

            // Add an element for the current project.  This one won't have a project to build.
            currentVersion = new VersionSettings(buildProcess.CurrentProject);
            allVersions.Add(currentVersion);

            var node = configuration.Element("currentProject");

            if(node != null)
            {
                currentVersion.FrameworkLabel = node.Attribute("label").Value;
                currentVersion.Version = node.Attribute("version").Value;

                // This wasn't in older versions
                ripOldApis = (bool?)node.Attribute("ripOldApis") ?? false;
            }

            foreach(var v in configuration.Descendants("version"))
                allVersions.Add(VersionSettings.FromXml(builder.CurrentProject, v));

            // An empty label messes up the HTML so use a single space if it's blank
            if(String.IsNullOrEmpty(currentVersion.FrameworkLabel))
                currentVersion.FrameworkLabel = " ";

            if(node == null)
                throw new BuilderException("VBP0002", "A version value is required for the Version Builder plug-in");

            if(allVersions.Count == 1)
                builder.ReportProgress("No other version information was supplied.  Only version information " +
                    "for the documented assemblies will be included.");

            foreach(VersionSettings vs in allVersions)
                if(!uniqueLabels.Contains(vs.FrameworkLabel))
                    uniqueLabels.Add(vs.FrameworkLabel);

            uniqueLabels.Sort();
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            if(context == null)
                throw new ArgumentNullException(nameof(context));

            // Update shared content version items
            if(context.BuildStep == BuildStep.BuildTopics)
            {
                this.UpdateVersionItems();
                return;
            }

            currentVersion.ReflectionFilename = builder.ReflectionInfoFilename;

            // Sort the collection by framework label in ascending order and by version in descending order.  The
            // collection must be sorted in descending order by version in order for the version builder tool to
            // output the correct information.
            allVersions.Sort((x, y) =>
            {
                int result = String.Compare(x.FrameworkLabel, y.FrameworkLabel, StringComparison.Ordinal);

                if(result == 0)
                {
                    int posX = 0, posY = 0;

                    // First, compare only the version parts ignoring anything else (i.e. "1.2.3.4 SP1",
                    // ignores " SP1").
                    while(posX < x.Version.Length && (Char.IsNumber(x.Version[posX]) || x.Version[posX] == '.'))
                        posX++;

                    while(posY < y.Version.Length && (Char.IsNumber(y.Version[posY]) || y.Version[posY] == '.'))
                        posY++;

                    // If not valid versions, compare literally
                    if(posX != 0 && posY != 0 && Version.TryParse(x.Version.Substring(0, posX), out Version versionX) &&
                      Version.TryParse(y.Version.Substring(0, posY), out Version versionY))
                    {
                        result = versionY.CompareTo(versionX);

                        // If the versions match, then compare any remainder
                        if(result == 0)
                        {
                            result = String.Compare(x.Version.Substring(posX), y.Version.Substring(posY),
                                StringComparison.Ordinal) * -1;
                        }
                    }
                    else
                        result = String.Compare(x.Version, y.Version, StringComparison.Ordinal) * -1;
                }

                return result;
            });

            // Merge the version information
            builder.ReportProgress("\r\nPerforming partial builds on prior version projects");

            // Build each of the projects
            foreach(VersionSettings vs in allVersions)
            {
                // Not needed for current project
                if(vs != currentVersion)
                {
                    string workingPath;

                    using(SandcastleProject project = new SandcastleProject(vs.HelpFileProject, true, true))
                    {
                        // We'll use a working folder below the current project's working folder
                        workingPath = Path.Combine(builder.WorkingFolder, vs.HelpFileProject.GetHashCode().ToString("X",
                            CultureInfo.InvariantCulture));

                        bool success = this.BuildProject(project, workingPath);

                        // Switch back to the original folder for the current project
                        Directory.SetCurrentDirectory(builder.ProjectFolder);

                        if(!success)
                            throw new BuilderException("VBP0003", "Unable to build prior version project: " +
                                project.Filename);
                    }

                    // Save the reflection file location as we need it later
                    vs.ReflectionFilename = Path.Combine(workingPath, "reflection.org");
                }
            }

            // TODO: Use this information in memory rather than creating a configuration file
            // Create the Version Builder configuration and add the parameters to the transform project
            this.CreateVersionBuilderConfigurationFile();

            this.GenerateVersionInformation();
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of the plug-in object
        /// </summary>
        public void Dispose()
        {
            // Nothing to dispose of in this one
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Update the version information items in the shared builder content file
        /// </summary>
        /// <remarks>Remove the standard version information items from the shared content file as the version
        /// builder information will take its place in the topics.  New items are added for each version of the
        /// project defined in the configuration settings.</remarks>
        private void UpdateVersionItems()
        {
            XmlDocument sharedContent;
            XmlNode root, node;
            XmlAttribute attr;

            builder.ReportProgress("Adding version information shared content items from the plug-in settings");

            // Add them to the first resource items file in the working folder
            string sharedContentFilename = builder.PresentationStyle.ResourceItemFiles(builder.Language.Name).FirstOrDefault();

            sharedContentFilename = Path.Combine(builder.WorkingFolder, Path.GetFileName(sharedContentFilename));
            sharedContent = new XmlDocument();
            sharedContent.Load(sharedContentFilename);

            root = sharedContent.SelectSingleNode("content");

            // Add items for each framework label
            foreach(string label in uniqueLabels)
            {
                // We need to use a hash value as this ends up as an XML attribute name in the reflection
                // data file and this ensures it only contains valid characters.
                string hashValue = label.GetHashCode().ToString("X", CultureInfo.InvariantCulture);

                // Label item
                node = sharedContent.CreateElement("item");
                attr = sharedContent.CreateAttribute("id");
                attr.Value = "SHFB_VBPI_Lbl_" + hashValue;
                node.Attributes.Append(attr);

                // Empty strings mess up the HTML so use a single space if blank
                node.InnerText = String.IsNullOrEmpty(label) ? " " : label;
                root.AppendChild(node);

                // Framework menu labels
                node = sharedContent.CreateElement("item");
                attr = sharedContent.CreateAttribute("id");
                attr.Value = "memberFrameworksSHFB_VBPI_Lbl_" + hashValue;
                node.Attributes.Append(attr);
                node.InnerText = String.IsNullOrEmpty(label) ? " " : label;
                root.AppendChild(node);

                node = sharedContent.CreateElement("item");
                attr = sharedContent.CreateAttribute("id");
                attr.Value = "IncludeSHFB_VBPI_Lbl_" + hashValue + "Members";
                node.Attributes.Append(attr);
                node.InnerText = String.IsNullOrEmpty(label) ? " " : label;
                root.AppendChild(node);
            }

            // Write out a label for each framework and version
            foreach(VersionSettings vs in allVersions)
            {
                node = sharedContent.CreateElement("item");
                attr = sharedContent.CreateAttribute("id");

                // We need to use a hash value as this ends up as an XML attribute name in the reflection
                // data file and this ensures it only contains valid characters.
                attr.Value = "SHFB_VBPI_" + vs.UniqueId.ToString("X", CultureInfo.InvariantCulture);

                node.Attributes.Append(attr);
                node.InnerText = vs.Version;
                root.AppendChild(node);
            }

            sharedContent.Save(sharedContentFilename);
        }

        /// <summary>
        /// This is called to build a project
        /// </summary>
        /// <param name="project">The project to build</param>
        /// <param name="workingPath">The working path for the project</param>
        /// <returns>Returns true if successful, false if not</returns>
        private bool BuildProject(SandcastleProject project, string workingPath)
        {
            BuildProcess buildProcess;

            lastBuildStep = BuildStep.None;

            builder.ReportProgress("\r\nBuilding {0}", project.Filename);

            try
            {
                project.Configuration = builder.CurrentProject.Configuration;
                project.Platform = builder.CurrentProject.Platform;

                // For the plug-in, we'll override some project settings
                project.HtmlHelp1xCompilerPath = new FolderPath(builder.Help1CompilerFolder, true, project);
                project.WorkingPath = new FolderPath(workingPath, true, project);
                project.OutputPath = new FolderPath(Path.Combine(workingPath, @"..\PartialBuildLog\"), true, project);

                // If the current project has defined OutDir, pass it on to the sub-project.
                string outDir = builder.CurrentProject.MSBuildProject.GetProperty("OutDir").EvaluatedValue;

                if(!String.IsNullOrEmpty(outDir) && outDir != @".\")
                    project.MSBuildOutDir = outDir;

                buildProcess = new BuildProcess(project, PartialBuildType.GenerateReflectionInfo)
                {
                    ProgressReportProvider = this,
                    CancellationToken = builder.CancellationToken
                };

                // Since this is a plug-in, we'll run it synchronously rather than as a background task
                buildProcess.Build();

                lastBuildStep = buildProcess.CurrentBuildStep;

                // Add the list of the comments files in the other project to this build
                if(lastBuildStep == BuildStep.Completed)
                    foreach(XmlCommentsFile comments in buildProcess.CommentsFiles)
                        builder.CommentsFiles.Insert(0, comments);
            }
            catch(Exception ex)
            {
                throw new BuilderException("VBP0004", String.Format(CultureInfo.InvariantCulture,
                    "Fatal error, unable to compile project '{0}': {1}", project.Filename, ex.ToString()));
            }

            return (lastBuildStep == BuildStep.Completed);
        }

        /// <summary>
        /// This creates the Version Builder configuration file
        /// </summary>
        private void CreateVersionBuilderConfigurationFile()
        {
            StringBuilder config = new StringBuilder(4096);

            builder.ReportProgress("Creating Version Builder configuration file");

            config.Append("<versions>\r\n");

            // Write out a <versions> element for each unique label that contains info for each related version.
            // We also copy the reflection files to unique names as we will create a new reflection.org file that
            // contains everything.
            foreach(string label in uniqueLabels)
            {
                config.AppendFormat(CultureInfo.InvariantCulture, "  <versions name=\"SHFB_VBPI_Lbl_{0:X}\">\r\n",
                    label.GetHashCode());

                // Add info for each related version
                foreach(VersionSettings vs in allVersions)
                    if(vs.FrameworkLabel == label)
                    {
                        config.AppendFormat(CultureInfo.InvariantCulture,
                            "    <version name=\"SHFB_VBPI_{0:X}\" file=\"{0:X}.ver\" />\r\n", vs.UniqueId);

                        File.Copy(vs.ReflectionFilename, Path.Combine(builder.WorkingFolder,
                            String.Format(CultureInfo.InvariantCulture, "{0:X}.ver", vs.UniqueId)), true);
                    }

                config.Append("  </versions>\r\n");
            }

            config.Append("</versions>\r\n");

            // Save the file
            using(StreamWriter sw = new StreamWriter(Path.Combine(builder.WorkingFolder, "VersionBuilder.config")))
            {
                sw.Write(config.ToString());
            }
        }
        #endregion

        #region IProgress<BuildProgressEventArgs> Members
        //=====================================================================

        /// <summary>
        /// This is called by the build process to report build progress for the reference link projects
        /// </summary>
        /// <param name="value">The event arguments</param>
        /// <remarks>Since the build is synchronous in this plug-in, we need to implement the interface and
        /// report progress synchronously as well or the final few messages can get lost and it looks like the
        /// build failed.</remarks>
        public void Report(BuildProgressEventArgs value)
        {
            if(value != null && value.StepChanged)
                builder.ReportProgress(value.BuildStep.ToString());
        }
        #endregion

        #region Version builder implementation
        //=====================================================================

        // TODO: Clean this up by removing the MSBuild intermediate structures and files and just use the configuration directly

        internal class VersionInfo
        {
            // Properties
            public string File { get; }
            public string Group { get; }
            public string Name { get; }

            // Methods
            public VersionInfo(string name, string group, string file)
            {
                this.Name = name;
                this.Group = group;
                this.File = file;
            }
        }

        internal class ElementInfo
        {
            // Properties
            public XPathNavigator ElementNode { get; }

            public Dictionary<string, string> Versions { get; } = new Dictionary<string, string>();

            // Methods
            public ElementInfo(string versionGroup, string version, XPathNavigator elementNode)
            {
                this.Versions.Add(versionGroup, version);
                this.ElementNode = elementNode;
            }
        }

        private readonly Dictionary<string, Dictionary<String, XPathNavigator>> extensionMethods =
            new Dictionary<string, Dictionary<String, XPathNavigator>>();

        // Copyright © Microsoft Corporation.
        // This source file is subject to the Microsoft Permissive License.
        // See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
        // All other rights reserved.

        // Change history:
        // 10/12/2013 - Added changes from Stazzz to merge information about additional extension methods even
        // when the type and method are defined in different assemblies.

        /// <summary>
        /// Generate the version information
        /// </summary>
        private void GenerateVersionInformation()
        {
            // TODO: This needs a complete overhaul to add better variable names and comments about what it is
            // actually doing.  Also, can this be simplified in anyway to accomplish the same thing?
            using(var r = XmlReader.Create(Path.Combine(builder.WorkingFolder, "VersionBuilder.config"),
              new XmlReaderSettings()))
            {
                XPathDocument document = new XPathDocument(r);

                XPathNavigator navigator = document.CreateNavigator().SelectSingleNode("versions");
                XPathExpression expr = XPathExpression.Compile("string(ancestor::versions/@name)");
                List<VersionInfo> allVersions = new List<VersionInfo>();
                List<string> latestVersions = new List<string>();

                foreach(XPathNavigator navigator2 in document.CreateNavigator().Select("versions//version[@file]"))
                {
                    string group = (string)navigator2.Evaluate(expr);
                    string attribute = navigator2.GetAttribute("name", String.Empty);
                    string name = navigator2.GetAttribute("file", String.Empty);

                    name = Path.Combine(builder.WorkingFolder, name);
                    VersionInfo item = new VersionInfo(attribute, group, name);
                    allVersions.Add(item);
                }

                string str5 = String.Empty;

                foreach(VersionInfo info2 in allVersions)
                    if(info2.Group != str5)
                    {
                        latestVersions.Add(info2.Name);
                        str5 = info2.Group;
                    }

                builder.CancellationToken.ThrowIfCancellationRequested();

                XmlReaderSettings settings = new XmlReaderSettings
                {
                    IgnoreWhitespace = true
                };

                XmlWriterSettings settings2 = new XmlWriterSettings
                {
                    Indent = true
                };

                Dictionary<string, List<KeyValuePair<string, string>>> versionIndex = new Dictionary<string, List<KeyValuePair<string, string>>>();
                Dictionary<string, Dictionary<string, ElementInfo>> dictionary2 = new Dictionary<string, Dictionary<string, ElementInfo>>();
                XPathExpression expression2 = XPathExpression.Compile("string(/api/@id)");
                XPathExpression expression4 = XPathExpression.Compile("/api/elements/element");
                XPathExpression expression = XPathExpression.Compile("/api/attributes/attribute[type[@api='T:System.ObsoleteAttribute']]");
                XPathExpression extensionAttributeExpression = XPathExpression.Compile("/api/attributes/attribute[type[@api='T:System.Runtime.CompilerServices.ExtensionAttribute']]");
                XPathExpression extensionFirstParameterExpression = XPathExpression.Compile("/api/parameters/parameter[1]/*");
                XPathExpression specialization = XPathExpression.Compile("./specialization");
                XPathExpression templates = XPathExpression.Compile("./template[boolean(@index) and starts-with(@api, 'M:')]");
                XPathExpression skipFirstParam = XPathExpression.Compile("./parameter[position()>1]");
                XPathExpression expression6 = XPathExpression.Compile("boolean(argument[type[@api='T:System.Boolean'] and value[.='True']])");
                XPathExpression apiChild = XPathExpression.Compile("./api");

                foreach(VersionInfo info3 in allVersions)
                {
                    builder.CancellationToken.ThrowIfCancellationRequested();

                    builder.ReportProgress("Indexing version '{0}' using file '{1}'.", info3.Name, info3.File);

                    using(XmlReader reader = XmlReader.Create(info3.File, settings))
                    {
                        reader.MoveToContent();

                        while(reader.Read())
                        {
                            if((reader.NodeType == XmlNodeType.Element) && (reader.LocalName == "api"))
                            {
                                string str7 = String.Empty;
                                XmlReader reader2 = reader.ReadSubtree();
                                XPathNavigator navigator3 = new XPathDocument(reader2).CreateNavigator();

                                string key = (string)navigator3.Evaluate(expression2);

                                if(!versionIndex.TryGetValue(key, out List<KeyValuePair<string, string>> list3))
                                {
                                    list3 = new List<KeyValuePair<string, string>>();
                                    versionIndex.Add(key, list3);
                                }

                                if(!dictionary2.TryGetValue(key, out Dictionary<string, ElementInfo> dictionary3))
                                {
                                    dictionary3 = new Dictionary<string, ElementInfo>();
                                    dictionary2.Add(key, dictionary3);
                                }

                                foreach(XPathNavigator navigator4 in navigator3.Select(expression4))
                                {
                                    string str8 = navigator4.GetAttribute("api", String.Empty);

                                    if(!dictionary3.TryGetValue(str8, out ElementInfo info4))
                                    {
                                        XPathNavigator elementNode = null;

                                        if((navigator4.SelectSingleNode("*") != null) || (navigator4.SelectChildren(XPathNodeType.Attribute).Count > 1))
                                        {
                                            elementNode = navigator4;
                                        }

                                        info4 = new ElementInfo(info3.Group, info3.Name, elementNode);
                                        dictionary3.Add(str8, info4);
                                        continue;
                                    }
                                    if(!info4.Versions.ContainsKey(info3.Group))
                                    {
                                        info4.Versions.Add(info3.Group, info3.Name);
                                    }
                                }

                                XPathNavigator navigator6 = navigator3.SelectSingleNode(expression);

                                if(navigator6 != null)
                                {
                                    str7 = ((bool)navigator6.Evaluate(expression6)) ? "error" : "warning";
                                }

                                if(key.StartsWith("M:", StringComparison.Ordinal))
                                {
                                    // Only check for extension methods when this is actually a method in question
                                    var navigator7 = navigator3.SelectSingleNode(extensionAttributeExpression);

                                    if(navigator7 != null)
                                    {
                                        // Check first parameter
                                        var navigator8 = navigator3.SelectSingleNode(extensionFirstParameterExpression);

                                        if(navigator8 != null)
                                        {
                                            // Get type node
                                            var typeID = navigator8.GetAttribute("api", String.Empty);

                                            if(navigator8.LocalName == "type")
                                            {
                                                var specNode = navigator8.SelectSingleNode(specialization);
                                                if(specNode == null || specNode.SelectChildren(XPathNodeType.Element).Count == specNode.Select(templates).Count)
                                                {
                                                    // Either non-generic type or all type parameters are from within this method
                                                    if(!extensionMethods.TryGetValue(typeID, out Dictionary<string, XPathNavigator> extMethods))
                                                    {
                                                        extMethods = new Dictionary<String, XPathNavigator>();
                                                        extensionMethods.Add(typeID, extMethods);
                                                    }
                                                    if(!extMethods.ContainsKey(key))
                                                    {
                                                        extMethods.Add(key, navigator3.SelectSingleNode(apiChild));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // TODO: extension methods for generic parameters...
                                                // This was never implemented.  Is it needed?  Does this get hit?
                                            }
                                        }
                                    }
                                }

                                list3.Add(new KeyValuePair<string, string>(info3.Name, str7));
                                reader2.Close();
                            }
                        }
                    }
                }

                if(ripOldApis)
                    RemoveOldApis(versionIndex, latestVersions);

                builder.ReportProgress("Indexed {0} entities in {1} versions.", versionIndex.Count, allVersions.Count);

                using(var writer = XmlWriter.Create(builder.ReflectionInfoFilename, settings2))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("reflection");
                    writer.WriteStartElement("assemblies");
                    Dictionary<string, object> dictionary4 = new Dictionary<string, object>();

                    foreach(VersionInfo info5 in allVersions)
                    {
                        builder.CancellationToken.ThrowIfCancellationRequested();

                        using(XmlReader reader3 = XmlReader.Create(info5.File, settings))
                        {
                            reader3.MoveToContent();

                            while(reader3.Read())
                            {
                                if((reader3.NodeType == XmlNodeType.Element) && (reader3.LocalName == "assembly"))
                                {
                                    string str9 = reader3.GetAttribute("name");
                                    if(!dictionary4.ContainsKey(str9))
                                    {
                                        XmlReader reader4 = reader3.ReadSubtree();
                                        writer.WriteNode(reader4, false);
                                        reader4.Close();
                                        dictionary4.Add(str9, null);
                                    }
                                }
                            }
                        }
                    }

                    writer.WriteEndElement();
                    writer.WriteStartElement("apis");
                    var readElements = new HashSet<String>();

                    foreach(VersionInfo info6 in allVersions)
                    {
                        builder.CancellationToken.ThrowIfCancellationRequested();

                        using(XmlReader reader5 = XmlReader.Create(info6.File, settings))
                        {
                            reader5.MoveToContent();

                            while(reader5.Read())
                            {
                                if((reader5.NodeType == XmlNodeType.Element) && (reader5.LocalName == "api"))
                                {
                                    string str10 = reader5.GetAttribute("id");
                                    if(versionIndex.ContainsKey(str10))
                                    {
                                        List<KeyValuePair<string, string>> versions = versionIndex[str10];
                                        KeyValuePair<string, string> pair = versions[0];
                                        if(info6.Name == pair.Key)
                                        {
                                            writer.WriteStartElement("api");
                                            writer.WriteAttributeString("id", str10);
                                            XmlReader reader6 = reader5.ReadSubtree();
                                            reader6.MoveToContent();
                                            reader6.ReadStartElement();

                                            var hasExtensionMethods = extensionMethods.TryGetValue(str10, out Dictionary<string, XPathNavigator> eElems);

                                            if(hasExtensionMethods)
                                            {
                                                readElements.Clear();
                                                readElements.UnionWith(extensionMethods[str10].Keys);
                                            }
                                            while(!reader6.EOF)
                                            {
                                                if((reader6.NodeType == XmlNodeType.Element) && (reader6.LocalName == "elements"))
                                                {
                                                    Dictionary<string, ElementInfo> dictionary5 = dictionary2[str10];
                                                    Dictionary<string, object> dictionary6 = new Dictionary<string, object>();
                                                    writer.WriteStartElement("elements");
                                                    XmlReader reader7 = reader6.ReadSubtree();
                                                    foreach(XPathNavigator navigator8 in new XPathDocument(reader7).CreateNavigator().Select("elements/element"))
                                                    {
                                                        string str11 = navigator8.GetAttribute("api", String.Empty);
                                                        dictionary6[str11] = null;
                                                        writer.WriteStartElement("element");
                                                        writer.WriteAttributeString("api", str11);
                                                        if(hasExtensionMethods)
                                                        {
                                                            readElements.Remove(str11);
                                                        }
                                                        foreach(string str12 in dictionary5[str11].Versions.Keys)
                                                        {
                                                            writer.WriteAttributeString(str12, dictionary5[str11].Versions[str12]);
                                                        }
                                                        foreach(XPathNavigator navigator9 in navigator8.Select("@*"))
                                                        {
                                                            if(navigator9.LocalName != "api")
                                                            {
                                                                writer.WriteAttributeString(navigator9.LocalName, navigator9.Value);
                                                            }
                                                        }
                                                        foreach(XPathNavigator navigator10 in navigator8.Select("*"))
                                                        {
                                                            writer.WriteNode(navigator10, false);
                                                        }
                                                        writer.WriteEndElement();
                                                    }
                                                    reader7.Close();
                                                    if(dictionary6.Count != dictionary5.Count)
                                                    {
                                                        foreach(string str13 in dictionary5.Keys)
                                                        {
                                                            if(dictionary6.ContainsKey(str13) || (ripOldApis &&
                                                                !IsLatestElement(dictionary5[str13].Versions.Values, latestVersions)))
                                                            {
                                                                continue;
                                                            }

                                                            writer.WriteStartElement("element");
                                                            writer.WriteAttributeString("api", str13);
                                                            if(hasExtensionMethods)
                                                            {
                                                                readElements.Remove(str13);
                                                            }
                                                            foreach(string str14 in dictionary5[str13].Versions.Keys)
                                                            {
                                                                writer.WriteAttributeString(str14, dictionary5[str13].Versions[str14]);
                                                            }
                                                            if(dictionary5[str13].ElementNode != null)
                                                            {
                                                                foreach(XPathNavigator navigator11 in dictionary5[str13].ElementNode.Select("@*"))
                                                                {
                                                                    if(navigator11.LocalName != "api")
                                                                    {
                                                                        writer.WriteAttributeString(navigator11.LocalName, navigator11.Value);
                                                                    }
                                                                }
                                                                foreach(XPathNavigator navigator12 in dictionary5[str13].ElementNode.Select("*"))
                                                                {
                                                                    writer.WriteNode(navigator12, false);
                                                                }
                                                            }
                                                            writer.WriteEndElement();
                                                        }
                                                    }

                                                    if(hasExtensionMethods)
                                                    {
                                                        foreach(var eMethodID in readElements)
                                                        {
                                                            writer.WriteStartElement("element");
                                                            writer.WriteAttributeString("api", eMethodID);
                                                            writer.WriteAttributeString("source", "extension");
                                                            foreach(XPathNavigator extMember in eElems[eMethodID].SelectChildren(XPathNodeType.Element))
                                                            {
                                                                switch(extMember.LocalName)
                                                                {
                                                                    case "apidata":
                                                                        writer.WriteStartElement("apidata");
                                                                        foreach(XPathNavigator apidataAttr in extMember.Select("@*"))
                                                                        {
                                                                            writer.WriteAttributeString(apidataAttr.LocalName, apidataAttr.Value);
                                                                        }
                                                                        writer.WriteAttributeString("subsubgroup", "extension");
                                                                        foreach(XPathNavigator child in extMember.SelectChildren(XPathNodeType.All & ~XPathNodeType.Attribute))
                                                                        {
                                                                            writer.WriteNode(child, false);
                                                                        }
                                                                        writer.WriteEndElement();
                                                                        break;
                                                                    case "parameters":
                                                                        var noParamsWritten = true;
                                                                        foreach(XPathNavigator eParam in extMember.Select(skipFirstParam))
                                                                        {
                                                                            if(noParamsWritten)
                                                                            {
                                                                                writer.WriteStartElement("parameters");
                                                                                noParamsWritten = false;
                                                                            }
                                                                            writer.WriteNode(eParam, false);
                                                                        }
                                                                        if(!noParamsWritten)
                                                                        {
                                                                            writer.WriteEndElement();
                                                                        }
                                                                        break;
                                                                    case "memberdata":
                                                                        writer.WriteStartElement("memberdata");
                                                                        foreach(XPathNavigator mDataAttr in extMember.Select("@*"))
                                                                        {
                                                                            if(mDataAttr.LocalName != "static")
                                                                            {
                                                                                writer.WriteAttributeString(mDataAttr.LocalName, mDataAttr.Value);
                                                                            }
                                                                        }
                                                                        foreach(XPathNavigator child in extMember.SelectChildren(XPathNodeType.All & ~XPathNodeType.Attribute))
                                                                        {
                                                                            writer.WriteNode(child, false);
                                                                        }
                                                                        writer.WriteEndElement();
                                                                        break;
                                                                    case "attributes":
                                                                        break;
                                                                    default:
                                                                        writer.WriteNode(extMember, false);
                                                                        break;
                                                                }
                                                            }
                                                            writer.WriteEndElement();
                                                        }
                                                    }

                                                    writer.WriteEndElement();
                                                    reader6.Read();
                                                }
                                                else if(reader6.NodeType == XmlNodeType.Element)
                                                {
                                                    writer.WriteNode(reader6, false);
                                                }
                                                else
                                                {
                                                    reader6.Read();
                                                }
                                            }
                                            reader6.Close();
                                            writer.WriteStartElement("versions");
                                            foreach(XPathNavigator navigator13 in navigator.SelectChildren(XPathNodeType.Element))
                                            {
                                                WriteVersionTree(versions, navigator13, writer);
                                            }
                                            writer.WriteEndElement();
                                            writer.WriteEndElement();
                                        }
                                    }
                                }
                            }
                        }
                    }

                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
        }

        private static bool IsLatestElement(Dictionary<string, string>.ValueCollection versions, List<string> latestVersions)
        {
            foreach(string str in versions)
                if(latestVersions.Contains(str))
                    return true;

            return false;
        }

        private void RemoveOldApis(Dictionary<string, List<KeyValuePair<string, string>>> versionIndex, List<string> latestVersions)
        {
            foreach(string key in versionIndex.Keys.ToList())
            {
                var list = versionIndex[key];
                bool remove = true;

                foreach(KeyValuePair<string, string> pair in list)
                    if(latestVersions.Contains(pair.Key))
                    {
                        remove = false;
                        break;
                    }

                if(remove)
                {
                    versionIndex.Remove(key);

                    // Remove from extension methods too
                    foreach(string typeKey in extensionMethods.Keys.ToList())
                    {
                        var extMethods = extensionMethods[typeKey];

                        if(extMethods.ContainsKey(key))
                            extMethods.Remove(key);

                        if(extMethods.Count == 0)
                            extensionMethods.Remove(typeKey);
                    }
                }
            }
        }

        private void WriteVersionTree(List<KeyValuePair<string, string>> versions, XPathNavigator branch, XmlWriter writer)
        {
            string localName = branch.LocalName;
            string attribute = branch.GetAttribute("name", String.Empty);

            switch(localName)
            {
                case "versions":
                    writer.WriteStartElement("versions");

                    if(!String.IsNullOrEmpty(attribute))
                        writer.WriteAttributeString("name", attribute);

                    foreach(XPathNavigator navigator in branch.SelectChildren(XPathNodeType.Element))
                        WriteVersionTree(versions, navigator, writer);

                    writer.WriteEndElement();
                    return;

                case "version":
                    foreach(KeyValuePair<string, string> pair in versions)
                        if(pair.Key == attribute)
                        {
                            writer.WriteStartElement("version");
                            writer.WriteAttributeString("name", attribute);

                            if(!String.IsNullOrEmpty(pair.Value))
                                writer.WriteAttributeString("obsolete", pair.Value);

                            writer.WriteEndElement();
                        }

                    break;
            }
        }
        #endregion
    }
}
