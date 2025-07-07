//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : TopicType.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/02/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to contain topic type information
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/28/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This class is used to contain information about the topic types that can appear in a help file
    /// </summary>
    public sealed class TopicType
    {
        #region Private data members
        //=====================================================================

        private static readonly List<TopicType> topicTypes =
        [
            new("1FE70836-AA7D-4515-B54B-E10C4B516E50", "developerConceptualDocument",
                TopicTypeGroup.Concepts, TopicTypeGroup.Concepts),
            new("B137C930-7BF7-48A2-A329-3ADCAEF8868E", "developerOrientationDocument",
                TopicTypeGroup.Concepts, TopicTypeGroup.OtherResources),
            new("68F07632-C4C5-4645-8DFA-AC87DCB4BD54", "developerSDKTechnologyOverviewArchitectureDocument",
                TopicTypeGroup.Concepts, TopicTypeGroup.Concepts),
            new("CDB8C120-888F-447B-8AF8-F9540562E7CA", "developerSDKTechnologyOverviewOrientationDocument",
                TopicTypeGroup.Concepts, TopicTypeGroup.OtherResources),
            new("356C57C4-384D-4AF2-A637-FDD6F088A033", "developerSDKTechnologyOverviewScenariosDocument",
                TopicTypeGroup.Concepts, TopicTypeGroup.Concepts),
            new("19F1BB0E-F32A-4D5F-80A9-211D92A8A715", "developerSDKTechnologyOverviewTechnologySummaryDocument",
                TopicTypeGroup.Concepts, TopicTypeGroup.Concepts),
            new("56DB00EC-28BA-4C0D-8694-28E8B244E236", "developerWhitePaperDocument",
                TopicTypeGroup.Concepts, TopicTypeGroup.OtherResources),
            new("DAC3A6A0-C863-4E5B-8F65-79EFC6A4BA09", "developerHowToDocument",
                TopicTypeGroup.HowTo, TopicTypeGroup.Tasks),
            new("4779DD54-5D0C-4CC3-9DB3-BF1C90B721B3", "developerWalkthroughDocument",
                TopicTypeGroup.HowTo, TopicTypeGroup.Tasks),
            new("A635375F-98C2-4241-94E7-E427B47C20B6", "developerErrorMessageDocument",
                TopicTypeGroup.Reference, TopicTypeGroup.Reference),
            new("95DADC4C-A2A6-447A-AA36-B6BE3A4F8DEC", "developerReferenceWithSyntaxDocument",
                TopicTypeGroup.Reference, TopicTypeGroup.Reference),
            new("F9205737-4DEC-4A58-AA69-0E621B1236BD", "developerReferenceWithoutSyntaxDocument",
                TopicTypeGroup.Reference, TopicTypeGroup.Reference),
            new("38C8E0D1-D601-4DBA-AE1B-5BEC16CD9B01", "developerTroubleshootingDocument",
                TopicTypeGroup.Reference, TopicTypeGroup.Tasks),
            new("B8ED9F21-39A4-4967-928D-160CD2ED9DCE", "developerUIReferenceDocument",
                TopicTypeGroup.Reference, TopicTypeGroup.Reference),
            new("3272D745-2FFC-48C4-9E9D-CF2B2B784D5F", "developerXmlReference",
                TopicTypeGroup.Reference, TopicTypeGroup.Reference),
            new("A689E19C-2687-4881-8CE1-652FF60CF46C", "developerGlossaryDocument",
                TopicTypeGroup.Reference, TopicTypeGroup.OtherResources),
            new("069EFD88-412D-4E2F-8848-2D5C3AD56BDE", "developerSampleDocument",
                TopicTypeGroup.Samples, TopicTypeGroup.Tasks),
            new("4BBAAF90-0E5F-4C86-9D31-A5CAEE35A416", "developerSDKTechnologyOverviewCodeDirectoryDocument",
                TopicTypeGroup.Samples, TopicTypeGroup.Concepts),
            new("4A273212-0AC8-4D72-8349-EC11CD2FF8CD", "codeEntityDocument", TopicTypeGroup.Samples,
                TopicTypeGroup.OtherResources)
        ];

        private static readonly Dictionary<string, TopicType>
            uniqueIds = topicTypes.ToDictionary(t => t.UniqueId, t => t, StringComparer.OrdinalIgnoreCase),
            elementNames = topicTypes.ToDictionary(t => t.TopicElementName, t => t, StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// The unique ID of the topic type
        /// </summary>
        public string UniqueId { get; }

        /// <summary>
        /// The topic element name associated with the topic type if any
        /// </summary>
        public string TopicElementName { get; }

        /// <summary>
        /// The content type group for the topic type
        /// </summary>
        /// <value>This is used to determine the values for the content type topic metadata</value>
        public TopicTypeGroup ContentType { get; }

        /// <summary>
        /// The See Also group for the topic type
        /// </summary>
        /// <value>This is used to determine the group in which the link appears in the See Also topic section</value>
        public TopicTypeGroup SeeAlsoGroup { get; }

        #endregion

        #region Private constructors
        //=====================================================================

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="uniqueId">The unique ID for the content type</param>
        /// <param name="topicElementName">The topic element name for the content type if any</param>
        /// <param name="contentTypeTitle">The content type title</param>
        /// <param name="seeAlsoGroupTitle">The See Also group title for the content type</param>
        private TopicType(string uniqueId, string topicElementName, TopicTypeGroup contentTypeTitle,
          TopicTypeGroup seeAlsoGroupTitle)
        {
            this.UniqueId = uniqueId;
            this.TopicElementName = topicElementName;
            this.ContentType = contentTypeTitle;
            this.SeeAlsoGroup = seeAlsoGroupTitle;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to get the topic type information for the given unique ID
        /// </summary>
        /// <param name="uniqueId">The unique ID of the topic type for which to get the information</param>
        /// <returns>The topic type for the unique ID or null if not found</returns>
        public static TopicType FromUniqueId(string uniqueId)
        {
            if(!String.IsNullOrWhiteSpace(uniqueId) && uniqueIds.TryGetValue(uniqueId, out TopicType t))
                return t;

            return null;
        }

        /// <summary>
        /// This is used to get the topic type information for the given element name
        /// </summary>
        /// <param name="elementName">The element name of the topic type for which to get the information</param>
        /// <returns>The topic type for the element name or null if not found</returns>
        public static TopicType FromElementName(string elementName)
        {
            if(!String.IsNullOrWhiteSpace(elementName) && elementNames.TryGetValue(elementName, out TopicType t))
                return t;

            return null;
        }

        /// <summary>
        /// This is used to get a description for the given topic type group
        /// </summary>
        /// <param name="group">The topic type group for which to get a description</param>
        /// <returns>The description for the topic type group</returns>
        public static string DescriptionForTopicTypeGroup(TopicTypeGroup group)
        {
            switch(group)
            {
                case TopicTypeGroup.HowTo:
                    return "How To";

                case TopicTypeGroup.OtherResources:
                    return "Other Resources";

                default:
                    return group.ToString();
            }
        }
        #endregion
    }
}
