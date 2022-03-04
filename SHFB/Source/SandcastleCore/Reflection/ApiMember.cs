//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ApiMember.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/16/2022
// Note    : Copyright 2021-2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to contain information about an API member entry in a reflection
// information file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/03/2021  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.Reflection
{
    /// <summary>
    /// This is used to contain information about an API member entry in a reflection information file
    /// </summary>
    public class ApiMember
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the full API member ID including its prefix
        /// </summary>
        public string MemberId { get; }

        /// <summary>
        /// This read-only property returns the API member ID excluding the leading prefix
        /// </summary>
        public string MemberIdWithoutPrefix { get; }

        /// <summary>
        /// This read-only property returns the API member ID excluding the parameters (methods only)
        /// </summary>
        public string MemberIdWithoutParameters { get; }

        /// <summary>
        /// This read-only property returns the member name without the namespace or parameters
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// This read-only property returns the member's API group
        /// </summary>
        public ApiMemberGroup ApiGroup { get; }

        /// <summary>
        /// This read-only property returns the member's API subgroup
        /// </summary>
        public ApiMemberGroup ApiSubgroup { get; }

        /// <summary>
        /// This read-only property returns the member's API sub-subgroup
        /// </summary>
        public ApiMemberGroup ApiSubSubgroup { get; }

        /// <summary>
        /// This read-only property returns the API topic group
        /// </summary>
        public ApiMemberGroup ApiTopicGroup { get; }

        /// <summary>
        /// This read-only property returns the API topic subgroup
        /// </summary>
        public ApiMemberGroup ApiTopicSubgroup { get; }

        /// <summary>
        /// This read-only property returns the API topic subgroup
        /// </summary>
        public ApiMemberGroup ApiTopicSubSubgroup { get; }

        /// <summary>
        /// This read-only property returns the member's topic name
        /// </summary>
        public string TopicName { get; }

        /// <summary>
        /// This read-only property returns the member's topic name for explicit interface implementations
        /// </summary>
        public string TopicEiiName { get; }

        /// <summary>
        /// This read-only property returns the member's topic group
        /// </summary>
        public ApiMemberGroup TopicGroup { get; }

        /// <summary>
        /// This read-only property returns the member's topic subgroup
        /// </summary>
        public ApiMemberGroup TopicSubgroup { get; }

        /// <summary>
        /// This read-only property returns the member's topic sub-subgroup
        /// </summary>
        public ApiMemberGroup TopicSubSubgroup { get; }

        /// <summary>
        /// This read-only property returns the member's type topic ID
        /// </summary>
        /// <remarks>This appears on member list topics</remarks>
        public string TypeTopicId { get; }

        /// <summary>
        /// This read-only property returns the member's type API subgroup
        /// </summary>
        /// <remarks>This appears on member list topics</remarks>
        public string TypeApiSubgroup { get; }

        /// <summary>
        /// This read-only property returns the topic filename
        /// </summary>
        public string TopicFilename { get; }

        /// <summary>
        /// This read-only property returns the overload topic ID if the member is overloaded
        /// </summary>
        public string OverloadTopicId { get; }

        /// <summary>
        /// This read-only property is used to get the parameter count for methods
        /// </summary>
        /// <remarks>This is used for sorting overload sets</remarks>
        public int ParameterCount { get; }

        /// <summary>
        /// This read-only property is used to get the type of the first parameter
        /// </summary>
        /// <remarks>This is used for sorting overload sets</remarks>
        public string FirstParameterTypeName { get; }

        /// <summary>
        /// This read-only property returns whether or not the member is an explicit interface member implementation
        /// </summary>
        public bool IsExplicitlyImplemented { get; }

        /// <summary>
        /// This read-only property is used to get the type of the member if explicitly implemented
        /// </summary>
        public string ImplementedType { get; }

        /// <summary>
        /// This read-only property returns an enumerable list of child element IDs for those topics that have them
        /// </summary>
        public IEnumerable<string> ChildElements { get; }

        /// <summary>
        /// The raw XML for the API entry
        /// </summary>
        /// <remarks>This is only used when updating the document model</remarks>
        public XElement Node { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiMember">The XML element containing the reflection information for the API member</param>
        /// <param name="key">An optional key to use as the member ID if the API member element does not contain it</param>
        public ApiMember(XElement apiMember, string key)
        {
            if(apiMember == null)
                throw new ArgumentNullException(nameof(apiMember));

            var apiData = apiMember.Element("apidata");
            var apiGroup = apiData?.Attribute("group");
            var apiSubgroup = apiData?.Attribute("subgroup");
            var apiSubSubgroup = apiData?.Attribute("subsubgroup");
            var topicData = apiMember.Element("topicdata");
            var topicGroup = topicData?.Attribute("group");
            var topicSubgroup = topicData?.Attribute("subgroup");
            var topicSubSubgroup = topicData?.Attribute("subsubgroup");
            var procedureData = apiMember.Element("proceduredata");
            var memberData = apiMember.Element("memberdata");

            this.MemberId = key ?? (apiMember.Attribute("id") ?? apiMember.Attribute("api"))?.Value;
            this.MemberIdWithoutPrefix = this.MemberId?.Substring(this.MemberId.IndexOf(':') + 1);
            this.Name = apiData?.Attribute("name").Value;
            this.TopicName = topicData?.Attribute("name")?.Value;
            this.TopicEiiName = topicData?.Attribute("eiiName")?.Value;
            this.TopicFilename = apiMember.Element("file")?.Attribute("name")?.Value;
            this.OverloadTopicId = memberData?.Attribute("overload")?.Value;

            this.IsExplicitlyImplemented = (procedureData?.Attribute("eii")?.Value ?? String.Empty).Equals(
                "true", StringComparison.OrdinalIgnoreCase);

            if(this.IsExplicitlyImplemented)
            {
                this.ImplementedType = apiMember.Element("implements").Element("member").Element(
                    "type").Attribute("api").Value;
            }

            int pos = this.MemberId.IndexOf('(');

            if(pos != -1)
                this.MemberIdWithoutParameters = this.MemberId.Substring(0, pos);
            else
                this.MemberIdWithoutParameters = this.MemberId;

            ApiMemberGroup g;

            if(apiGroup != null)
            {
                if(Enum.TryParse(apiGroup.Value, true, out g))
                    this.ApiGroup = g;
                else
                    this.ApiGroup = ApiMemberGroup.Unknown;
            }

            if(apiSubgroup != null)
            {
                if(Enum.TryParse(apiSubgroup.Value, true, out g))
                    this.ApiSubgroup = g;
                else
                    this.ApiSubgroup = ApiMemberGroup.Unknown;
            }
            else
            {
                if(this.MemberId.StartsWith("Overload:", StringComparison.Ordinal))
                {
                    this.ApiSubgroup = this.MemberId.Contains(".#ctor") ? ApiMemberGroup.Constructor :
                        ApiMemberGroup.Method;
                    this.ApiSubSubgroup = ApiMemberGroup.Overload;
                }
            }

            if(apiSubSubgroup != null)
            {
                if(Enum.TryParse(apiSubSubgroup.Value, true, out g))
                    this.ApiSubSubgroup = g;
                else
                    this.ApiSubSubgroup = ApiMemberGroup.Unknown;
            }

            if(topicGroup != null)
            {
                if(Enum.TryParse(topicGroup.Value, true, out g))
                    this.TopicGroup = g;
                else
                    this.TopicGroup = ApiMemberGroup.Unknown;
            }

            if(topicSubgroup != null)
            {
                if(Enum.TryParse(topicSubgroup.Value, true, out g))
                    this.TopicSubgroup = g;
                else
                    this.TopicSubgroup = ApiMemberGroup.Unknown;
            }

            if(topicSubSubgroup != null)
            {
                if(Enum.TryParse(topicSubSubgroup.Value, true, out g))
                    this.TopicSubSubgroup = g;
                else
                    this.TopicSubSubgroup = ApiMemberGroup.Unknown;
            }

            this.ApiTopicGroup = (this.TopicGroup == ApiMemberGroup.Api) ? this.ApiGroup : this.TopicGroup;
            this.ApiTopicSubgroup = (this.TopicGroup == ApiMemberGroup.Api) ? this.ApiSubgroup : this.TopicSubgroup;
            this.ApiTopicSubSubgroup = (this.TopicGroup == ApiMemberGroup.Api) ? this.ApiSubSubgroup : this.TopicSubSubgroup;

            if(this.TopicGroup == ApiMemberGroup.Api && this.ApiGroup == ApiMemberGroup.Type)
            {
                this.TypeTopicId = this.MemberId;
                this.TypeApiSubgroup = apiData.Attribute("subgroup")?.Value;
            }
            else
            {
                if(topicData?.Attribute("typeTopicId") != null)
                {
                    this.TypeTopicId = topicData.Attribute("typeTopicId").Value;
                    this.TypeApiSubgroup = apiData.Attribute("subgroup")?.Value;
                }
                else
                {
                    this.TypeTopicId = apiMember.Element("containers")?.Element("type")?.Attribute("api")?.Value;
                    this.TypeApiSubgroup = apiMember.Element("containers")?.Element("type")?.Element("apidata")?.Attribute("subgroup")?.Value;
                }
            }

            var parameters = apiMember.Element("parameters");

            if(parameters != null)
            {
                this.ParameterCount = parameters.Elements("parameter").Count();

                var firstParameter = parameters.Element("parameter");
                var parameterType = firstParameter.Descendants("type").FirstOrDefault();

                if(parameterType == null)
                    parameterType = firstParameter.Descendants("template").FirstOrDefault();

                this.FirstParameterTypeName = parameterType.Attribute("api").Value;

                pos = this.FirstParameterTypeName.LastIndexOf('.');

                if(pos != -1)
                    this.FirstParameterTypeName = this.FirstParameterTypeName.Substring(pos + 1);
                else
                    this.FirstParameterTypeName = this.FirstParameterTypeName.Substring(2);
            }

            var elements = apiMember.Element("elements");

            // Any child elements are sorted by their member ID in ascending order excluding the prefix
            if(elements == null)
                this.ChildElements = Enumerable.Empty<string>();
            else
                this.ChildElements = elements.Elements("element").Select(e => e.Attribute("api").Value).OrderBy(
                    id => id.Substring(id.IndexOf(':') + 1)).ToList();
        }
        #endregion
    }
}
