//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SandcastleBuilderFileNode.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/08/2012
// Note    : Copyright 2011-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that represents a file node in a Sandcastle
// Help File Builder Visual Studio project.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/27/2011  EFW  Created the code
// 1.9.4.0  04/08/2012  EFW  Added support for XAML configuration files
//=============================================================================

using System;
using System.IO;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;

namespace SandcastleBuilder.Package.Nodes
{
    /// <summary>
    /// This class represents a file node in a Sandcastle Help File Builder
    /// Visual Studio project.
    /// </summary>
    public class SandcastleBuilderFileNode : FileNode
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Initializes a new instance of the Sandcastle Builder file node class.
        /// </summary>
        /// <param name="root">The root <see cref="SandcastleBuilderProjectNode"/>
        /// that contains this node.</param>
        /// <param name="element">The element that contains MSBuild properties.</param>
        public SandcastleBuilderFileNode(SandcastleBuilderProjectNode root, ProjectElement element) :
          base(root, element)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// Creates an object derived from <see cref="NodeProperties"/> that
        /// will be used to expose properties specific for this object to the
        /// property browser.
        /// </summary>
        /// <returns>A new <see cref="SandcastleBuilderFileNodeProperties"/>
        /// object.</returns>
        protected override NodeProperties CreatePropertiesObject()
        {
            if(this.IsNonMemberItem)
                return new FileNodeProperties(this);

            return new SandcastleBuilderFileNodeProperties(this);
        }

        /// <summary>
        /// This is overridden to get an appropriate icon for the extra
        /// Sandcastle Help File Builder file types
        /// </summary>
        /// <param name="open"></param>
        /// <returns></returns>
        public override object GetIconHandle(bool open)
        {
            int index = base.ImageIndex;
            string ext;

            // If no image is found, try for one of the SHFB file types
            if(index == HierarchyNode.NoImage)
            {
                ext = Path.GetExtension(this.FileName).ToLowerInvariant();

                switch(ext)
                {
                    case ".aml":
                        index = (int)ProjectImageIndex.ConceptualContent;
                        break;

                    case ".content":
                    case ".sitemap":
                        index = (int)ProjectImageIndex.ContentSiteMap;
                        break;

                    case ".items":
                        index = (int)ProjectImageIndex.ResourceItems;
                        break;

                    case ".snippets":
                        index = (int)ProjectImageIndex.CodeSnippets;
                        break;

                    case ".tokens":
                        index = (int)ProjectImageIndex.Tokens;
                        break;

                    case ".xamlcfg":    // Use the default XML file icon
                        return this.ProjectMgr.ImageHandler.GetIconHandle((int)ProjectNode.ImageName.XMLFile);

                    default:
                        break;
                }

                if(index != HierarchyNode.NoImage)
                    return this.ProjectMgr.ImageHandler.GetIconHandle(this.ProjectMgr.ImageIndex + index);
            }

            return base.GetIconHandle(open);
        }

        /// <inheritdoc />
        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText,
          ref QueryStatusResult result)
        {
            if(cmdGroup == VsMenus.guidStandardCommandSet97 && (VsCommands)cmd == VsCommands.ViewCode)
            {
                result |= QueryStatusResult.INVISIBLE | QueryStatusResult.SUPPORTED;
                return VSConstants.S_OK;
            }

            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }
        #endregion
    }
}
