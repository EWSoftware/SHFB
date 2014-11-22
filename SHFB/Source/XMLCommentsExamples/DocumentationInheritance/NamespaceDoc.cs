//===============================================================================================================
// System  : Sandcastle Tools - XML Comments Example
// File    : NamespaceDoc.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/26/2014
// Note    : Copyright 2012-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This class is used to demonstrate the use of a NamespaceDoc class to define namespace comments.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/06/2012  EFW  Created the code
//===============================================================================================================

namespace XMLCommentsExamples.DocumentationInheritance
{
    /// <summary>
    /// These are comments from the DocumentationInheritance namespace's NamespaceDoc class
    /// </summary>
    /// <conceptualLink target="41B2D835-DB0D-4828-8D9E-0E423EDA4590" />
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    class NamespaceDoc
    {
    }

    /*
     * This project does not have enough namespaces to make namespace grouping useful.  However, if you do
     * have a project in which namespace grouping is enabled, you can maintain the namespace group comments
     * using a NamespaceGroupDoc class as shown below.  As with the NamespaceDoc class you would place it in
     * the namespace to which the group comments apply.

    /// <summary>
    /// These are comments from the Company.Product.Root namespace group's NamespaceGroupDoc class
    /// </summary>
    /// <conceptualLink target="41B2D835-DB0D-4828-8D9E-0E423EDA4590" />
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    class NamespaceGroupDoc
    {
    }
    */
}
