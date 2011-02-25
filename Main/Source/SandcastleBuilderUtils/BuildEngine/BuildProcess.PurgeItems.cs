//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildProcess.PurgeItems.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/15/2011
// Note    : Copyright 2006-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the code used to purge duplicate topics and other items
// from the Sandcastle reflection.org file based on the project's Visibility
// category properties.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  08/05/2006  EFW  Created the code
// 1.3.1.1  10/05/2006  EFW  Implemented the Visibility category properties
// 1.3.3.1  12/11/2006  EFW  Added support for <exclude/>
// 1.5.0.2  07/19/2007  EFW  <exclude/> is now handled by the MRefBuilder
//                           namespace ripping feature.
// 1.5.1.0  08/24/2007  EFW  Added support for the inherited private/internal
//                           framework member flags.
// 1.5.2.0  09/13/2007  EFW  Added support for calling plug-ins
// 1.6.0.7  03/24/2008  EFW  Removed the PurgeDuplicateTopics build step
// 1.8.0.1  12/14/2008  EFW  Added support for ExtensionAttribute
//=============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

using SandcastleBuilder.Utils.PlugIn;

namespace SandcastleBuilder.Utils.BuildEngine
{
    partial class BuildProcess
    {
        #region Private data members

        // A set of members that need to be excluded from the reflection
        // information based on the visibility settings.
        private HashSet<string> excludedMembers;

        private static Regex reExcludeElementEntry = new Regex(
            "<element api=\"([^\n\"]+?)\">.*?</element>|<element api=\"([^\n\"]+?)\" />",
            RegexOptions.Singleline);

        private MatchEvaluator excludeElementEval;
        #endregion

        /// <summary>
        /// Apply the project's Visibility category properties to the
        /// reflection information file.
        /// </summary>
        /// <remarks>This is used to remove entries from the reflection
        /// information file so that it does not appear in the help file.
        /// See the <b>Document*</b> properties in the
        /// <see cref="SandcastleProject"/> class for information on the
        /// items removed.</remarks>
        private void ApplyVisibilityProperties()
        {
            XmlNodeList apis;
            XmlNode api;
            MatchCollection matches;
            string xpath;

            this.ReportProgress(BuildStep.ApplyVisibilityProperties,
                "Applying visibility properties to reflection information file");

            if(this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                return;

            this.ExecutePlugIns(ExecutionBehaviors.Before);

            // The reflection file can contain tens of thousands of entries
            // for large assemblies.  Removing the elements at the time they
            // were found proved to be too slow for such large files.  As such,
            // we will get a list of all api and element entries to be
            // removed and we'll remove them using regular expressions and a
            // match evaluator at the end of this method.
            excludedMembers = new HashSet<string>();

            try
            {
                // As of version 1.5.0.2, removal of excluded members is
                // handled by the MRefBuilder namespace ripping feature so
                // we don't have to deal with them here.

                // Apply the visibility properties
                if(!project.DocumentAttributes)
                    this.RemoveAttributes();

                if(!project.DocumentExplicitInterfaceImplementations)
                    this.RemoveExplicitInterfaceImplementations();

                // Determine if any inherited members need to be removed
                if(!project.DocumentInheritedMembers ||
                  !project.DocumentInheritedFrameworkMembers ||
                  (!project.DocumentInheritedFrameworkInternalMembers &&
                  (project.DocumentInternals || project.DocumentPrivates)) ||
                  (!project.DocumentInheritedFrameworkPrivateMembers &&
                  (project.DocumentInternals || project.DocumentPrivates)))
                    this.RemoveInheritedMembers();

                // If both privates and internals are not shown, they won't be
                // there as the /interal+ parameter will not have been used.
                // If privates are omitted, private fields are omitted by
                // default too.
                if(project.DocumentPrivates || project.DocumentInternals)
                {
                    if(!project.DocumentPrivates)
                    {
                        // Remove all private members excluding EII entries
                        // which are controlled by the Document EII property.
                        this.RemoveMembers("api[memberdata/@visibility=" +
                            "'private' and (not(proceduredata) or " +
                            "proceduredata/@virtual='false')]",
                            "private members");

                        // Remove all private types
                        this.RemoveMembers("api[typedata/@visibility='private']",
                            "private types");
                    }
                    else
                    {
                        if(!project.DocumentPrivateFields)
                        {
                            this.RemoveMembers("api[apidata/@subgroup='field' " +
                                "and memberdata/@visibility='private']",
                                "private fields");

                            // This will get rid of a load of base framework
                            // class fields too.
                            this.RemoveMembers("api/elements/element[" +
                                "starts-with(@api, 'F:System.') or " +
                                "starts-with(@api, 'F:Microsoft.')]",
                                "private framework fields");
                        }
                        else
                        {
                            // Remove backing fields that correspond to events.
                            // These can never be documented and should not
                            // show up.
                            this.RemoveMembers("api[apidata/@subgroup='field' " +
                                "and contains(returns/type/@api, 'EventHandler')]",
                                "event backer fields");
                         }

                        if(!project.DocumentInternals)
                        {
                            // Remove internal members
                            this.RemoveMembers("api[memberdata/@visibility=" +
                                "'assembly' or memberdata/@visibility=" +
                                "'family and assembly']", "internal members");

                            // Remove internal types
                            this.RemoveMembers("api[typedata/@visibility=" +
                                "'assembly' or typedata/@visibility='family " +
                                "and assembly']", "internal types");
                        }
                    }
                }

                if(!project.DocumentProtected)
                {
                    xpath = "memberdata/@visibility='family'";

                    // If DocumentInternals is false, remove protected internal
                    // members as well. If not, leave them alone.
                    if(!project.DocumentInternals)
                        xpath += " or memberdata/@visibility='family or " +
                            "assembly'";

                    this.RemoveMembers("api[" + xpath + "]",
                        "protected members");

                    // Remove protected inherited framework members too
                    this.RemoveMembers("api/elements/element[" +
                        "(starts-with(substring-after(@api,':'),'System.') " +
                        "or starts-with(substring-after(@api,':')," +
                        "'Microsoft.')) and (" + xpath + ")]",
                        "protected inherited framework members");
                }
                else
                {
                    if(project.DocumentProtectedInternalAsProtected)
                        this.ModifyProtectedInternalVisibility();

                    if(!project.DocumentSealedProtected)
                        this.RemoveSealedProtected();
                }

                // Now remove unwanted members if any were found
                if(excludedMembers.Count != 0)
                {
                    this.ReportProgress("    Removing previously noted unwanted APIs and elements");
                    apis = apisNode.SelectNodes("*");

                    for(int idx = 0; idx < apis.Count; idx++)
                    {
                        api = apis[idx];

                        if(!excludedMembers.Contains(api.Attributes["id"].Value))
                        {
                            // An XPath sub-query for the <element> entries is
                            // way to slow especially on very large files so
                            // we'll use a regular expression and a match
                            // evaluator to get rid of unwanted <element>
                            // entries.
                            matches = reExcludeElementEntry.Matches(api.InnerXml);

                            // However, just doing a straight replace consumes
                            // a large amount of memory.  As such, check the
                            // results for those that contain an unwanted
                            // element and only do it when really necessary.
                            foreach(Match m in matches)
                                if(excludedMembers.Contains(m.Groups[1].Value) ||
                                  excludedMembers.Contains(m.Groups[2].Value))
                                {
                                    api.InnerXml = reExcludeElementEntry.Replace(
                                        api.InnerXml, excludeElementEval);
                                    break;
                                }
                        }
                        else
                            api.ParentNode.RemoveChild(api);
                    }

                    excludedMembers = null;
                }

                // Backup the original for reference and save the changed file
                File.Copy(reflectionFile, Path.ChangeExtension(reflectionFile, ".bak"), true);
                reflectionInfo.Save(reflectionFile);
            }
            catch(BuilderException )
            {
                // If it's from one of the other methods, just rethrow it
                throw;
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0044",
                    "Error applying Visibility properties: " + ex.Message, ex);
            }

            this.ExecutePlugIns(ExecutionBehaviors.After);
        }

        /// <summary>
        /// Remove attribute information
        /// </summary>
        private void RemoveAttributes()
        {
            XmlNodeList elements;
            XmlNode typeNode, parent;
            int count = 0;

            // These attributes are always kept as they provide useful information or are used by
            // other components.  SerializableAttribute is also needed but it is an XML attribute
            // in the apis/api/typedata node rather than an actual attribute type entry so we can
            // ignore it here.
            HashSet<string> keepAttrs = new HashSet<string>
            {
                "T:System.FlagsAttribute",
                "T:System.ObsoleteAttribute",
                "T:System.Runtime.CompilerServices.ExtensionAttribute",

                // AjaxDoc/Script# attributes
                "T:System.AttachedPropertyAttribute",
                "T:System.IgnoreNamespaceAttribute",
                "T:System.IntrinsicPropertyAttribute",
                "T:System.NonScriptableAttribute",
                "T:System.PreserveCaseAttribute",
                "T:System.RecordAttribute"
            };

            try
            {
                // Find all of the attribute nodes
                elements = apisNode.SelectNodes("api/attributes/attribute");

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

                this.ReportProgress("    {0} attribute nodes removed", count);
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0045", "Error removing attributes nodes: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Remove explicit interface implementation information
        /// </summary>
        private void RemoveExplicitInterfaceImplementations()
        {
            XmlNodeList elements;
            int idx;

            try
            {
                // Find all explicit implementations in the documented assemblies
                elements = apisNode.SelectNodes("api[memberdata/@visibility=" +
                    "'private' and proceduredata/@virtual='true']");

                for(idx = 0; idx < elements.Count; idx++)
                    elements[idx].ParentNode.RemoveChild(elements[idx]);

                this.ReportProgress("    {0} explicit interface " +
                    "implementations removed", elements.Count);

                // Get rid of the matching element entries and those for base
                // classes too.  This ensures they don't show up in the
                // "class members" help page.
                elements = apisNode.SelectNodes("api/elements/element[" +
                    "contains(@api, '#') and not(contains(@api, '.#ctor')) " +
                    "and not(contains(@api, '.#cctor')) and " +
                    "not(contains(@api, ':J#'))]");

                for(idx = 0; idx < elements.Count; idx++)
                    elements[idx].ParentNode.RemoveChild(elements[idx]);

                this.ReportProgress("    {0} local and base class EII " +
                    "elements removed", elements.Count);
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0046",
                    "Error removing EII nodes: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Remove inherited member information from each type's element list
        /// based on the project settings.
        /// </summary>
        private void RemoveInheritedMembers()
        {
            XmlNodeList types, elements;
            int typeIdx, elementIdx, removed = 0;
            string typeName, query;
            bool frameworkOnly = project.DocumentInheritedMembers,
                 frameworkAll = project.DocumentInheritedFrameworkMembers,
                 frameworkInternal = project.DocumentInheritedFrameworkInternalMembers,
                 frameworkPrivate = project.DocumentInheritedFrameworkPrivateMembers;

            try
            {
                // Find all types
                types = apisNode.SelectNodes("api[starts-with(@id, 'T:')]");

                for(typeIdx = 0; typeIdx < types.Count; typeIdx++)
                {
                    // Only remove inherited Framework members?
                    if(frameworkOnly)
                    {
                        query = "elements/element[(starts-with(substring-after(" +
                            "@api,':'),'System.') or starts-with(" +
                            "substring-after(@api,':'),'Microsoft.'))";

                        // If just removing privates and/or internals, add the
                        // necessary additional filter conditions.
                        if(frameworkAll)
                            if(!frameworkPrivate && !frameworkInternal)
                                query += " and (memberdata/@visibility = " +
                                    "'private' or memberdata/@visibility = " +
                                    "'assembly' or memberdata/@visibility = " +
                                    "'family and assembly')";
                            else
                                if(!frameworkPrivate)
                                    query += " and memberdata/@visibility = " +
                                        "'private'";
                                else
                                    query += " and (memberdata/@visibility = " +
                                        "'assembly' or memberdata/@visibility = " +
                                        "'family and assembly')";

                        query += "]";

                        elements = types[typeIdx].SelectNodes(query);
                    }
                    else
                    {
                        // Get rid of any member not starting with this entry's
                        // type.  This includes all framework members too.
                        typeName = types[typeIdx].Attributes[
                            "id"].Value.Substring(2);
                        elements = types[typeIdx].SelectNodes(
                            "elements/element[not(starts-with(" +
                            "substring-after(@api,':'),'" + typeName + "'))]");
                    }

                    for(elementIdx = 0; elementIdx < elements.Count; elementIdx++)
                    {
                        elements[elementIdx].ParentNode.RemoveChild(elements[elementIdx]);
                        removed++;
                    }
                }

                this.ReportProgress("    {0} inherited member elements removed", removed);
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0047",
                    "Error removing inherited members: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Remove member information matching the specified XPath query.
        /// </summary>
        /// <param name="xpath">The XPath query used to find the members.</param>
        /// <param name="memberDesc">A description of the members removed.</param>
        /// <returns>The number of members to be removed</returns>
        /// <remarks>Actual removal of the members is deferred.  On very large
        /// files, the XPath queries took to long when removing the
        /// &lt;elemen&gt; members.</remarks>
        private int RemoveMembers(string xpath, string memberDesc)
        {
            XmlNodeList apis;
            int count = 0;
            string id;

            try
            {
                // Find all matching members
                apis = apisNode.SelectNodes(xpath);

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
                    this.ReportProgress("    {0} {1} noted for removal", count, memberDesc);
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
        private void ModifyProtectedInternalVisibility()
        {
            XmlNodeList members;

            try
            {
                // Find all matching members
                members = apisNode.SelectNodes("api/memberdata[" +
                    "@visibility='family or assembly']");

                foreach(XmlNode n in members)
                    n.Attributes["visibility"].Value = "family";

                this.ReportProgress("    {0} 'protected internal' members " +
                    "changed to 'protected'", members.Count);
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0049",
                    String.Format(CultureInfo.CurrentCulture,
                    "Error changing protected internal to protected: {0}",
                    ex.Message), ex);
            }
        }

        /// <summary>
        /// Remove protected members from sealed classes.
        /// </summary>
        private void RemoveSealedProtected()
        {
            XmlNodeList elements;
            XmlNode member;
            int elementIdx, removed = 0;
            string elementType, memberType;

            try
            {
                // Find all matching members
                elements = apisNode.SelectNodes("api[apidata/@subgroup != " +
                    "'enumeration' and typedata/@sealed = 'true']/elements/*");

                for(elementIdx = 0; elementIdx < elements.Count; elementIdx++)
                    if(!elements[elementIdx].HasChildNodes)
                    {
                        member = apisNode.SelectSingleNode("api[@id='" +
                            elements[elementIdx].Attributes["api"].Value +
                            "' and (memberdata/@visibility='family' or " +
                            "memberdata/@visibility='family or assembly')]");

                        if(member != null)
                        {
                            // If the type matches, delete the member too
                            elementType = elements[
                                elementIdx].ParentNode.ParentNode.Attributes[
                                "id"].Value.Substring(2);
                            memberType = member.Attributes["id"].Value.Substring(2);
                            memberType = memberType.Substring(0,
                                memberType.LastIndexOf('.'));

                            if(memberType == elementType)
                                member.ParentNode.RemoveChild(member);

                            elements[elementIdx].ParentNode.RemoveChild(
                                elements[elementIdx]);
                            removed++;
                        }
                    }
                    else
                    {
                        // Remove inherited protected members
                        member = elements[elementIdx].SelectSingleNode(
                            "memberdata[@visibility='family' or " +
                            "@visibility='family or assembly']");

                        if(member != null)
                        {
                            elements[elementIdx].ParentNode.RemoveChild(
                                elements[elementIdx]);
                            removed++;
                        }
                    }

                this.ReportProgress("    {0} protected members removed from " +
                    "sealed classes", removed);
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0050",
                    String.Format(CultureInfo.CurrentCulture,
                    "Error removing protected members from sealed classes: {0}",
                    ex.Message), ex);
            }
        }

        /// <summary>
        /// This is used as the match evaluator for the regular expression
        /// that finds the &lt;element&gt; entries to remove from the
        /// reflection information file.
        /// </summary>
        /// <param name="m">The match found</param>
        /// <returns>The string with which to replace the match</returns>
        /// <remarks>The removals are done this way as it proved to be a very
        /// slow process to remove the child elements at the time they
        /// were found with an XPath query on very large files.</remarks>
        private string OnExcludeElement(Match m)
        {
            if(excludedMembers.Contains(m.Groups[1].Value) || excludedMembers.Contains(m.Groups[2].Value))
                return String.Empty;

            return m.Value;
        }
    }
}
