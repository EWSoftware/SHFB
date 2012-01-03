//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : ResourceItemFileEditorFactory.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/26/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used generate resource item file editor instances
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/26/2011  EFW  Created the code
//=============================================================================

using System;
using System.Runtime.InteropServices;

namespace SandcastleBuilder.Package.Editors
{
    /// <summary>
    /// This is the factory class for resource item file editors
    /// </summary>
    [Guid(GuidList.guidResourceItemEditorFactoryString)]
    public sealed class ResourceItemEditorFactory : SimpleEditorFactory<ResourceItemEditorPane>
    {
    }
}
