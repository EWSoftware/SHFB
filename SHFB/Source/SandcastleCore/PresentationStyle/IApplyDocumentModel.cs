//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IApplyDocumentModel.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/16/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains an interface used to implement classes used to apply a document model to a reflection
// information file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/05/2021  EFW  Created the code
//===============================================================================================================

namespace Sandcastle.Core.PresentationStyle
{
    /// <summary>
    /// This interface is used to implement the application of a document model to the reflection information
    /// file.
    /// </summary>
    public interface IApplyDocumentModel
    {
        /// <summary>
        /// This is used to get or set an optional root namespace container ID
        /// </summary>
        /// <value>If not set, no root namespace container node will be added.  If set, a root namespace
        /// container node (R:) will be added with the given ID.  The build engine will set this based on the
        /// root namespace container project properties.</value>
        string RootNamespaceContainerId { get; set; }

        /// <summary>
        /// This is used to apply the document model to a reflection information file
        /// </summary>
        /// <param name="reflectionDataFile">The source reflection data file</param>
        /// <param name="docModelReflectionDataFile">The destination reflection data file with the document
        /// model applied to it.</param>
        void ApplyDocumentModel(string reflectionDataFile, string docModelReflectionDataFile);
    }
}
