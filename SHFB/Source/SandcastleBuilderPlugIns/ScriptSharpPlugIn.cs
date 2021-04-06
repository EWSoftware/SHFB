//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : ScriptSharpPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/03/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a plug-in designed to modify the reflection information file produced after running
// MRefBuilder on assemblies produced by the Script# compiler so that it is suitable for use in producing a help
// file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// =====================================================================================================
// 01/25/2008  EFW  Created the code
// 07/14/2008  EFW  Updated for use with MSBuild project format
// 12/04/2013  EFW  Updated for use with the new visibility settings in MRefBuilder.config.
// 12/17/2013  EFW  Updated to use MEF for the plug-ins
// 04/03/2021  EFW  Replaced FixScriptSharp.xsl with code in this plug-in that modifies the reflection.org file
//===============================================================================================================

// Ignore Spelling: Nikhil Kothari

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using Sandcastle.Core;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is designed to modify the reflection information file produced after running
    /// MRefBuilder on assemblies produced by the Script# compiler so that it is suitable for use in
    /// producing a help file.
    /// </summary>
    [HelpFileBuilderPlugInExport("Script# Reflection File Fixer", RunsInPartialBuild = true,
      Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright + "\r\nScript# is Copyright \xA9 " +
      "2007-2021 Nikhil Kothari, All Rights Reserved",
      Description = "This plug-in is used to modify the reflection information file produced after running " +
      "MRefBuilder on assemblies produced by the Script# compiler so that it is suitable for use in producing " +
      "a help file.")]
    public sealed class ScriptSharpPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;

        private BuildProcess builder;

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
                        new ExecutionPoint(BuildStep.GenerateReflectionInfo, ExecutionBehaviors.BeforeAndAfter)
                    };

                return executionPoints;
            }
        }

        /// <summary>
        /// This method is used by the Sandcastle Help File Builder to let the plug-in perform its own
        /// configuration.
        /// </summary>
        /// <param name="project">A reference to the active project</param>
        /// <param name="currentConfig">The current configuration XML fragment</param>
        /// <returns>A string containing the new configuration XML fragment</returns>
        /// <remarks>The configuration data will be stored in the help file builder project</remarks>
        public string ConfigurePlugIn(SandcastleProject project, string currentConfig)
        {
            MessageBox.Show("This plug-in has no configurable settings", "Script# Reflection File Fixer Plug-In",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            return currentConfig;
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the build process
        /// </summary>
        /// <param name="buildProcess">A reference to the current build process</param>
        /// <param name="configuration">The configuration data that the plug-in should use to initialize itself</param>
        public void Initialize(BuildProcess buildProcess, XPathNavigator configuration)
        {
            builder = buildProcess;

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            if(context.Behavior == ExecutionBehaviors.Before)
                this.ModifyMRefBuilderConfig();
            else
                this.FixScriptSharpReflectionData();
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
        /// This is used to modify the MRefBuilder.config file for use with Script#
        /// </summary>
        private bool ModifyMRefBuilderConfig()
        {
            XmlDocument config;
            XmlNode filter, type;
            XmlAttribute attr;
            string configFile;

            configFile = builder.WorkingFolder + "MRefBuilder.config";

            // If the configuration file doesn't exist we have nothing to do.  However, it could be that some
            // other plug-in has bypassed it so only issue a warning.
            if(!File.Exists(configFile))
            {
                builder.ReportWarning("SSP0001", "The MRefBuilder configuration file '{0}' could not be " +
                    "found.  The Script# plug-in did not run successfully.", configFile);
                return false;
            }

            builder.ReportProgress("Adding Script# attributes to MRefBuilder.config");

            // Add the required Script# attributes to the attribute filter
            config = new XmlDocument();
            config.Load(configFile);

            filter = config.SelectSingleNode("configuration/dduetools/attributeFilter/namespace[@name='System']");

            if(filter == null)
                throw new BuilderException("SSP0002", "Unable to locate attribute filter for the System " +
                    "namespace in the configuration file");

            type = config.CreateElement("type");
            attr = config.CreateAttribute("name");
            attr.Value = "AttachedPropertyAttribute";
            type.Attributes.Append(attr);
            attr = config.CreateAttribute("required");
            attr.Value = "true";
            type.Attributes.Append(attr);
            filter.AppendChild(type);

            type = config.CreateElement("type");
            attr = config.CreateAttribute("name");
            attr.Value = "GlobalMethodsAttribute";
            type.Attributes.Append(attr);
            attr = config.CreateAttribute("required");
            attr.Value = "true";
            type.Attributes.Append(attr);
            filter.AppendChild(type);

            type = config.CreateElement("type");
            attr = config.CreateAttribute("name");
            attr.Value = "IgnoreNamespaceAttribute";
            type.Attributes.Append(attr);
            attr = config.CreateAttribute("required");
            attr.Value = "true";
            type.Attributes.Append(attr);
            filter.AppendChild(type);

            type = config.CreateElement("type");
            attr = config.CreateAttribute("name");
            attr.Value = "IntrinsicPropertyAttribute";
            type.Attributes.Append(attr);
            attr = config.CreateAttribute("required");
            attr.Value = "true";
            type.Attributes.Append(attr);
            filter.AppendChild(type);

            type = config.CreateElement("type");
            attr = config.CreateAttribute("name");
            attr.Value = "NonScriptableAttribute";
            type.Attributes.Append(attr);
            attr = config.CreateAttribute("required");
            attr.Value = "true";
            type.Attributes.Append(attr);
            filter.AppendChild(type);

            type = config.CreateElement("type");
            attr = config.CreateAttribute("name");
            attr.Value = "PreserveCaseAttribute";
            type.Attributes.Append(attr);
            attr = config.CreateAttribute("required");
            attr.Value = "true";
            type.Attributes.Append(attr);
            filter.AppendChild(type);

            type = config.CreateElement("type");
            attr = config.CreateAttribute("name");
            attr.Value = "RecordAttribute";
            type.Attributes.Append(attr);
            attr = config.CreateAttribute("required");
            attr.Value = "true";
            type.Attributes.Append(attr);
            filter.AppendChild(type);

            config.Save(configFile);

            return true;
        }

        /// <summary>
        /// This is used to fix up the Script# elements in the reflection.org file
        /// </summary>
        private void FixScriptSharpReflectionData()
        {
            HashSet<string> typesWithGlobalAttribute = new HashSet<string>(),
                typesWithRecordAttribute = new HashSet<string>(), delegateTypes = new HashSet<string>();
            var delegateParameters = new Dictionary<string, XElement>();
            string reflectionDataModified = Path.Combine(Path.GetDirectoryName(builder.ReflectionInfoFilename),
                Guid.NewGuid().ToString() + ".xml");

            XElement apiNode, apiData, ancestors, parameters;
            string apiId, group, subgroup, containingType;

            // Make a first pass to find delegate types and their invoke members and types tagged with the
            // global and/or record attribute.
            using(XmlReader reader = XmlReader.Create(builder.ReflectionInfoFilename,
              new XmlReaderSettings { IgnoreWhitespace = true }))
            {
                reader.ReadToFollowing("api");

                while(!reader.EOF)
                {
                    if(reader.NodeType != XmlNodeType.Element)
                    {
                        reader.Read();
                        continue;
                    }

                    apiNode = (XElement)XNode.ReadFrom(reader);
                    apiData = apiNode.Element("apidata");
                    apiId = apiNode.Attribute("id").Value;
                    group = apiData.Attribute("group").Value;

                    // Types are always seen first followed by their members
                    if(group == "type")
                    {
                        ancestors = apiNode.Element("family")?.Element("ancestors");

                        if(ancestors != null && ancestors.Descendants("type").Any(
                          t => t.Attribute("api").Value == "T:System.MulticastDelegate"))
                        {
                            delegateTypes.Add(apiId);
                        }

                        if(apiNode.Descendants("attribute").Any(d => d.Element("type").Attribute(
                          "api").Value == "T:System.GlobalMethodsAttribute"))
                        {
                            typesWithGlobalAttribute.Add(apiId);
                        }

                        if(apiNode.Descendants("attribute").Any(d => d.Element("type").Attribute(
                          "api").Value == "T:System.RecordAttribute"))
                        {
                            typesWithRecordAttribute.Add(apiId);
                        }
                    }

                    if(group == "member")
                    {
                        containingType = apiNode.Element("containers").Element("type").Attribute("api").Value;

                        if(delegateTypes.Contains(containingType) && apiId[0] == 'M' &&
                          apiId.IndexOf(".Invoke(", StringComparison.Ordinal) != -1)
                        {
                            parameters = apiNode.Element("parameters");

                            if(parameters != null && !delegateParameters.ContainsKey(containingType))
                                delegateParameters.Add(containingType, new XElement(parameters));
                        }
                    }
                }
            }

            using(XmlReader reader = XmlReader.Create(builder.ReflectionInfoFilename, new XmlReaderSettings {
              IgnoreWhitespace = true }))
            using(XmlWriter writer = XmlWriter.Create(reflectionDataModified, new XmlWriterSettings {
              Indent = true, CloseOutput = true }))
            {
                writer.WriteStartDocument();
                reader.Read();

                while(!reader.EOF)
                {
                    if(reader.NodeType != XmlNodeType.Element)
                    {
                        reader.Read();
                        continue;
                    }

                    switch(reader.Name)
                    {
                        case "apis":
                        case "reflection":
                            writer.WriteStartElement(reader.Name);
                            reader.Read();
                            break;

                        case "api":
                            apiNode = (XElement)XNode.ReadFrom(reader);
                            apiData = apiNode.Element("apidata");

                            apiId = apiNode.Attribute("id").Value;
                            group = apiData.Attribute("group").Value;
                            subgroup = apiData.Attribute("subgroup")?.Value;

                            // Add a "scriptSharp" element to each API node so that the JavaScript syntax
                            // generator will apply the casing rules to the member name.
                            apiNode.Add(new XElement("scriptSharp"));

                            if(group == "type")
                            {
                                ancestors = apiNode.Element("family")?.Element("ancestors");

                                if(ancestors != null && ancestors.Descendants("type").Any(
                                  t => t.Attribute("api").Value == "T:System.Enum"))
                                {
                                    // Fix subgroup and remove ancestors from enumerations
                                    apiData.Attribute("subgroup").Value = "enumeration";
                                    ancestors.Parent.Remove();

                                    // Remove invalid enumeration members
                                    foreach(var el in apiNode.Descendants("element").Where(
                                      el => !el.Attribute("api").Value.StartsWith("F:", StringComparison.Ordinal) ||
                                        el.Attribute("api").Value.IndexOf("value__", StringComparison.Ordinal) != -1).ToArray())
                                    {
                                        el.Remove();
                                    }
                                }

                                if(delegateTypes.Contains(apiId))
                                {
                                    // Fix subgroup and remove ancestors and elements from enumerations
                                    apiData.Attribute("subgroup").Value = "delegate";
                                    ancestors?.Parent.Remove();
                                    apiNode.Element("elements").Remove();

                                    // Add delegate parameters
                                    if(delegateParameters.TryGetValue(apiId, out parameters))
                                        apiNode.Add(parameters);
                                }
                            }

                            if(group == "member")
                            {
                                containingType = apiNode.Element("containers").Element("type").Attribute("api").Value;

                                // Annotate members in types that have the global attributes
                                if(typesWithGlobalAttribute.Contains(containingType))
                                    apiData.Add(new XAttribute("global", "true"));

                                // Annotate constructors in types that have the record attribute
                                if(subgroup == "constructor" && typesWithRecordAttribute.Contains(containingType))
                                    apiData.Add(new XAttribute("record", "true"));
                            }

                            apiNode.WriteTo(writer);
                            break;

                        default:
                            writer.WriteNode(reader.ReadSubtree(), true);
                            break;
                    }
                }

                writer.WriteEndDocument();
            }

            File.Delete(builder.ReflectionInfoFilename);
            File.Move(reflectionDataModified, builder.ReflectionInfoFilename);
        }
        #endregion
    }
}
