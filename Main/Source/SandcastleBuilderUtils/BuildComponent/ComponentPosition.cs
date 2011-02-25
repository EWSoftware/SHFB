//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ComponentPosition.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/03/2008
// Note    : Copyright 2007-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains an class that defines the position of a custom build
// component in the Sandcastle configuration file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.2  11/02/2007  EFW  Created the code
// 1.6.0.7  05/03/2008  EFW  Added support for conceptual build configurations
//=============================================================================

using System;
using System.Globalization;
using System.Xml.XPath;

namespace SandcastleBuilder.Utils.BuildComponent
{
    /// <summary>
    /// This is used to define the position of a custom build component within
    /// the Sandcastle configuration file.
    /// </summary>
    public class ComponentPosition
    {
        #region Component placement enumeration
        //=====================================================================
        // Placement enumeration

        /// <summary>
        /// This enumeration defines the placement values
        /// </summary>
        [Serializable]
        public enum Placement
        {
            /// <summary>Place the component before the one indicated.</summary>
            Before,
            /// <summary>Place the component after the one indicated.</summary>
            After,
            /// <summary>Insert the component at the start of the configuration
            /// file.</summary>
            Start,
            /// <summary>Insert the component at the end of the configuration
            /// file.</summary>
            End,
            /// <summary>Replace the indicated component configuration with
            /// this one.</summary>
            Replace,
            /// <summary>The component is not used in this configuration</summary>
            NotUsed
        }
        #endregion

        #region Private data members
        //=====================================================================
        // Private data members

        private Placement place;
        private string id, typeName;
        private int instance, adjustedInstance;
        #endregion

        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// This read-only property returns the placement value
        /// </summary>
        public Placement Place
        {
            get { return place; }
        }

        /// <summary>
        /// This read-only property returns the ID of the component
        /// </summary>
        /// <remarks>If not specified, use <see cref="TypeName"/>.</remarks>
        public string Id
        {
            get { return id; }
        }

        /// <summary>
        /// This read-only property returns the type name of the component
        /// </summary>
        /// <remarks>If not specified, use <see cref="Id"/>.</remarks>
        public string TypeName
        {
            get { return typeName; }
        }

        /// <summary>
        /// This read-only property returns the instance of the component
        /// to replace, insert before, or insert after.
        /// </summary>
        /// <remarks>If not specified 1 is assumed.</remarks>
        public int Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// This property is used to get or set the instance of the component
        /// to replace, insert before, or insert after adjusted for other
        /// components that have already been processed.
        /// </summary>
        internal int AdjustedInstance
        {
            get { return adjustedInstance; }
            set { adjustedInstance = value; }
        }
        #endregion

        //=====================================================================
        // Methods, etc.

        /// <summary>
        /// Constructor
        /// </summary>
        public ComponentPosition()
        {
            place = Placement.NotUsed;
            instance = 1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">The XPath navigator containing the configuration</param>
        public ComponentPosition(XPathNavigator config)
        {
            string attrValue;

            if(config == null)
                throw new ArgumentNullException("config");

            attrValue = config.GetAttribute("placement", String.Empty);
            if(!String.IsNullOrEmpty(attrValue))
                place = (Placement)Enum.Parse(typeof(Placement), attrValue,
                    true);

            attrValue = config.GetAttribute("instance", String.Empty);
            if(String.IsNullOrEmpty(attrValue))
                instance = 1;
            else
            {
                instance = Convert.ToInt32(attrValue,
                    CultureInfo.InvariantCulture);

                if(instance < 1)
                    throw new InvalidOperationException("The instance " +
                        "attribute cannot be less than one");
            }

            attrValue = config.GetAttribute("id", String.Empty);
            if(!String.IsNullOrEmpty(attrValue))
                id = attrValue;
            else
                typeName = config.GetAttribute("type", String.Empty);

            if((place == Placement.Before || place == Placement.After ||
              place == Placement.Replace) && String.IsNullOrEmpty(id) &&
              String.IsNullOrEmpty(typeName))
                throw new InvalidOperationException("An ID or type name " +
                    "must be specified if Before, After, or Replace is used " +
                    "for the Placement option");
        }
    }
}
