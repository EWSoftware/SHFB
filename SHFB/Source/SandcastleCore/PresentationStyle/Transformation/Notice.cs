//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : Notice.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/02/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains an class used to represent a notice that appears at the top of a topic or as a tag for the
// member in the list page.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/24/2025  EFW  Created the code
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation
{
    /// <summary>
    /// This represents a notice that appears at the top of a topic or as a tag for the member in the list page
    /// </summary>
    public class Notice : INotifyPropertyChanged
    {
        #region Common notices created by default
        //=====================================================================

        /// <summary>
        /// This returns a default instance of the preliminary notice
        /// </summary>
        public static Notice PreliminaryNotice => new()
        {
            ElementName = "preliminary",
            UseValueForText = true,
            NoticeMessage = "@preliminaryApi",
            TagText = "@preliminaryShort"
        };

        /// <summary>
        /// This returns a default instance of the obsolete notice
        /// </summary>
        public static Notice ObsoleteNotice => new()
        {
            AttributeTypeName = "T:System.ObsoleteAttribute",
            UseValueForText = true,
            NoticeMessage = "@boilerplate_obsoleteLong",
            TagText = "@boilerplate_obsoleteShort"
        };

        /// <summary>
        /// This returns a default instance of the experimental notice
        /// </summary>
        public static Notice ExperimentalNotice => new()
        {
            AttributeTypeName = "T:System.Diagnostics.CodeAnalysis.ExperimentalAttribute",
            NoticeMessage = "@boilerplate_experimentalLong",
            TagText = "@boilerplate_experimentalShort"
        };
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// An attribute type name if this notice is related to one
        /// </summary>
        /// <value>This should be the fully qualified attribute name including the "T:" prefix</value>
        public string AttributeTypeName
        {
            get;
            set
            {
                if(field != value)
                {
                    field = value;
                    this.OnPropertyChanged();
                    this.Validate();
                }
            }
        }

        /// <summary>
        /// An XML comments element name if this notice is related to one
        /// </summary>
        public string ElementName
        {
            get;
            set
            {
                if(field != value)
                {
                    field = value;
                    this.OnPropertyChanged();
                    this.Validate();
                }
            }
        }

        /// <summary>
        /// Set this to true to have the notice show the XML comment text or the attribute value if present.
        /// If set to false or there is no inner text or value, the <see cref="NoticeMessage"/> will be used
        /// instead.
        /// </summary>
        public bool UseValueForText
        {
            get;
            set
            {
                if(field != value)
                {
                    field = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The notice message for the topic page
        /// </summary>
        /// <value>This can be literal text or a resource item name preceded by an '@'</value>
        public string NoticeMessage
        {
            get;
            set
            {
                if(field != value)
                {
                    field = value;
                    this.OnPropertyChanged();
                    this.Validate();
                }
            }
        }

        /// <summary>
        /// The text for the tag on the member list page
        /// </summary>
        /// <value>This can be literal text or a resource item name preceded by an '@'</value>
        public string TagText
        {
            get;
            set
            {
                if(field != value)
                {
                    field = value;
                    this.OnPropertyChanged();
                    this.Validate();
                }
            }
        }

        /// <summary>
        /// For HTML presentation styles, this allows you to set the CSS classes used for the topic notice.  If
        /// not set, a default style will be used.
        /// </summary>
        public string NoticeStyleClasses
        {
            get;
            set
            {
                if(field != value)
                {
                    field = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// For HTML presentation styles, this allows you to set the CSS classes used for the member list tag.
        /// If not set, a default style will be used.
        /// </summary>
        public string TagStyleClasses
        {
            get;
            set
            {
                if(field != value)
                {
                    field = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This read-only property returns an error message describing any issues with the settings
        /// </summary>
        public string ErrorMessage
        {
            get;
            private set
            {
                field = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// This returns a description of the entry suitable for display in a bound list control
        /// </summary>
        public string NoticeDescription
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region INotifyPropertyChanged implementation
        //=====================================================================

        /// <summary>
        /// The property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This raises the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="propertyName">The property name that changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to validate the settings
        /// </summary>
        private void Validate()
        {
            if(String.IsNullOrWhiteSpace(this.ElementName) && String.IsNullOrWhiteSpace(this.AttributeTypeName))
                this.ErrorMessage = "An element name or attribute type name is required";
            else
            {
                // A notice message can be omitted to leave off the notice message.  Likewise, tag text can be
                // omitted to leave off the tag.  However one or the other must be specified.  The "Use value
                // for text" option can be set but if the element/attribute doesn't have one and the notice
                // message is blank, the notice won't show on the topic.
                if(String.IsNullOrWhiteSpace(this.NoticeMessage) && !this.UseValueForText &&
                  String.IsNullOrWhiteSpace(this.TagText))
                {
                    this.ErrorMessage = "A notice message must be specified or set 'Use value for text' or " +
                        "tag text must be specified";
                }
                else
                    this.ErrorMessage = null;
            }

            var sb = new StringBuilder(1024);

            if(!String.IsNullOrWhiteSpace(this.AttributeTypeName))
                sb.Append(this.AttributeTypeName);

            if(!String.IsNullOrWhiteSpace(this.ElementName))
            {
                if(sb.Length != 0)
                    sb.Append('/');

                sb.Append(this.ElementName);
            }

            this.NoticeDescription = sb.ToString();
        }

        /// <summary>
        /// Create a notice instance from an XElement containing the settings
        /// </summary>
        /// <param name="element">The XElement from which to obtain the settings</param>
        /// <returns>A <see cref="Notice"/> instance containing the settings from the XElement</returns>
        /// <remarks>It should contain an element called <c>Notice</c> with the necessary attributes
        /// for each of the properties.</remarks>
        public static Notice FromXml(XElement element)
        {
            if(element == null)
                throw new ArgumentNullException(nameof(element));

            return new Notice
            {
                ElementName = element.Attribute(nameof(ElementName))?.Value,
                AttributeTypeName = element.Attribute(nameof(AttributeTypeName))?.Value,
                NoticeMessage = element.Attribute(nameof(NoticeMessage))?.Value,
                TagText = element.Attribute(nameof(TagText))?.Value,
                NoticeStyleClasses = element.Attribute(nameof(NoticeStyleClasses))?.Value,
                TagStyleClasses = element.Attribute(nameof(TagStyleClasses))?.Value,
                UseValueForText = (bool?)element.Attribute(nameof(UseValueForText)) ?? false,
            };
        }

        /// <summary>
        /// Store the wildcard reference settings as a node in the given XML element
        /// </summary>
        /// <returns>Returns the node to add</returns>
        /// <remarks>The reference link settings are stored in an element called <c>reference</c> with three
        /// attributes (<c>path</c>, <c>wildcard</c>, and <c>recurse</c>).</remarks>
        public XElement ToXml()
        {
            return new XElement(nameof(Notice),
                String.IsNullOrWhiteSpace(this.ElementName) ? null : new XAttribute(nameof(ElementName), this.ElementName),
                String.IsNullOrWhiteSpace(this.AttributeTypeName) ? null : new XAttribute(nameof(AttributeTypeName), this.AttributeTypeName),
                String.IsNullOrWhiteSpace(this.NoticeMessage) ? null : new XAttribute(nameof(NoticeMessage), this.NoticeMessage),
                String.IsNullOrWhiteSpace(this.TagText) ? null : new XAttribute(nameof(TagText), this.TagText),
                String.IsNullOrWhiteSpace(this.NoticeStyleClasses) ? null : new XAttribute(nameof(NoticeStyleClasses), this.NoticeStyleClasses),
                String.IsNullOrWhiteSpace(this.TagStyleClasses) ? null : new XAttribute(nameof(TagStyleClasses), this.TagStyleClasses),
                new XAttribute(nameof(UseValueForText), this.UseValueForText));
        }
        #endregion
    }
}
