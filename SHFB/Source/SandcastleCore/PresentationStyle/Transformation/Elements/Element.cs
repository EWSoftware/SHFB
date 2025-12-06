//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : Element.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/23/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the abstract base class used to create rendered elements in API and MAML topics
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/14/2022  EFW  Created the code
//===============================================================================================================

using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements;

/// <summary>
/// This abstract base class is used to create rendered elements in API and MAML topics
/// </summary>
public abstract class Element
{
    #region Constants
    //=====================================================================

    /// <summary>
    /// The root MAML namespace
    /// </summary>
    public static readonly XNamespace Ddue = "http://ddue.schemas.microsoft.com/authoring/2003/5";

    /// <summary>
    /// The XML linking language namespace
    /// </summary>
    public static readonly XNamespace Xlink = "http://www.w3.org/1999/xlink";

    /// <summary>
    /// The xml:space name
    /// </summary>
    public static readonly XName XmlSpace = XNamespace.Xml + "space";

    /// <summary>
    /// A non-breaking space
    /// </summary>
    /// <remarks><c>XText</c> doesn't write out entities in text such as "&#160;" so we need to use literal
    /// characters instead.</remarks>
    public const char NonBreakingSpace = '\xA0';

    #endregion

    #region Properties
    //=====================================================================

    /// <summary>
    /// This read-only property returns the element name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// This read-only property returns true if the element is a block element, false if it is not.
    /// </summary>
    /// <remarks>This is used to help the Markdown block parser determine block elements and those that may
    /// need their content parsed to convert Markdown text within child elements.</remarks>
    public bool IsBlockElement { get; }

    /// <summary>
    /// This is used to get or set whether or not to parse the element for Markdown content
    /// </summary>
    public bool DoNotParse { get; set; }

    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">The element name</param>
    /// <remarks>The element is assumed to be an inline element</remarks>
    /// <overloads>There are two overloads for the constructor</overloads>
    protected Element(string name)
    {
        this.Name = name;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">The element name</param>
    /// <param name="isBlockElement">True if the element is a block element, false if it is not</param>
    protected Element(string name, bool isBlockElement)
    {
        this.Name = name;
        this.IsBlockElement = isBlockElement;
    }
    #endregion

    #region Methods
    //=====================================================================

    /// <summary>
    /// Render the element to the topic
    /// </summary>
    /// <param name="transformation">The topic transformation in use</param>
    /// <param name="element">The element to handle</param>
    public abstract void Render(TopicTransformationCore transformation, XElement element);

    #endregion
}
