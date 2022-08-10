//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ApiTopicSectionHandler.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/14/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle rendering of the various API topic sections
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/14/2022  EFW  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This handles rendering of an API topic section
    /// </summary>
    /// <remarks>API topics are rendered in various sections based on the topic type.  This facilitates rendering
    /// those sections.</remarks>
    public class ApiTopicSectionHandler
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// The section type
        /// </summary>
        public ApiTopicSectionType SectionType { get; }

        /// <summary>
        /// If <see cref="SectionType" /> is <c>CustomSection</c>, this contains the custom section name
        /// </summary>
        public string CustomSectionName { get; }

        /// <summary>
        /// This is the action to perform that will render the section
        /// </summary>
        public Action<TopicTransformationCore> RenderSection { get; }

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor for well-known sections
        /// </summary>
        /// <param name="sectionType">The section type</param>
        /// <param name="renderSection">The action to perform that will render the section.  It will be passed
        /// a reference to the topic transformations.</param>
        /// <overloads>There are two overloads for the constructor</overloads>
        public ApiTopicSectionHandler(ApiTopicSectionType sectionType,
          Action<TopicTransformationCore> renderSection) : this(sectionType, null, renderSection)
        {
        }

        /// <summary>
        /// This constructor supporting specifying a custom section type and name
        /// </summary>
        /// <param name="sectionType">The section type</param>
        /// <param name="customSectionName">The custom section name if this is a custom section</param>
        /// <param name="renderSection">The action to perform that will render the section.  It will be passed
        /// a reference to the topic transformations.</param>
        public ApiTopicSectionHandler(ApiTopicSectionType sectionType, string customSectionName,
          Action<TopicTransformationCore> renderSection)
        {
            this.SectionType = sectionType;
            this.CustomSectionName = customSectionName;
            this.RenderSection = renderSection ?? throw new ArgumentNullException(nameof(renderSection));

            if(sectionType == ApiTopicSectionType.CustomSection && String.IsNullOrWhiteSpace(customSectionName))
                throw new ArgumentException("Custom section name cannot be null or blank", nameof(customSectionName));

            if(sectionType != ApiTopicSectionType.CustomSection && !String.IsNullOrWhiteSpace(customSectionName))
            {
                throw new ArgumentException("Custom section name is not supported for well-known sections",
                    nameof(customSectionName));
            }
        }
        #endregion
    }
}
