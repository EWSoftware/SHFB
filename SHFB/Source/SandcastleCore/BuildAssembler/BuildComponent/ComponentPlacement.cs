//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ComponentPlacement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/06/2021
// Note    : Copyright 2013-2021, Eric Woodruff, All rights reserved
//
// This file contains a class that is used to define the placement of a build component within a BuildAssembler
// configuration file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/23/2013  EFW  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Core.BuildAssembler.BuildComponent
{
    /// <summary>
    /// This is used to define the placement of a build component within a BuildAssembler configuration file
    /// </summary>
    public class ComponentPlacement
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the placement action value
        /// </summary>
        public PlacementAction Placement { get; }

        /// <summary>
        /// This read-only property returns the ID of the component related to the <see cref="PlacementAction"/>
        /// if applicable.
        /// </summary>
        /// <value>This only has meaning for the <c>Before</c>, <c>After</c>, and <c>Replace</c>
        /// <see cref="PlacementAction"/>.</value>
        public string Id { get; }

        /// <summary>
        /// This read-only property returns the instance of the component to replace, insert before, or insert
        /// after.
        /// </summary>
        public int Instance { get; }

        /// <summary>
        /// This property is used to get or set the instance of the component to replace, insert before, or
        /// insert after adjusted for other components that have already been processed.
        /// </summary>
        /// <value>This is a property for use by designers and build tools</value>
        public int AdjustedInstance { get; set; }

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <remarks>The <see cref="Placement"/> is set to <c>None</c></remarks>
        public ComponentPlacement()
        {
            this.Instance = 1;
        }

        /// <summary>
        /// Constructor.  Indicate the placement action and an optional component ID related to it if needed.
        /// </summary>
        /// <param name="placement">The placement action to use</param>
        /// <param name="id">A build component ID for use with the <c>Before</c>, <c>After</c>, or
        /// <c>Replace</c> <see cref="Placement"/> action.</param>
        /// <exception cref="ArgumentException">This is thrown if <c>Before</c>, <c>After</c>, or <c>Replace</c>
        /// is specified without an ID or if one is specified with <c>None</c>, <c>Start</c>, or <c>End</c>.</exception>
        public ComponentPlacement(PlacementAction placement, string id)
        {
            if((placement == PlacementAction.Before || placement == PlacementAction.After ||
              placement == PlacementAction.Replace) && String.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("An ID must be specified if Before, After, or Replace is used " +
                    "for the placement action", nameof(id));
            }

            if((placement == PlacementAction.None || placement == PlacementAction.Start ||
              placement == PlacementAction.End) && !String.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("An ID cannot be specified if None, Start, or End is used " +
                    "for the placement action", nameof(id));
            }

            this.Placement = placement;
            this.Id = id;
            this.Instance = 1;
        }

        /// <summary>
        /// Constructor.  Indicate the placement action, component ID, and instance to act on.
        /// </summary>
        /// <param name="placement">The placement action to use</param>
        /// <param name="id">A build component ID for use with the <c>Before</c>, <c>After</c>, or
        /// <c>Replace</c> <see cref="Placement"/> action.</param>
        /// <param name="instance">The instance to use with the placement option</param>
        /// <exception cref="ArgumentException">This is thrown if <c>Before</c>, <c>After</c>, or <c>Replace</c>
        /// is specified without an ID or if instance is less than one.</exception>
        public ComponentPlacement(PlacementAction placement, string id, int instance) : this(placement, id)
        {
            if(instance < 1)
                throw new ArgumentException("Instance must be greater than or equal to one", nameof(instance));

            this.Instance = instance;
        }
        #endregion
    }
}
