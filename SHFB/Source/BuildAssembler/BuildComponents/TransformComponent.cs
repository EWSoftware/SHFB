// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 10/14/2012 - EFW - Added code to raise a component event to signal that the topic has been transformed.
// The event uses the new TransformedTopicEventArgs as the event arguments.
// 12/24/2013 - EFW - Updated the build component to be discoverable via MEF
// 04/27/2014 - EFW - Added support for a "transforming topic" event that happens prior to transformation
// 01/18/2022 - EFW - Replaced the transformed/transforming events with more generic applying/applied changes events
// 02/27/2022 - EFW - Renamed to Transform Component, dropped all references to XSL, and updated it to use the
// new code-based transformation process.

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;
using Sandcastle.Core.PresentationStyle.Transformation;

namespace Sandcastle.Tools.BuildComponents
{
    /// <summary>
    /// This build component is used to transform the intermediate topic to its final form such as an HTML
    /// document.
    /// </summary>
    public class TransformComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Transform Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new TransformComponent(this.BuildAssembler);
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private TopicTransformationCore transformation;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected TransformComponent(IBuildAssembler buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            transformation = this.BuildAssembler.TopicTransformation;

            // Add language filter item settings from the Syntax Component for presentation styles that use them
            if(this.BuildAssembler.Data["LanguageFilterItems"] is IEnumerable<LanguageFilterItem> languageFilterItems)
                transformation.AddLanguageFilterItems(languageFilterItems);
        }

        /// <summary>
        /// This is overridden to apply the presentation style transformations to the document
        /// </summary>
        /// <param name="document">The document to transform</param>
        /// <param name="key">The topic key</param>
        /// <remarks><note type="important">An argument called <c>key</c> is automatically added to the argument
        /// list when each topic is transformed.  It will contain the current topic's key.</note></remarks>
        public override void Apply(XmlDocument document, string key)
        {
            if(document == null)
                throw new ArgumentNullException(nameof(document));

            // Raise a component event to signal that the topic is about to be transformed
            this.OnComponentEvent(new ApplyingChangesEventArgs(this.GroupId, "Transform Component", key, document));

            // Historically, the document has been an XmlDocument.  The transformation uses an XDocument so we
            // need to convert it.  Longer term, changing the entire build component chain to use XDocument may
            // be considered.
            using(var reader = new XmlNodeReader(document))
            {
                reader.MoveToContent();

                try
                {
                    var transformedDoc = transformation.Render(key, XDocument.Load(reader));

                    try
                    {
                        using(var xmlReader = transformedDoc.CreateReader())
                        {
                            document.Load(xmlReader);
                        }
                    }
                    catch(Exception xmlEx)
                    {
                        this.WriteMessage(key, MessageLevel.Error, "An error occurred while attempting to " +
                            "reload the transformed topic. The error message was: {0}", xmlEx);
                    }
                }
                catch(Exception ex)
                {
                    this.WriteMessage(key, MessageLevel.Error, "An error occurred while attempting to " +
                        "transform the reflection data to a topic. The error message was: {0}", ex);
                }
            }

            // Raise a component event to signal that the topic has been transformed
            this.OnComponentEvent(new AppliedChangesEventArgs(this.GroupId, "Transform Component", key, document));
        }
        #endregion
    }
}
