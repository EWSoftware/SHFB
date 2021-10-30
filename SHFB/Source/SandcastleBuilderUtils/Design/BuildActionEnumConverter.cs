//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildActionEnumConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/13/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a type converter used to limit the available build actions in the project explorer.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/01/2008  EFW  Created the code
// 04/06/2011  EFW  Made the class public for use in the VSPackage
//===============================================================================================================

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
            if(values == null)
            {
                var baseValues = base.GetStandardValues(context);
                var list = new ArrayList();

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
