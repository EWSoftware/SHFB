//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : ResolveNameFunction.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/17/2021
// Note    : Copyright 2007-2021, Eric Woodruff, All rights reserved
//
// This file contains a custom XPath function used to convert an API name into its more readable form which is
// used for searching.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/27/2007  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: utils Filt Excep

using System;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace SandcastleBuilder.WPF.XPath
{
    /// <summary>
    /// This class is a custom XPath function used to convert an API name into its more readable form used for
    /// searching.
    /// </summary>
    /// <remarks>The function should be passed an XML node containing the necessary information used to obtain
    /// the name and convert it into the searchable format along with a boolean indicating whether or not the
    /// name should be fully qualified with the namespace and type.</remarks>
    /// <example>
    /// Some examples of XPath queries using the function:
    /// 
    /// <code language="none">
    /// //apis/api[matches-regex(resolve-name(node(), boolean(false),
    /// 'utils.*proj', boolean(true))
    ///
    /// //apis/api[matches-regex(resolve-name(node(), boolean(true)),
    /// 'Proj|Filt|Excep', boolean(false))
    /// </code>
    /// </example>
    internal sealed class ResolveNameFunction : IXsltContextFunction
    {
        #region IXsltContextFunction Members
        //=====================================================================

        /// <summary>
        /// Gets the supplied XPath types for the function's argument list.  This information can be used to
        /// discover the signature of the function which allows you to differentiate between overloaded
        /// functions.
        /// </summary>
        /// <value>Always returns an array with a <c>Navigator</c> type and a <c>Boolean</c> type entry</value>
        public XPathResultType[] ArgTypes => new XPathResultType[] { XPathResultType.Navigator,
            XPathResultType.Boolean };

        /// <summary>
        /// Gets the minimum number of arguments for the function. This enables the user to differentiate between
        /// overloaded functions.
        /// </summary>
        /// <value>Always returns two</value>
        public int Minargs => 2;

        /// <summary>
        /// Gets the maximum number of arguments for the function. This enables the user to differentiate between
        /// overloaded functions.
        /// </summary>
        /// <value>Always returns two</value>
        public int Maxargs => 2;

        /// <summary>
        /// Gets the XPath type returned by the function
        /// </summary>
        /// <value>Always returns <c>String</c></value>
        public XPathResultType ReturnType => XPathResultType.String;

        /// <summary>
        /// This is called to invoke the <b>resolve-name</b> method
        /// </summary>
        /// <param name="xsltContext">The XSLT context for the function call</param>
        /// <param name="args">The arguments for the function call</param>
        /// <param name="docContext">The context node for the function call</param>
        /// <returns>An object representing the return value of the function (the name string)</returns>
        /// <exception cref="ArgumentException">This is thrown if the number of arguments for the function is
        /// not two.</exception>
        public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            XPathNavigator nav;
            XmlNode apiNode;
            XmlNodeList templates;
            StringBuilder sb = new StringBuilder(100);
            string nodeText;
            bool fullyQualified;
            int pos, idx = 1;
            char nodeType;

            if(args.Length != 2)
                throw new ArgumentException("There must be two parameters passed to the 'resolve-name' function",
                    nameof(args));

            nav = ((XPathNodeIterator)args[0]).Current;
            apiNode = ((IHasXmlNode)nav).GetNode();
            fullyQualified = Convert.ToBoolean(args[1], CultureInfo.InvariantCulture);

            if(apiNode.Name == "api")
            {
                templates = apiNode.SelectNodes("templates/template");
                nodeText = apiNode.Attributes["id"].Value;
            }
            else
            {
                nodeText = apiNode.Attributes["api"].Value;

                templates = apiNode.SelectNodes("specialization/template");

                if(templates.Count == 0)
                    templates = apiNode.SelectNodes("specialization/type");
            }

            nodeType = nodeText[0];
            nodeText = nodeText.Substring(2);

            if(nodeType == 'N')
            {
                if(nodeText.Length == 0 && !fullyQualified)
                    nodeText = "(global)";
            }
            else
            {
                // Strip parameters
                pos = nodeText.IndexOf('(');

                if(pos != -1)
                    nodeText = nodeText.Substring(0, pos);

                // Remove the namespace and type if not fully qualified.  Note the last period's position though
                // as we'll need it to skip generic markers in the name type name.
                pos = nodeText.LastIndexOf('.');

                if(!fullyQualified && pos != -1)
                {
                    nodeText = nodeText.Substring(pos + 1);
                    pos = 0;
                }

                if(nodeType != 'T')
                {
                    // Replace certain values to make it more readable
                    if(nodeText.IndexOf("#cctor", StringComparison.Ordinal) != -1)
                        nodeText = nodeText.Replace("#cctor", "Static Constructor");
                    else
                        if(nodeText.IndexOf("#ctor", StringComparison.Ordinal) != -1)
                            nodeText = nodeText.Replace("#ctor", "Constructor");
                        else
                        {
                            if(nodeText.IndexOf('#') != -1)
                                nodeText = nodeText.Replace('#', '.');

                            if(pos == 0)
                            {
                                if(nodeText.StartsWith("op_", StringComparison.Ordinal))
                                    nodeText = nodeText.Substring(3) + " Operator";
                            }
                            else
                                if(nodeText.IndexOf("op_", pos, StringComparison.Ordinal) != -1)
                                    nodeText = nodeText.Substring(0, pos + 1) + nodeText.Substring(pos + 4) + " Operator";
                        }
                }

                // Replace generic template markers with the names
                if(pos != -1)
                    pos = nodeText.IndexOf('`', pos);

                if(pos != -1)
                {
                    nodeText = nodeText.Substring(0, pos);

                    foreach(XmlNode template in templates)
                    {
                        if(sb.Length != 0)
                            sb.Append(',');

                        if(template.Name != "type")
                            sb.Append(template.Attributes["name"].Value);
                        else
                        {
                            // For specializations of types, we don't want to show the type but a generic placeholder
                            sb.Append('T');

                            if(idx > 1)
                                sb.Append(idx);

                            idx++;
                        }
                    }

                    if(sb.Length != 0)
                    {
                        sb.Insert(0, "<");
                        sb.Append('>');
                        nodeText += sb.ToString();
                    }
                }

                // Replace generic template markers in the type name if fully qualified
                if(fullyQualified && nodeText.IndexOf('`') != -1)
                    nodeText = XPathFunctionContext.ReplaceTypeTemplateMarker(apiNode, nodeText);
            }

            return nodeText;
        }
        #endregion
    }
}
