//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildProcess.ConfigFiles.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/14/2016
// Note    : Copyright 2006-2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the code used to transform and modify configuration files for the build
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/07/2006  EFW  Created the code
// 05/23/2015  EFW  Refactored the code and moved all substitution tag handling into a separate class
// 12/01/2015  EFW  Merged conceptual and reference topic build steps
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

using Sandcastle.Core.BuildAssembler.BuildComponent;

using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.Utils.BuildEngine
{
    partial class BuildProcess
    {
        #region Generate API filter
        //=====================================================================

        /// <summary>
        /// This is used to generate the API filter collection used by MRefBuilder to exclude items from the
        /// reflection information file.
        /// </summary>
        /// <remarks>Namespaces and members with an <c>&lt;exclude /&gt;</c> tag in their comments are removed
        /// using the ripping feature as it is more efficient than searching for and removing them from the
        /// reflection file after it has been generated especially on large projects.</remarks>
        private void GenerateApiFilter()
        {
            XmlNodeList excludes;
            XmlNode docMember;
            List<string> ripList;
            string nameSpace, memberName, typeName, fullName;
            int pos;

            this.ReportProgress(BuildStep.GenerateApiFilter, "Generating API filter for MRefBuilder...");

            if(this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                return;

            this.ExecutePlugIns(ExecutionBehaviors.Before);

            ripList = new List<string>();

            // Add excluded namespaces
            foreach(NamespaceSummaryItem ns in project.NamespaceSummaries)
                if(!ns.IsDocumented && !ns.IsGroup)
                {
                    memberName = ns.Name;

                    if(memberName[0] == '(')
                        memberName = "N:";  // Global namespace
                    else
                        memberName = "N:" + memberName;

                    ripList.Add(memberName);
                }

            // If the namespace summaries don't contain an explicit entry for the global namespace, exclude it
            // by default.
            if(project.NamespaceSummaries[null] == null)
                ripList.Add("N:");

            // Add members excluded via comments
            foreach(XmlCommentsFile comments in commentsFiles)
            {
                excludes = comments.Members.SelectNodes("//exclude/..");

                foreach(XmlNode member in excludes)
                {
                    // It should appear at the same level as <summary> so that we can find the member name in the
                    // parent node.
                    if(member.Attributes["name"] == null)
                    {
                        this.ReportProgress("    Incorrect placement of <exclude/> tag.  Unable to locate " +
                            "member name.");
                        continue;
                    }

                    memberName = member.Attributes["name"].Value;

                    if(!ripList.Contains(memberName))
                        ripList.Add(memberName);
                }
            }

            // Sort by entry type and name so that we create the collection from the namespace down to the
            // members.
            ripList.Sort((x, y) =>
            {
                ApiEntryType xType = ApiFilter.ApiEntryTypeFromLetter(x[0]),
                    yType = ApiFilter.ApiEntryTypeFromLetter(y[0]);

                if(xType == yType)
                    return String.Compare(x, y, false, CultureInfo.CurrentCulture);

                return (int)xType - (int)yType;
            });

            // Get the project's API filter and merge the members from the rip list
            var apiFilter = project.ApiFilter;

            // For the API filter to work, we have to nest the entries by namespace, type, and member.  As such,
            // we have to break apart what we've got in the list and merge it with the stuff the user may have
            // specified using the project's API filter property.
            foreach(string member in ripList)
            {
                // Namespaces are easy
                if(member[0] == 'N')
                {
                    if(!apiFilter.MergeExclusionEntry(ApiEntryType.Namespace, member.Substring(2)))
                        this.ReportWarning("BE0008", "Namespace '{0}' excluded via namespace comments " +
                            "conflicted with API filter setting.  Exclusion ignored.", member);

                    continue;
                }

                // Types and members are a bit tricky.  Since we don't have any real context, we have to assume
                // that we can remove the last part and look it up.  If a type entry isn't found, we can assume
                // it's the namespace.  Where this can fail is on a nested class where the parent class is
                // lacking XML comments.  Not much we can do about it in that case.
                if(member[0] == 'T')
                {
                    fullName = nameSpace = member;
                    typeName = member.Substring(2);
                    memberName = null;
                }
                else
                {
                    // Strip parameters.  The ripping feature only goes to the name level.  If one overload is
                    // ripped, they are all ripped.
                    pos = member.IndexOf('(');

                    if(pos != -1)
                        fullName = memberName = member.Substring(0, pos);
                    else
                        fullName = memberName = member;

                    // Generic method
                    pos = memberName.IndexOf("``", StringComparison.Ordinal);

                    if(pos != -1)
                        memberName = memberName.Substring(0, pos);

                    pos = memberName.LastIndexOf('.');
                    memberName = memberName.Substring(pos + 1);
                    typeName = fullName.Substring(2, pos - 2);
                    nameSpace = "T:" + typeName;
                }

                for(int idx = 0; idx < commentsFiles.Count; idx++)
                {
                    docMember = commentsFiles[idx].Members.SelectSingleNode("member[@name='" + nameSpace + "']");

                    if(docMember != null)
                    {
                        pos = nameSpace.LastIndexOf('.');

                        if(pos == -1)
                        {
                            nameSpace = "N:";
                            break;
                        }
                        else
                            nameSpace = nameSpace.Substring(0, pos);

                        idx = -1;
                    }
                }

                nameSpace = nameSpace.Substring(2);

                // If the names still match, we probably didn't find comments for the type so assume the
                // namespace is the part up to the last period.
                if(nameSpace == typeName)
                {
                    pos = nameSpace.LastIndexOf('.');

                    if(pos != -1)
                        nameSpace = nameSpace.Substring(0, pos);
                    else
                        nameSpace = "N:";   // Global namespace
                }

                if(apiFilter.AddNamespaceChild(fullName, nameSpace, typeName, memberName))
                {
                    if(fullName.Length > 2)
                    {
                        // If it's a nested class, adjust the filter name
                        fullName = typeName;
                        typeName = typeName.Substring(nameSpace.Length + 1);

                        if(typeName.IndexOf('.') != -1)
                            foreach(ApiFilter ns in apiFilter)
                                if(ns.FullName == nameSpace)
                                {
                                    foreach(ApiFilter t in ns.Children)
                                        if(t.FullName == fullName)
                                        {
                                            t.FilterName = typeName;
                                            break;
                                        }

                                    break;
                                }
                    }
                }
                else
                    this.ReportWarning("BE0009", "'{0}' is marked with <exclude /> but conflicted with the " +
                        "API filter setting.  Exclusion ignored.", member);
            }

            this.ExecutePlugIns(ExecutionBehaviors.After);
        }
        #endregion

        #region Component configuration merging
        //=====================================================================

        /// <summary>
        /// This is used to merge the component configurations from the project with the
        /// <strong>sandcastle.config</strong> file.
        /// </summary>
        private void MergeComponentConfigurations()
        {
            XmlDocument config;
            string configName;

            this.ReportProgress(BuildStep.MergeCustomConfigs, "Merging custom build component configurations");

            // Reset the adjusted instance values to match the configuration instance values
            foreach(var component in buildComponents.Values)
            {
                component.ReferenceBuildPlacement.AdjustedInstance = component.ReferenceBuildPlacement.Instance;
                component.ConceptualBuildPlacement.AdjustedInstance = component.ConceptualBuildPlacement.Instance;
            }

            if(this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                return;

            this.ExecutePlugIns(ExecutionBehaviors.Before);

            configName = workingFolder + "sandcastle.config";
            this.ReportProgress(configName);

            config = new XmlDocument();
            config.Load(configName);

            this.ReportProgress("  Updating reference topic configurations.");
            this.MergeConfigurations(config, false);

            // Do the same for conceptual configuration if necessary
            if(this.ConceptualContent.ContentLayoutFiles.Count != 0)
            {
                this.ReportProgress("  Updating conceptual topic configurations.");
                this.MergeConfigurations(config, true);

                // Remove the example component if there are no snippets file
                if(this.ConceptualContent.CodeSnippetFiles.Count == 0)
                {
                    this.ReportProgress("    Removing unused ExampleComponent.");

                    var exampleComponent = config.SelectSingleNode("configuration/dduetools/builder/components/" +
                        "component[@id='Switch Component']/case[@value='MAML']/component[@id='Example Component']");

                    if(exampleComponent != null)
                        exampleComponent.ParentNode.RemoveChild(exampleComponent);
                }
            }
            else
            {
                // Remove the conceptual content components since they aren't needed
                var rootNode = config.SelectSingleNode("configuration/dduetools/builder/components/" +
                    "component[@id='Switch Component']/case[@value='MAML']");

                if(rootNode != null)
                {
                    this.ReportProgress("  No conceptual content.  Removing conceptual content components.");
                    rootNode.ParentNode.RemoveChild(rootNode);
                }
            }

            config.Save(configName);

            this.ExecutePlugIns(ExecutionBehaviors.After);
        }

        /// <summary>
        /// This handles merging the build component configurations into the given configuration file
        /// </summary>
        /// <param name="config">The configuration file into which the configurations are merged</param>
        /// <param name="isConceptualConfig">True for a conceptual configuration file, false for a reference
        /// configuration file.</param>
        private void MergeConfigurations(XmlDocument config, bool isConceptualConfig)
        {
            Dictionary<string, XmlNode> outputNodes = new Dictionary<string, XmlNode>();
            BuildComponentFactory factory;
            BuildComponentConfiguration projectComp;
            XmlNode rootNode, configNode, clone;
            XmlNodeList outputFormats;
            PlacementAction placement;
            string configType;

            rootNode = config.SelectSingleNode("configuration/dduetools/builder/components/" +
                "component[@id='Switch Component']/case[@value='" + (isConceptualConfig ? "MAML" : "API") + "']");

            foreach(string id in project.ComponentConfigurations.Keys)
            {
                projectComp = project.ComponentConfigurations[id];

                if(!buildComponents.TryGetValue(id, out factory))
                    throw new BuilderException("BE0021", String.Format(CultureInfo.CurrentCulture,
                        "The project contains a reference to a custom build component '{0}' that could not " +
                        "be found.", id));

                if(isConceptualConfig)
                {
                    placement = factory.ConceptualBuildPlacement.Placement;
                    configType = "conceptual";
                }
                else
                {
                    placement = factory.ReferenceBuildPlacement.Placement;
                    configType = "reference";
                }

                if(placement == PlacementAction.None)
                    this.ReportProgress("    Skipping component '{0}', not used in {1} build", id, configType);
                else
                    if(projectComp.Enabled)
                    {
                        configNode = config.CreateDocumentFragment();
                        configNode.InnerXml = substitutionTags.TransformText(projectComp.Configuration);
                        outputNodes.Clear();

                        foreach(XmlNode match in configNode.SelectNodes("//helpOutput"))
                        {
                            outputNodes.Add(match.Attributes["format"].Value, match);
                            match.ParentNode.RemoveChild(match);
                        }

                        // Is it output format specific?
                        if(outputNodes.Count == 0)
                        {
                            // Replace the component in the file
                            this.MergeComponent(id, factory, rootNode, configNode, isConceptualConfig, null);
                        }
                        else
                        {
                            // Replace the component in each output format node
                            outputFormats = rootNode.SelectNodes(
                                "component[@id='Multi-format Output Component']/helpOutput");

                            if(outputFormats.Count != 0)
                            {
                                foreach(XmlNode format in outputFormats)
                                {
                                    clone = configNode.Clone();
                                    clone.FirstChild.InnerXml += outputNodes[format.Attributes["format"].Value].InnerXml;
                                    this.MergeComponent(id, factory, format, clone, isConceptualConfig, null);
                                }
                            }
                            else
                            {
                                // Some presentation styles only support one help format.  In those cases, get
                                // the configuration for that format and use it alone.
                                XmlNode format;

                                if(outputNodes.TryGetValue(project.HelpFileFormat.ToString(), out format))
                                {
                                    configNode.FirstChild.InnerXml += format.InnerXml;
                                    this.MergeComponent(id, factory, rootNode, configNode, isConceptualConfig, null);
                                }
                                else
                                    this.ReportProgress("    Skipping component '{0}', configuration for help " +
                                        "format '{1}' not found", id, project.HelpFileFormat);
                            }
                        }
                    }
                    else
                        this.ReportProgress("    The configuration for '{0}' is disabled and will not be used.", id);
            }
        }

        /// <summary>
        /// This handles merging of the custom component configurations into the configuration file including
        /// dependencies.
        /// </summary>
        /// <param name="id">The ID of the component to merge</param>
        /// <param name="factory">The build component factory</param>
        /// <param name="rootNode">The root container node</param>
        /// <param name="configNode">The configuration node to merge</param>
        /// <param name="isConceptualConfig">True if this is a conceptual content configuration file or false if
        /// it is a reference build configuration file.</param>
        /// <param name="mergeStack">A stack used to check for circular build component dependencies.  Pass null
        /// on the first non-recursive call.</param>
        private void MergeComponent(string id, BuildComponentFactory factory, XmlNode rootNode, XmlNode configNode,
          bool isConceptualConfig, Stack<string> mergeStack)
        {
            BuildComponentFactory dependencyFactory;
            ComponentPlacement position;
            XmlNodeList matchingNodes;
            XmlNode node;
            string replaceId;

            // Merge dependent component configurations first
            if(factory.Dependencies.Any())
            {
                if(mergeStack == null)
                    mergeStack = new Stack<string>();

                foreach(string dependency in factory.Dependencies)
                {
                    node = rootNode.SelectSingleNode("component[@id='" + dependency + "']");

                    // If it's already there or would create a circular dependency, ignore it
                    if(node != null || mergeStack.Contains(dependency))
                        continue;

                    // Add the dependency with a default configuration
                    if(!buildComponents.TryGetValue(dependency, out dependencyFactory))
                        throw new BuilderException("BE0023", String.Format(CultureInfo.CurrentCulture,
                            "The project contains a reference to a custom build component '{0}' that has a " +
                            "dependency '{1}' that could not be found.", id, dependency));

                    node = rootNode.OwnerDocument.CreateDocumentFragment();
                    node.InnerXml = substitutionTags.TransformText(dependencyFactory.DefaultConfiguration);

                    this.ReportProgress("    Merging '{0}' dependency for '{1}'", dependency, id);

                    mergeStack.Push(dependency);
                    this.MergeComponent(dependency, dependencyFactory, rootNode, node, isConceptualConfig, mergeStack);
                    mergeStack.Pop();
                }
            }

            position = (!isConceptualConfig) ? factory.ReferenceBuildPlacement : factory.ConceptualBuildPlacement;

            // Find all matching components by ID
            replaceId = position.Id;
            matchingNodes = rootNode.SelectNodes("component[@id='" + replaceId + "']");

            // If replacing another component, search for that by ID and replace it if found
            if(position.Placement == PlacementAction.Replace)
            {
                if(matchingNodes.Count < position.AdjustedInstance)
                {
                    this.ReportProgress("    Could not find configuration '{0}' (instance {1}) to replace with " +
                        "configuration for '{2}' so it will be omitted.", replaceId, position.AdjustedInstance, id);

                    // If it's a dependency, that's a problem
                    if(mergeStack.Count != 0)
                        throw new BuilderException("BE0024", "Unable to add dependent configuration: " + id);

                    return;
                }

                rootNode.ReplaceChild(configNode, matchingNodes[position.AdjustedInstance - 1]);

                this.ReportProgress("    Replaced configuration for '{0}' (instance {1}) with configuration " +
                    "for '{2}'", replaceId, position.AdjustedInstance, id);

                // Adjust instance values on matching components
                foreach(var component in buildComponents.Values)
                    if(!isConceptualConfig)
                    {
                        if(component.ReferenceBuildPlacement.Id == replaceId &&
                          component.ReferenceBuildPlacement.AdjustedInstance > position.AdjustedInstance)
                        {
                            component.ReferenceBuildPlacement.AdjustedInstance--;
                        }
                    }
                    else
                        if(component.ConceptualBuildPlacement.Id == replaceId &&
                          component.ConceptualBuildPlacement.AdjustedInstance > position.AdjustedInstance)
                        {
                            component.ConceptualBuildPlacement.AdjustedInstance--;
                        }

                return;
            }

            // See if the configuration already exists.  If so, replace it.  We'll assume it's already in the
            // correct location.
            node = rootNode.SelectSingleNode("component[@id='" + id + "']");

            if(node != null)
            {
                this.ReportProgress("    Replacing default configuration for '{0}' with the custom configuration", id);
                rootNode.ReplaceChild(configNode, node);
                return;
            }

            // Create the node and add it in the correct location
            switch(position.Placement)
            {
                case PlacementAction.Start:
                    rootNode.InsertBefore(configNode, rootNode.ChildNodes[0]);
                    this.ReportProgress("    Added configuration for '{0}' to the start of the configuration file", id);
                    break;

                case PlacementAction.End:
                    rootNode.InsertAfter(configNode,
                        rootNode.ChildNodes[rootNode.ChildNodes.Count - 1]);
                    this.ReportProgress("    Added configuration for '{0}' to the end of the configuration file", id);
                    break;

                case PlacementAction.Before:
                    if(matchingNodes.Count < position.AdjustedInstance)
                        this.ReportProgress("    Could not find configuration '{0}' (instance {1}) to add " +
                            "configuration for '{2}' so it will be omitted.", replaceId, position.AdjustedInstance, id);
                    else
                    {
                        rootNode.InsertBefore(configNode, matchingNodes[position.AdjustedInstance - 1]);
                        this.ReportProgress("    Added configuration for '{0}' before '{1}' (instance {2})",
                            id, replaceId, position.AdjustedInstance);
                    }
                    break;

                default:    // After
                    if(matchingNodes.Count < position.AdjustedInstance)
                        this.ReportProgress("    Could not find configuration '{0}' (instance {1}) to add " +
                            "configuration for '{2}' so it will be omitted.", replaceId, position.AdjustedInstance, id);
                    else
                    {
                        rootNode.InsertAfter(configNode, matchingNodes[position.AdjustedInstance - 1]);
                        this.ReportProgress("    Added configuration for '{0}' after '{1}' (instance {2})",
                            id, replaceId, position.AdjustedInstance);
                    }
                    break;
            }
        }
        #endregion
    }
}
