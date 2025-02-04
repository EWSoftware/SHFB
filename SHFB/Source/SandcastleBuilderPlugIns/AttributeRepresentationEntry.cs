using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using SandcastleBuilder.Utils;

namespace SandcastleBuilder.PlugIns
{
    public class AttributeRepresentationEntry : INotifyPropertyChanged
    {
        public AttributeRepresentationEntry(AttributeRendererPlugin parent)
        {
            Parent = parent;
        }

        private AttributeRendererPlugin Parent { get; }

        private string attributeClassName;

        /// <summary>
        /// This is the full name of the Attribute to render, for example: <c>T:System.ObsoleteAttribute</c>
        /// </summary>
        public string AttributeClassName
        {
            get => attributeClassName;
            set
            {
                if(attributeClassName != value)
                {
                    attributeClassName = value?.Trim();

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        private string sortRepresentation;

        /// <summary>
        /// This is the full name of the Attribute to render, for example: <c>T:System.ObsoleteAttribute</c>
        /// </summary>
        public string ShortRepresentation
        {
            get => sortRepresentation;
            set
            {
                if(sortRepresentation != value)
                {
                    sortRepresentation = value?.Trim();

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        private string longRepresentation;

        /// <summary>
        /// This is the full name of the Attribute to render, for example: <c>T:System.ObsoleteAttribute</c>
        /// </summary>
        public string LongRepresentation
        {
            get => longRepresentation;
            set
            {
                if(longRepresentation != value)
                {
                    longRepresentation = value?.Trim();

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

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
        
        private string errorMessage;

        /// <summary>
        /// This read-only property returns an error message describing any issues with the settings
        /// </summary>
        public string ErrorMessage
        {
            get => errorMessage;
            private set
            {
                errorMessage = value;

                this.OnPropertyChanged();
            }
        }

        public bool HasLongRepresentation => !String.IsNullOrWhiteSpace(LongRepresentation);
        public bool HasShortRepresentation => !String.IsNullOrWhiteSpace(ShortRepresentation);


        /// <summary>
        /// This is used to validate the settings
        /// </summary>
        private void Validate()
        {
            List<string> problems = new List<string>();

            if(String.IsNullOrWhiteSpace(AttributeClassName))
                problems.Add("An attribute name is required");

            if(!HasShortRepresentation && !HasLongRepresentation) 
                problems.Add("A representation is required");

            if (Parent.AttributeRepresentationEntries.Count(x => x!= this && x.AttributeClassName.Equals(attributeClassName, StringComparison.InvariantCulture)) > 1)
                problems.Add($"Representation for {attributeClassName} registered twice.");
            
            if(problems.Count != 0)
                this.ErrorMessage = String.Join(" / ", problems);
            else
                this.ErrorMessage = null;
        }

        #endregion

        #region Convert from/to XML

        //=====================================================================

        /// <summary>
        /// Create a binding redirect settings instance from an XML element containing the settings
        /// </summary>
        /// <param name="pathProvider">The base path provider object</param>
        /// <param name="configuration">The XML element from which to obtain the settings</param>
        /// <returns>A <see cref="BindingRedirectSettings"/> object containing the settings from the XPath
        /// navigator.</returns>
        /// <remarks>It should contain an element called <c>dependentAssembly</c> with a <c>configFile</c>
        /// attribute or a nested <c>assemblyIdentity</c> and <c>bindingRedirect</c> element that define
        /// the settings.</remarks>
        public static AttributeRepresentationEntry FromXml(AttributeRendererPlugin parent, XElement configuration)
        {
            AttributeRepresentationEntry entry = new AttributeRepresentationEntry(parent);

            if(configuration != null)
            {
                entry.AttributeClassName = configuration.Element("AttributeClassName")?.Value;
                entry.ShortRepresentation = configuration.Element("ShortRepresentation")?.Value;
                entry.LongRepresentation = configuration.Element("LongRepresentation")?.Value;
            }

            return entry;
        }

        /// <summary>
        /// Store the binding redirect settings in an XML element
        /// </summary>
        /// <param name="relativePath">True to allow a relative path on <c>importFrom</c> attributes, false to
        /// fully qualify the path.</param>
        /// <returns>Returns the XML element</returns>
        /// <remarks>The settings are stored in an element called <c>dependentAssembly</c>.</remarks>
        public XElement ToXml()
        {
            var el = new XElement("AttributeRepresentationEntry",
                new XElement("AttributeClassName", AttributeClassName),
                new XElement("ShortRepresentation", ShortRepresentation),
                new XElement("LongRepresentation", LongRepresentation)
                );
            
            return el;
        }

        #endregion

        public XElement GetLongRepresentation()
        {
            if (String.IsNullOrWhiteSpace(longRepresentation)) return null;
            
            // try to parse XElement
            var el = XElement.Parse(LongRepresentation);

            if(el.IsEmpty)
            {
                return new XElement("span",
                    new XAttribute("class", "tag is-danger is-medium"),
                    new XElement("include", new XAttribute("item", LongRepresentation)));;
            }
            else
            {
                return el;
            }
        }
        
        public XElement GetShortRepresentation()
        {
            if (String.IsNullOrWhiteSpace(ShortRepresentation)) return null;
            
            // try to parse XElement
            var el = XElement.Parse(ShortRepresentation);

            if(el.IsEmpty)
            {
                return new XElement("span",
                    new XAttribute("class", "tag is-danger is-medium"),
                    new XElement("include", new XAttribute("item", ShortRepresentation)));;
            }
            else
            {
                return el;
            }
        }
    }
}