//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : BuildActionEnumConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/06/2011
// Note    : Copyright 2008-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type converter used to limit the available build
// actions in the project explorer.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  08/01/2008  EFW  Created the code
// 1.9.3.0  04/06/2011  EFW  Made the class public for use in the VSPackage
//=============================================================================

using System.Collections;
using System.ComponentModel;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This is used to limit which values are available to properties using
    /// <see cref="BuildAction" /> as their underlying type.
    /// </summary>
    public sealed class BuildActionEnumConverter : EnumConverter
    {
        #region Private data members
        //=====================================================================

        private StandardValuesCollection values;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public BuildActionEnumConverter() : base(typeof(BuildAction))
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>Build actions that serve no purpose for selection
        /// are removed.</remarks>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            StandardValuesCollection baseValues;
            ArrayList list;
            
            if(values == null)
            {
                baseValues = base.GetStandardValues(context);
                list = new ArrayList();

                foreach(BuildAction v in baseValues)
                    if(v < BuildAction.Folder)
                        list.Add(v);

                values = new StandardValuesCollection(list);
            }

            return values;
        }
        #endregion
    }
}
