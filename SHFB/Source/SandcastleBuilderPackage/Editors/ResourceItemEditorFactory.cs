//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : ResourceItemFileEditorFactory.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/26/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This file contains a class used generate resource item file editor instances
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/26/2011  EFW  Created the code
//===============================================================================================================

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
