//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : ManualVisibilityApiFilterPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/25/2015
// Note    : Copyright 2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in that can be used to manually apply the visibility settings and API filter from
// the project to the reflection data file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/25/2015  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: memberdata proceduredata typedata

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class can be used to manually apply the visibility settings and API filter from the project
    /// to the reflection data file.
    /// </summary>
    /// <remarks><note type="note">This is only necessary if the Generate Reflection Information build step is
    /// suppressed or replaced by some other means.  In such cases, the visibility settings and API filter are
    /// not applied unless this plug-in is used.  If the reflection information file is produced by
    /// <strong>MRefBuilder.exe</strong>, there is no need to use this plug-in as it will apply the visibility
    /// settings and API filter automatically to the file that it produces.</note></remarks>
    [HelpFileBuilderPlugInExport("Manual Visibility/API Filter", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright, Description = "This plug-in can be used to manually apply the " +
      "visibility settings and API filter from the project to the reflection data file if MRefBuilder is not " +
      "used.\r\n\r\nNOTE:  This is only necessary if the Generate Reflection Information build step is " +
      "suppressed or replaced by some other means as in the AjaxDoc Builder plug-in for example.  In such cases, " +
      "the visibility settings and API filter are not applied unless this plug-in is used.  If the reflection " +
      "information file is produced by MRefBuilder.exe, there is no need to use this plug-in as it will apply " +
      "the visibility settings and API filter automatically to the file that it produces.")]
    public sealed class ManualVisibilityApiFilterPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;

        private BuildProcess builder;

        // A set of members that need to be excluded from the reflection information based on the visibility
        // settings.
        private HashSet<string> excludedMembers;

        private static Regex reExcludeElementEntry = new Regex(
            "<element api=\"([^\n\"]+?)\">.*?</element>|<element api=\"([^\n\"]+?)\" />", RegexOptions.Singleline);

        private MatchEvaluator excludeElementEval;

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
                        // This one has a lower priority as it may need to remove things added by other plug-ins
                        new ExecutionPoint(BuildStep.TransformReflectionInfo, ExecutionBehaviors.Before, 100),
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
            MessageBox.Show("This plug-in has no configurable settings", "Manual Visibility/API Filter Plug-In",
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

            excludeElementEval = new MatchEvaluator(OnExcludeElement);
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            builder.ReportProgress("Applying visibility settings manually");
            this.ApplyVisibilityProperties(builder.ReflectionInfoFilename);

            // Don't apply the API filter settings in a partial build
            if(builder.PartialBuildType == PartialBuildType.None && builder.CurrentProject.ApiFilter.Count != 0)
            {
                builder.ReportProgress("Applying API filter manually");
                this.ApplyManualApiFilter(builder.CurrentProject.ApiFilter, builder.ReflectionInfoFilename);
            }
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

        #region Manual visibility filter methods
        //=====================================================================

        /// <summary>
        /// Apply the project's Visibility category properties to the given reflection information file
        /// </summary>
        /// <param name="reflectionInfoFile">The name of the reflection information file to use</param>
        /// <remarks>This is used to remove entries from the reflection information file so that it does not
        /// appear in the help file.  See the <c>Document*</c> properties in the <see cref="SandcastleProject"/>
        /// class for information on the items removed.</remarks>
        public void ApplyVisibilityProperties(string reflectionInfoFile)
        {
            XmlNodeList apis;
            XmlNode api;
            MatchCollection matches;
            string xpath;
            SandcastleProject project = builder.CurrentProject;

            builder.ReportProgress("Applying visibility properties to reflection information file");

            // The reflection file can contain tens of thousands of entries for large assemblies.  Removing the
            // elements at the time they were found proved to be too slow for such large files.  As such, we will
            // get a list of all API and element entries to be removed and we'll remove them using regular
            // expressions and a match evaluator at the end of this method.
            excludedMembers = new HashSet<string>();

            try
            {
                XmlDocument reflectionDoc = new XmlDocument();
                reflectionDoc.Load(reflectionInfoFile);

                XmlNode apisRoot = reflectionDoc.SelectSingleNode("reflection/apis");

                // Removal of excluded members is handled by the API filter so we don't have to deal with them here

                // Apply the visibility properties
                if(!project.DocumentAttributes)
                    this.RemoveAttributes(apisRoot);

                if(!project.DocumentExplicitInterfaceImplementations)
                    this.RemoveExplicitInterfaceImplementations(apisRoot);

                // Determine if any inherited members need to be removed
                if(!project.DocumentInheritedMembers || !project.DocumentInheritedFrameworkMembers ||
                  (!project.DocumentInheritedFrameworkInternalMembers &&
                  (project.DocumentInternals || project.DocumentPrivates)) ||
                  (!project.DocumentInheritedFrameworkPrivateMembers &&
                  (project.DocumentInternals || project.DocumentPrivates)))
                {
                    this.RemoveInheritedMembers(apisRoot);
                }

                if(!project.DocumentPrivates)
                {
                    // Remove all private members excluding EII entries which are controlled by the Document EII
                    // property.
                    this.RemoveMembers(apisRoot, "api[memberdata/@visibility='private' and " +
                        "(not(proceduredata) or proceduredata/@virtual='false')]", "private members");

                    // Remove all private types
                    this.RemoveMembers(apisRoot, "api[typedata/@visibility='private']", "private types");
                }
                else
                    if(!project.DocumentPrivateFields)
                    {
                        this.RemoveMembers(apisRoot, "api[apidata/@subgroup='field' and " +
                            "memberdata/@visibility='private']", "private fields");

                        // This will get rid of a load of base framework class fields too
                        this.RemoveMembers(apisRoot, "api/elements/element[starts-with(@api, 'F:System.') or " +
                            "starts-with(@api, 'F:Microsoft.')]", "private framework fields");
                    }
                    else
                    {
                        // Remove backing fields that correspond to events.  These can never be documented and
                        // should not show up.
                        this.RemoveMembers(apisRoot, "api[apidata/@subgroup='field' and " +
                            "contains(returns/type/@api, 'EventHandler')]", "event backer fields");
                    }

                if(!project.DocumentInternals)
                {
                    // Remove internal members
                    this.RemoveMembers(apisRoot, "api[memberdata/@visibility='assembly' or " +
                        "memberdata/@visibility='family and assembly']", "internal members");

                    // Remove internal types
                    this.RemoveMembers(apisRoot, "api[typedata/@visibility='assembly' or " +
                        "typedata/@visibility='family and assembly']", "internal types");
                }

                if(!project.DocumentProtected)
                {
                    xpath = "memberdata/@visibility='family'";

                    // If DocumentInternals is false, remove protected internal members as well. If not, leave
                    // them alone.
                    if(!project.DocumentInternals)
                        xpath += " or memberdata/@visibility='family or assembly'";

                    this.RemoveMembers(apisRoot, "api[" + xpath + "]", "protected members");

                    // Remove protected inherited framework members too
                    this.RemoveMembers(apisRoot, "api/elements/element[(starts-with(substring-after(@api, ':')," +
                        "'System.') or starts-with(substring-after(@api,':'), 'Microsoft.')) and (" + xpath + ")]",
                        "protected inherited framework members");
                }
                else
                {
                    if(project.DocumentProtectedInternalAsProtected)
                        this.ModifyProtectedInternalVisibility(apisRoot);

                    if(!project.DocumentSealedProtected)
                        this.RemoveSealedProtected(apisRoot);
                }

                // Now remove unwanted members if any were found
                if(excludedMembers.Count != 0)
                {
                    builder.ReportProgress("    Removing previously noted unwanted APIs and elements");
                    apis = apisRoot.SelectNodes("*");

                    for(int idx = 0; idx < apis.Count; idx++)
                    {
                        api = apis[idx];

                        if(!excludedMembers.Contains(api.Attributes["id"].Value))
                        {
                            // An XPath sub-query for the <element> entries is way to slow especially on very
                            // large files so we'll use a regular expression and a match evaluator to get rid of
                            // unwanted <element> entries.
                            matches = reExcludeElementEntry.Matches(api.InnerXml);

                            // However, just doing a straight replace consumes a large amount of memory.  As
                            // such, check the results for those that contain an unwanted element and only do it
                            // when really necessary.
                            foreach(Match m in matches)
                                if(excludedMembers.Contains(m.Groups[1].Value) ||
                                  excludedMembers.Contains(m.Groups[2].Value))
                                {
                                    api.InnerXml = reExcludeElementEntry.Replace(api.InnerXml, excludeElementEval);
                                    break;
                                }
                        }
                        else
                            api.ParentNode.RemoveChild(api);
                    }

                    excludedMembers = null;
                }

                // Backup the original for reference and save the changed file
                File.Copy(reflectionInfoFile, Path.ChangeExtension(reflectionInfoFile, ".bak"), true);
                reflectionDoc.Save(reflectionInfoFile);
            }
            catch(BuilderException )
            {
                // If it's from one of the other methods, just rethrow it
                throw;
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0044", "Error applying Visibility properties: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Remove attribute information
        /// </summary>
        /// <param name="apisRoot">The root APIs node</param>
        private void RemoveAttributes(XmlNode apisRoot)
        {
            XmlNodeList elements;
            XmlNode typeNode, parent;
            int count = 0;

            // These attributes are always kept as they provide useful information or are used by other
            // components.  Some are converted to metadata by MRefBuilder and cannot be excluded.  They are
            // listed here for completeness.  See MRefBuilder.config for details.
            HashSet<string> keepAttrs = new HashSet<string>
            {
                "T:System.FlagsAttribute",
                "T:System.ObsoleteAttribute",
                "T:System.SerializableAttribute",
                "T:System.ComponentModel.TypeConverterAttribute",
                "T:System.Reflection.AssemblyFileVersionAttribute",
                "T:System.Runtime.CompilerServices.ExtensionAttribute",
                "T:System.Runtime.CompilerServices.FixedBufferAttribute",
                "T:System.Runtime.InteropServices.ComImportAttribute",
                "T:System.Runtime.InteropServices.DllImportAttribute",
                "T:System.Runtime.InteropServices.FieldOffsetAttribute",
                "T:System.Runtime.InteropServices.OptionalAttribute",
                "T:System.Runtime.InteropServices.PreserveSigAttribute",
                "T:System.Runtime.InteropServices.StructLayoutAttribute",
                "T:System.Security.AllowPartiallyTrustedCallersAttribute",
                "T:System.Security.Permissions.HostProtectionAttribute",
                "T:System.Web.UI.PersistenceModeAttribute",
                "T:System.Windows.Markup.ContentPropertyAttribute",

                // AjaxDoc/Script# attributes
                "T:System.AttachedPropertyAttribute",
                "T:System.GlobalMethodsAttribute",
                "T:System.IgnoreNamespaceAttribute",
                "T:System.IntrinsicPropertyAttribute",
                "T:System.NonScriptableAttribute",
                "T:System.PreserveCaseAttribute",
                "T:System.RecordAttribute"
            };

            try
            {
                // Find all of the attribute nodes
                elements = apisRoot.SelectNodes("api/attributes/attribute");

                for(int idx = 0; idx < elements.Count; idx++)
                {
                    typeNode = elements[idx].SelectSingleNode("type");

                    if(typeNode != null && !keepAttrs.Contains(typeNode.Attributes["api"].Value))
                    {
                        parent = elements[idx].ParentNode;
                        parent.RemoveChild(elements[idx]);
                        count++;

                        // Remove the parent if it is empty
                        if(parent.ChildNodes.Count == 0)
                            parent.ParentNode.RemoveChild(parent);
                    }
                }

                builder.ReportProgress("    {0} attribute nodes removed", count);
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0045", "Error removing attributes nodes: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Remove explicit interface implementation information
        /// </summary>
        /// <param name="apisRoot">The root APIs node</param>
        private void RemoveExplicitInterfaceImplementations(XmlNode apisRoot)
        {
            XmlNodeList elements;
            int idx;

            try
            {
                // Find all explicit implementations in the documented assemblies
                elements = apisRoot.SelectNodes("api[memberdata/@visibility='private' and " +
                    "proceduredata/@virtual='true']");

                for(idx = 0; idx < elements.Count; idx++)
                    elements[idx].ParentNode.RemoveChild(elements[idx]);

                builder.ReportProgress("    {0} explicit interface implementations removed", elements.Count);

                // Get rid of the matching element entries and those for base classes too.  This ensures they
                // don't show up in the "class members" help page.
                elements = apisRoot.SelectNodes("api/elements/element[contains(@api, '#') and " +
                    "not(contains(@api, '.#ctor')) and not(contains(@api, '.#cctor')) and " +
                    "not(contains(@api, ':J#'))]");

                for(idx = 0; idx < elements.Count; idx++)
                    elements[idx].ParentNode.RemoveChild(elements[idx]);

                builder.ReportProgress("    {0} local and base class EII elements removed", elements.Count);
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0046", "Error removing EII nodes: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Remove inherited member information from each type's element list based on the project settings
        /// </summary>
        /// <param name="apisRoot">The root APIs node</param>
        private void RemoveInheritedMembers(XmlNode apisRoot)
        {
            XmlNodeList types, elements;
            int typeIdx, elementIdx, removed = 0;
            string typeName, query;
            SandcastleProject project = builder.CurrentProject;

            bool frameworkOnly = project.DocumentInheritedMembers,
                 frameworkAll = project.DocumentInheritedFrameworkMembers,
                 frameworkInternal = project.DocumentInheritedFrameworkInternalMembers,
                 frameworkPrivate = project.DocumentInheritedFrameworkPrivateMembers;

            try
            {
                // Find all types
                types = apisRoot.SelectNodes("api[starts-with(@id, 'T:')]");

                for(typeIdx = 0; typeIdx < types.Count; typeIdx++)
                {
                    // Only remove inherited Framework members?
                    if(frameworkOnly)
                    {
                        query = "elements/element[(starts-with(substring-after(@api,':'),'System.') or " +
                            "starts-with(substring-after(@api,':'),'Microsoft.'))";

                        // If just removing privates and/or internals, add the necessary additional filter
                        // conditions.
                        if(frameworkAll)
                            if(!frameworkPrivate && !frameworkInternal)
                                query += " and (memberdata/@visibility = 'private' or memberdata/@visibility = " +
                                    "'assembly' or memberdata/@visibility = 'family and assembly')";
                            else
                                if(!frameworkPrivate)
                                    query += " and memberdata/@visibility = 'private'";
                                else
                                    query += " and (memberdata/@visibility = 'assembly' or " +
                                        "memberdata/@visibility = 'family and assembly')";

                        query += "]";

                        elements = types[typeIdx].SelectNodes(query);
                    }
                    else
                    {
                        // Get rid of any member not starting with this entry's type.  This includes all
                        // framework members too.
                        typeName = types[typeIdx].Attributes["id"].Value.Substring(2);
                        elements = types[typeIdx].SelectNodes("elements/element[not(starts-with(" +
                            "substring-after(@api,':'),'" + typeName + ".'))]");
                    }

                    for(elementIdx = 0; elementIdx < elements.Count; elementIdx++)
                    {
                        elements[elementIdx].ParentNode.RemoveChild(elements[elementIdx]);
                        removed++;
                    }
                }

                builder.ReportProgress("    {0} inherited member elements removed", removed);
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0047", "Error removing inherited members: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Remove member information matching the specified XPath query.
        /// </summary>
        /// <param name="apisRoot">The root APIs node</param>
        /// <param name="xpath">The XPath query used to find the members.</param>
        /// <param name="memberDesc">A description of the members removed.</param>
        /// <returns>The number of members to be removed</returns>
        /// <remarks>Actual removal of the members is deferred.  On very large files, the XPath queries took too
        /// long when removing the &lt;element&gt; members.</remarks>
        private int RemoveMembers(XmlNode apisRoot, string xpath, string memberDesc)
        {
            XmlNodeList apis;
            int count = 0;
            string id;

            try
            {
                // Find all matching members
                apis = apisRoot.SelectNodes(xpath);

                foreach(XmlNode node in apis)
                {
                    if(node.Attributes["id"] != null)
                        id = node.Attributes["id"].Value;
                    else
                        id = node.Attributes["api"].Value;

                    if(id != null && excludedMembers.Add(id))
                        count++;
                }

                if(!String.IsNullOrEmpty(memberDesc))
                    builder.ReportProgress("    {0} {1} noted for removal", count, memberDesc);
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0048", String.Format(CultureInfo.CurrentCulture,
                    "Error removing {0}: {1}", memberDesc, ex.Message), ex);
            }

            return count;
        }

        /// <summary>
        /// Change the visibility of "protected internal" members to
        /// "protected".
        /// </summary>
        /// <param name="apisRoot">The root APIs node</param>
        private void ModifyProtectedInternalVisibility(XmlNode apisRoot)
        {
            XmlNodeList members;

            try
            {
                // Find all matching members
                members = apisRoot.SelectNodes("api/memberdata[@visibility='family or assembly']");

                foreach(XmlNode n in members)
                    n.Attributes["visibility"].Value = "family";

                builder.ReportProgress("    {0} 'protected internal' members changed to 'protected'", members.Count);
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0049", String.Format(CultureInfo.CurrentCulture,
                    "Error changing protected internal to protected: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Remove protected members from sealed classes.
        /// </summary>
        /// <param name="apisRoot">The root APIs node</param>
        private void RemoveSealedProtected(XmlNode apisRoot)
        {
            XmlNodeList elements;
            XmlNode member;
            int elementIdx, removed = 0;
            string elementType, memberType;

            try
            {
                // Find all matching members
                elements = apisRoot.SelectNodes("api[apidata/@subgroup != 'enumeration' and " +
                    "typedata/@sealed = 'true']/elements/*");

                for(elementIdx = 0; elementIdx < elements.Count; elementIdx++)
                    if(!elements[elementIdx].HasChildNodes)
                    {
                        member = apisRoot.SelectSingleNode("api[@id='" +
                            elements[elementIdx].Attributes["api"].Value +
                            "' and (memberdata/@visibility='family' or " +
                            "memberdata/@visibility='family or assembly')]");

                        if(member != null)
                        {
                            // If the type matches, delete the member too
                            elementType = elements[elementIdx].ParentNode.ParentNode.Attributes[
                                "id"].Value.Substring(2);
                            memberType = member.Attributes["id"].Value.Substring(2);
                            memberType = memberType.Substring(0, memberType.LastIndexOf('.'));

                            if(memberType == elementType)
                                member.ParentNode.RemoveChild(member);

                            elements[elementIdx].ParentNode.RemoveChild(elements[elementIdx]);
                            removed++;
                        }
                    }
                    else
                    {
                        // Remove inherited protected members
                        member = elements[elementIdx].SelectSingleNode("memberdata[@visibility='family' or " +
                            "@visibility='family or assembly']");

                        if(member != null)
                        {
                            elements[elementIdx].ParentNode.RemoveChild(elements[elementIdx]);
                            removed++;
                        }
                    }

                builder.ReportProgress("    {0} protected members removed from sealed classes", removed);
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0050", String.Format(CultureInfo.CurrentCulture,
                    "Error removing protected members from sealed classes: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// This is used as the match evaluator for the regular expression that finds the &lt;element&gt; entries
        /// to remove from the reflection information file.
        /// </summary>
        /// <param name="m">The match found</param>
        /// <returns>The string with which to replace the match</returns>
        /// <remarks>The removals are done this way as it proved to be a very slow process to remove the child
        /// elements at the time they were found with an XPath query on very large files.</remarks>
        private string OnExcludeElement(Match m)
        {
            if(excludedMembers.Contains(m.Groups[1].Value) || excludedMembers.Contains(m.Groups[2].Value))
                return String.Empty;

            return m.Value;
        }
        #endregion

        #region Manual API filter methods
        //=====================================================================

        /// <summary>
        /// This is used to manually apply the specified API filter to the specified reflection information file
        /// </summary>
        /// <param name="filterToApply">The API filter to apply</param>
        /// <param name="reflectionFilename">The reflection information file</param>
        private void ApplyManualApiFilter(ApiFilterCollection filterToApply, string reflectionFilename)
        {
            XmlDocument refInfo;
            XmlNode apis;
            string id;
            bool keep;

            refInfo = new XmlDocument();
            refInfo.Load(reflectionFilename);
            apis = refInfo.SelectSingleNode("reflection/apis");

            foreach(ApiFilter nsFilter in filterToApply)
                if(nsFilter.Children.Count == 0)
                    this.RemoveNamespace(apis, nsFilter.FullName);
                else
                    if(!nsFilter.IsExposed)
                    {
                        // Remove all but the indicated types
                        foreach(XmlNode typeNode in apis.SelectNodes("api[starts-with(@id, 'T:') and containers/" +
                          "namespace/@api='N:" + nsFilter.FullName + "']"))
                        {
                            id = typeNode.Attributes["id"].Value.Substring(2);
                            keep = false;

                            foreach(ApiFilter typeFilter in nsFilter.Children)
                                if(typeFilter.FullName == id)
                                {
                                    // Just keep or remove members
                                    this.ApplyMemberFilter(apis, typeFilter);
                                    keep = true;
                                    break;
                                }

                            if(!keep)
                                this.RemoveType(apis, id);
                        }
                    }
                    else
                    {
                        // Remove just the indicated types or their members
                        foreach(ApiFilter typeFilter in nsFilter.Children)
                        {
                            if(!typeFilter.IsExposed && typeFilter.Children.Count == 0)
                            {
                                this.RemoveType(apis, typeFilter.FullName);
                                continue;
                            }

                            // Just keep or remove members
                            this.ApplyMemberFilter(apis, typeFilter);
                        }
                    }

            refInfo.Save(reflectionFilename);
        }

        /// <summary>
        /// Apply a member filter to the specified type.
        /// </summary>
        /// <param name="apis">The APIs node from which to remove info</param>
        /// <param name="typeFilter">The type filter to be processed</param>
        private void ApplyMemberFilter(XmlNode apis, ApiFilter typeFilter)
        {
            string id;
            bool keep;
            int pos;

            if(!typeFilter.IsExposed)
            {
                // Remove all but the indicated members
                foreach(XmlNode memberNode in apis.SelectNodes("api[containers/type/@api='T:" +
                  typeFilter.FullName + "']"))
                {
                    id = memberNode.Attributes["id"].Value.Substring(2);
                    pos = id.IndexOf('(');
                    keep = false;

                    // The API filter ignores parameters on methods
                    if(pos != -1)
                        id = id.Substring(0, pos);

                    foreach(ApiFilter memberFilter in typeFilter.Children)
                        if(memberFilter.FullName == id)
                        {
                            keep = true;
                            break;
                        }

                    if(!keep)
                    {
                        id = memberNode.Attributes["id"].Value;
                        builder.ReportProgress("    Removing member '{0}'", id);
                        memberNode.ParentNode.RemoveChild(memberNode);

                        // Remove the element nodes too
                        foreach(XmlNode element in apis.SelectNodes("api/elements/element[@api='" + id + "']"))
                            element.ParentNode.RemoveChild(element);
                    }
                }
            }
            else
            {
                // Remove just the indicated members
                foreach(ApiFilter memberFilter in typeFilter.Children)
                    foreach(XmlNode memberNode in apis.SelectNodes("api[starts-with(substring-after(@id,':'),'" +
                      memberFilter.FullName + "')]"))
                    {
                        id = memberNode.Attributes["id"].Value.Substring(2);
                        pos = id.IndexOf('(');

                        // The API filter ignores parameters on methods
                        if(pos != -1)
                            id = id.Substring(0, pos);

                        if(id == memberFilter.FullName)
                        {
                            id = memberNode.Attributes["id"].Value;
                            builder.ReportProgress("    Removing member '{0}'", id);
                            memberNode.ParentNode.RemoveChild(memberNode);

                            // Remove the element nodes too
                            foreach(XmlNode element in apis.SelectNodes("api/elements/element[@api='" + id + "']"))
                                element.ParentNode.RemoveChild(element);
                        }
                    }
            }
        }

        /// <summary>
        /// Remove an entire namespace and all of its members
        /// </summary>
        /// <param name="apis">The APIs node from which to remove info</param>
        /// <param name="id">The namespace ID to remove</param>
        private void RemoveNamespace(XmlNode apis, string id)
        {
            XmlNode ns;
            string nodeId;

            builder.ReportProgress("    Removing namespace 'N:{0}'", id);
            ns = apis.SelectSingleNode("api[@id='N:" + id + "']");

            if(ns != null)
            {
                // Remove the namespace container
                ns.ParentNode.RemoveChild(ns);

                // Remove all of the namespace members
                foreach(XmlNode xn in apis.SelectNodes("api[containers/namespace/@api='N:" + id + "']"))
                {
                    xn.ParentNode.RemoveChild(xn);

                    // Remove the element nodes too
                    nodeId = xn.Attributes["id"].Value;

                    foreach(XmlNode element in apis.SelectNodes("api/elements/element[@api='" + nodeId + "']"))
                        element.ParentNode.RemoveChild(element);
                }
            }
        }

        /// <summary>
        /// Remove an entire type and all of its members
        /// </summary>
        /// <param name="apis">The APIs node from which to remove info</param>
        /// <param name="id">The type ID to remove</param>
        private void RemoveType(XmlNode apis, string id)
        {
            XmlNode typeNode;
            string nodeId;

            builder.ReportProgress("    Removing type 'T:{0}'", id);
            typeNode = apis.SelectSingleNode("api[@id='T:" + id + "']");

            if(typeNode != null)
            {
                // Remove the namespace container
                typeNode.ParentNode.RemoveChild(typeNode);

                // Remove all of the type members
                foreach(XmlNode xn in apis.SelectNodes("api[containers/type/@api='T:" + id + "']"))
                {
                    xn.ParentNode.RemoveChild(xn);

                    // Remove the element nodes too
                    nodeId = xn.Attributes["id"].Value;

                    foreach(XmlNode element in apis.SelectNodes("api/elements/element[@api='" + nodeId + "']"))
                        element.ParentNode.RemoveChild(element);
                }

                // Remove namespace element nodes
                foreach(XmlNode element in apis.SelectNodes("api/elements/element[@api='T:" + id + "']"))
                    element.ParentNode.RemoveChild(element);
            }
        }
        #endregion
    }
}
