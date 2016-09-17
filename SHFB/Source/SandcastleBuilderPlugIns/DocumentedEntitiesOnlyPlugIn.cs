//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : DocumentedEntitiesOnlyPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/16/2016
// Note    : Copyright 2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in that can be used to automatically generate an API filter based on the XML
// comments member IDs to limit the help file content to only the documented entities.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/27/2016  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in is used to automatically generate an API filter based on the XML comments member IDs to
    /// limit the help file content to only the documented entities.
    /// </summary>
    /// <remarks><para>This plug-in is unique in that it runs both before and after the <c>GenerateReflectionInfo</c>
    /// step.  This is necessary as we must remove any project-based API filter and generate a full set of API
    /// information in order to compare it to the XML comments member IDs.  Once we have done that, we can
    /// generate an API filter and apply it by building the reflection information again.</para>
    /// 
    /// <para>Note that we are still constrained by the rules of the API filter.  Specifically, if you exclude
    /// one member of an overloaded method set, all of them will be excluded.  There is no getting around that
    /// rule.</para>
    /// 
    /// <para>In addition, since inherited documentation is generated after reflection data is generated,
    /// undocumented members of explicitly implemented interfaces will not be included.  If you want them
    /// included, you must explicitly add an <c>inheritdoc</c> tag on those members.</para>
    /// 
    /// <para>To explicitly exclude a documented member, add the <c>exclude</c> XML comment tag to its
    /// comments.  If placed on a type, it will exclude the entire type regardless of whether or not it has
    /// documented members.  If placed on a <c>NamespaceDoc</c> class, it will exclude all types within the
    /// namespace regardless of whether or not they are documented.  Note that namespace exclusions defined in
    /// the project namespace summaries will have no effect.  You must either add a <c>NamespaceDoc</c> class for
    /// the excluded namespace or exclude all of its types.</para></remarks>
    [HelpFileBuilderPlugInExport("Documented Entities Only", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright, Description = "This plug-in is used to automatically generate " +
        "an API filter based on the XML comments member IDs to limit the help file content to only the " +
        "documented entities.  Note that this will override any API filter defined in the project.")]
    public sealed class DocumentedEntitiesOnlyPlugIn : IPlugIn
    {
        #region Member documentation state information class
        //=====================================================================

        /// <summary>
        /// This is used to store the documentation state for the members
        /// </summary>
        private class DocumentationState
        {
            #region Private data members
            //=====================================================================

            private List<DocumentationState> members;

            #endregion

            #region Properties
            //=====================================================================

            /// <summary>
            /// This is used to get or set the full member ID (type:namespace[.type[.member]]).
            /// </summary>
            public string MemberId { get;  set; }

            /// <summary>
            /// This returns the member ID type (N = Namespace, T = Type, anything else = member)
            /// </summary>
            public char IdType
            {
                get { return this.MemberId[0]; }
            }

            /// <summary>
            /// This is used to get or set the namespace name
            /// </summary>
            public string NamespaceName { get; set; }

            /// <summary>
            /// This is used to get the full type name including the namespace
            /// </summary>
            public string FullTypeName { get; set; }

            /// <summary>
            /// This is used to get or set the type name if for types and their members
            /// </summary>
            public string TypeName { get; set; }

            /// <summary>
            /// This is used to get or set the member name for type members
            /// </summary>
            public string MemberName { get; set; }

            /// <summary>
            /// This is used to get or set whether or not the member is an auto-documented constructor or dispose
            /// method.
            /// </summary>
            /// <value>If true and no other members are documented, the type containing the auto-documented
            /// constructors and dispose methods will be excluded.</value>
            public bool IsAutoDocumented { get; set; }

            /// <summary>
            /// This is used to get or set the documented state
            /// </summary>
            public bool IsDocumented { get; set; }

            /// <summary>
            /// This is used to get whether or not the member is explicitly excluded via an <c>exclude</c>
            /// XML comments tag.
            /// </summary>
            /// <remarks>Namespaces that are explicitly excluded will exclude all of their contained types even
            /// if they are documented.  Types that are explicitly excluded will exclude all of their members
            /// even if they are documented.  If not explicitly included, the types/members will be included if
            /// documented even if their parent namespace/type is not documented.</remarks>
            public bool IsExplicitlyExcluded { get; set; }

            /// <summary>
            /// This returns the documentation state of the members for namespaces and types
            /// </summary>
            public List<DocumentationState> Members
            {
                get
                {
                    if(members == null)
                        members = new List<DocumentationState>();

                    return members;
                }
            }
            #endregion

            #region Methods
            //=====================================================================

            /// <inheritdoc />
            public override string ToString()
            {
                return this.MemberId;
            }

            /// <summary>
            /// This is used to get the member information from a reflection information file node
            /// </summary>
            /// <param name="member">The reflection information file node from which to obtain the details</param>
            public static DocumentationState FromApiMember(XPathNavigator member)
            {
                var docState = new DocumentationState();

                docState.MemberId = member.SelectSingleNode("@id").Value;

                if(String.IsNullOrWhiteSpace(docState.MemberId) || docState.MemberId.Length < 2 ||
                  docState.IdType == 'G' || docState.MemberId[1] != ':')
                    return docState;

                if(docState.IdType == 'N')
                    docState.NamespaceName = docState.MemberId.Substring(2);
                else
                {
                    docState.NamespaceName = member.SelectSingleNode("containers/namespace/@api").Value.Substring(2);

                    if(docState.IdType == 'T')
                        docState.FullTypeName = docState.MemberId.Substring(2);
                    else
                        docState.FullTypeName = member.SelectSingleNode("containers/type/@api").Value.Substring(2);

                    if(docState.NamespaceName.Length != 0)
                        docState.TypeName = docState.FullTypeName.Substring(docState.NamespaceName.Length + 1);
                    else
                        docState.TypeName = docState.FullTypeName;

                    if(docState.IdType != 'T')
                    {
                        string memberName = docState.MemberId.Substring(docState.FullTypeName.Length + 3);

                        // Strip off parameters and generic type counts from type members
                        int pos = memberName.IndexOf('(');

                        if(pos != -1)
                            memberName = memberName.Substring(0, pos);

                        pos = memberName.IndexOf("``");

                        if(pos != -1)
                            memberName = memberName.Substring(0, pos);

                        docState.MemberName = memberName.Replace('#', '.');
                    }
                }

                return docState;
            }
            #endregion
        }
        #endregion

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
                        // This plug-in is unique.  It is given the lowest priority to run last before the build
                        // step in order to remove any project-defined API filter.  It is given the highest
                        // priority to run first after the default processing in order to determine and apply the
                        // modified API filter based on what information is defined.
                        new ExecutionPoint(BuildStep.GenerateReflectionInfo, ExecutionBehaviors.Before, Int32.MinValue),
                        new ExecutionPoint(BuildStep.GenerateReflectionInfo, ExecutionBehaviors.After, Int32.MaxValue)
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
            MessageBox.Show("This plug-in has no configurable settings", "Build Process Plug-In",
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
            XDocument config;
            XElement currentFilter;

            if(context.Behavior == ExecutionBehaviors.Before)
            {
                // Having an API filter in the SHFB project is counterproductive.  Either use the API filter or
                // rely on the XML comments.  It's too complicated to try and use both since you can't reliably
                // resolve conflicts between what is included/excluded between the two.  Using the presence or
                // absence of the XML comments alone produces consistent results and avoids unexpected results.
                builder.ReportProgress("Removing any API filter defined in the project...");

                config = XDocument.Load(builder.WorkingFolder + "MRefBuilder.config");

                currentFilter = config.Root.Descendants("apiFilter").FirstOrDefault();

                if(currentFilter != null)
                    currentFilter.RemoveNodes();

                config.Save(builder.WorkingFolder + "MRefBuilder.config");

                return;
            }

            Dictionary<string, DocumentationState> memberDocState = new Dictionary<string, DocumentationState>();
            DocumentationState docState;
            string id;
            bool isAutoDocumented, originalTypeDocState, autoDocConstructors = builder.CurrentProject.AutoDocumentConstructors,
                autoDocDispose = builder.CurrentProject.AutoDocumentDisposeMethods, isDocumented;

            builder.ReportProgress("Determining the documentation state of members in the documented assemblies...");

            // To get everything documented correctly, we need to get a list of all assembly member IDs and then
            // match them up to documentation member IDs.
            var reflectionInfo = new XPathDocument(builder.ReflectionInfoFilename);
            var fileNav = reflectionInfo.CreateNavigator();

            foreach(XPathNavigator member in fileNav.Select("reflection/apis/api"))
            {
                docState = DocumentationState.FromApiMember(member);

                if(docState.NamespaceName != null)
                    memberDocState[docState.MemberId] = docState;
            }

            foreach(var file in builder.CommentsFiles)
                foreach(XmlNode member in file.Members)
                {
                    id = member.Attributes["name"].Value;

                    // Convert NamespaceDoc types to namespace comments element IDs
                    if(id.Length > 2 && id[0] == 'T' && id.EndsWith("NamespaceDoc", StringComparison.Ordinal))
                        id = "N:" + id.Substring(2, id.Length - 15);

                    if(memberDocState.TryGetValue(id, out docState))
                    {
                        // Note whether or not the member is explicitly excluded.  For namespaces and types, this
                        // will govern whether or not any of the children are documented even if the children
                        // have documentation.
                        if(member.SelectSingleNode("exclude") != null)
                        {
                            docState.IsExplicitlyExcluded = true;
                            docState.IsDocumented = false;
                        }
                        else
                        {
                            docState.IsExplicitlyExcluded = false;
                            docState.IsDocumented = true;
                        }

                        // Add or update entries for attached properties and events
                        if(id[0] == 'F')
                            if(member.SelectSingleNode("AttachedPropertyComments") != null &&
                                id.EndsWith("Property", StringComparison.Ordinal))
                            {
                                id = "P" + id.Substring(1, id.Length - 9);

                                docState = new DocumentationState
                                {
                                    MemberId = id,
                                    NamespaceName = docState.NamespaceName,
                                    FullTypeName = docState.FullTypeName,
                                    TypeName = docState.TypeName,
                                    MemberName = id.Substring(id.LastIndexOf('.') + 1),
                                    IsDocumented = docState.IsDocumented
                                };

                                memberDocState[id] = docState;
                            }
                            else
                                if(member.SelectSingleNode("AttachedEventComments") != null &&
                                    id.EndsWith("Event", StringComparison.Ordinal))
                                {
                                    id = "E" + id.Substring(1, id.Length - 6);

                                    docState = new DocumentationState
                                    {
                                        MemberId = id,
                                        NamespaceName = docState.NamespaceName,
                                        FullTypeName = docState.FullTypeName,
                                        TypeName = docState.TypeName,
                                        MemberName = id.Substring(id.LastIndexOf('.') + 1),
                                        IsDocumented = docState.IsDocumented
                                    };

                                    memberDocState[id] = docState;
                                }
                    }
                }

            var allNamespaces = memberDocState.Values.Where(m => m.IdType == 'N').ToList();
            var allTypes = memberDocState.Values.Where(m => m.IdType == 'T');
            var allMembers = memberDocState.Values.Where(m => m.IdType != 'N' && m.IdType != 'T');

            foreach(var ns in allNamespaces)
                foreach(var t in allTypes.Where(t => t.NamespaceName == ns.NamespaceName))
                {
                    ns.Members.Add(t);

                    foreach(var m in allMembers.Where(am => am.FullTypeName == t.FullTypeName))
                        t.Members.Add(m);
                }

            builder.ReportProgress("Building API filter based on the XML comments members...");

            // First, see if types and namespaces need to be excluded
            foreach(var namespaceDoc in allNamespaces)
            {
                if(namespaceDoc.IsExplicitlyExcluded)
                    namespaceDoc.IsDocumented = false;
                else
                {
                    foreach(var typeDoc in namespaceDoc.Members)
                    {
                        if(typeDoc.IsExplicitlyExcluded)
                            typeDoc.IsDocumented = false;
                        else
                        {
                            foreach(var memberGroup in typeDoc.Members.GroupBy(m => m.MemberName))
                            {
                                isAutoDocumented = false;

                                // Auto-document static constructors if wanted.  Only auto-document parameterless
                                // constructors if wanted and if there are no other constructors or if all other
                                // constructors are documented as well.  Only auto-document the standard dispose methods
                                // if wanted.  For all others, base it on whether or not any single method is excluded
                                // which is how the API filter works.  If one overload is excluded, they are all excluded.
                                if(memberGroup.Key == ".cctor" && autoDocConstructors)
                                {
                                    isAutoDocumented = true;
                                    isDocumented = !memberGroup.First().IsExplicitlyExcluded;
                                }
                                else
                                    if(memberGroup.Key == ".ctor" && autoDocConstructors)
                                    {
                                        isAutoDocumented = true;
                                        isDocumented = (memberGroup.All(m => !m.IsExplicitlyExcluded) &&
                                            memberGroup.Where(m => m.MemberId.IndexOf('(') != -1).All(m => m.IsDocumented));
                                    }
                                    else
                                        if(memberGroup.Key == "Dispose" && autoDocDispose)
                                        {
                                            isAutoDocumented = true;
                                            isDocumented = memberGroup.All(m => !m.IsExplicitlyExcluded &&
                                                (m.MemberId.EndsWith(".Dispose", StringComparison.Ordinal) ||
                                                m.MemberId.EndsWith(".Dispose(System.Boolean)", StringComparison.Ordinal)));
                                        }
                                        else
                                            isDocumented = memberGroup.All(m => m.IsDocumented);

                                foreach(var m in memberGroup)
                                {
                                    m.IsAutoDocumented = isAutoDocumented;
                                    m.IsDocumented = isDocumented;
                                }
                            }

                            // Document the type if any members are documented or if it is documented but has no members
                            originalTypeDocState = typeDoc.IsDocumented;

                            typeDoc.IsDocumented = (typeDoc.Members.Any(m => m.IsDocumented) ||
                                (originalTypeDocState && typeDoc.Members.Count == 0));

                            // One exception is if the only members documented are the auto-documented
                            // constructors and dispose methods.  In that case, exclude the type unless the
                            // type has documentation as indicated by the original state.
                            if(!originalTypeDocState && typeDoc.IsDocumented &&
                              (typeDoc.Members.Count(m => !m.IsAutoDocumented) != 0 ||
                              typeDoc.Members.All(m => m.IsAutoDocumented)) &&
                              !typeDoc.Members.Any(m => !m.IsAutoDocumented && m.IsDocumented))
                                typeDoc.IsDocumented = false;
                        }
                    }

                    namespaceDoc.IsDocumented = namespaceDoc.Members.Any(m => m.IsDocumented);
                }
            }

            // The root filter element must set the expose attribute to true or it rips all inherited member
            // information which prevents inherited documentation from working among other things.
            //
            // The namespace elements set the expose attribute to false if they contain no documented types thus
            // excluding everything.  If they contain documented types, it will be set to true.
            //
            // The type elements set the expose attribute to false if they contain no documented members thus
            // excluding everything.  If they contain documented members, it will be set to true.
            //
            // The member elements are always set to false as we only add the undocumented members to exclude
            // from the type.  Documented members are included by default since the type member is exposed.
            var apiFilter = new XElement("apiFilter", new XAttribute("expose", true));

            foreach(var namespaceDoc in allNamespaces.OrderBy(n => n.NamespaceName))
            {
                var ns = new XElement("namespace", new XAttribute("name", namespaceDoc.NamespaceName),
                    new XAttribute("expose", namespaceDoc.IsDocumented));
                apiFilter.Add(ns);

                // Only add documented types if the namespace is documented
                if(namespaceDoc.IsDocumented)
                    foreach(var typeDoc in namespaceDoc.Members.OrderBy(m => m.TypeName))
                    {
                        // Only add types if not documented or one or more members are not documented
                        if(!typeDoc.IsDocumented || !typeDoc.Members.All(m => m.IsDocumented))
                        {
                            var t = new XElement("type", new XAttribute("name", typeDoc.TypeName),
                                new XAttribute("expose", typeDoc.IsDocumented));
                            ns.Add(t);

                            // Only add undocumented members if the type is documented.  Note that we group by
                            // member name.  The way the API filter works is by name alone.  If one overload of a
                            // set is excluded, all of them are.
                            if(typeDoc.IsDocumented)
                                foreach(var memberDoc in typeDoc.Members.OrderBy(m => m.MemberName).GroupBy(m => m.MemberName))
                                    if(!memberDoc.First().IsDocumented)
                                        t.Add(new XElement("member", new XAttribute("name", memberDoc.Key),
                                            new XAttribute("expose", false)));
                        }
                    }
            }

            config = XDocument.Load(builder.WorkingFolder + "MRefBuilder.config");

            currentFilter = config.Root.Descendants("apiFilter").FirstOrDefault();

            if(currentFilter != null)
                currentFilter.ReplaceWith(apiFilter);
            else
                config.Root.Element("dduetools").Add(apiFilter);

            config.Save(builder.WorkingFolder + "MRefBuilder.config");

            builder.ReportProgress("Regenerating reflection information based on the documented members API filter...");

            // Silverlight build targets are only available for 32-bit builds regardless of the framework
            // version and require the 32-bit version of MSBuild in order to load the target file correctly.
            if(builder.CurrentProject.FrameworkVersion.StartsWith("Silverlight", StringComparison.OrdinalIgnoreCase))
                builder.TaskRunner.Run32BitProject("GenerateRefInfo.proj", false);
            else
                builder.TaskRunner.RunProject("GenerateRefInfo.proj", false);
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the plug-in if not done explicitly
        /// with <see cref="Dispose()"/>.
        /// </summary>
        ~DocumentedEntitiesOnlyPlugIn()
        {
            this.Dispose();
        }

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of the plug-in object
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
