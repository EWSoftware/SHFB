//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ParseMessages.cs
// Author  : Jason Curl (jcurl@arcor.de)
// Updated : 04/05/2026
// Note    : Copyright 2026, SHFB project, All rights reserved
//
// Contains a warning entry, raised from the builder.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/05/2025  JMC  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Core.Markdown.Extensions.Inclusion;

/// <summary>
/// An entry for a warning message.
/// </summary>
public readonly struct ParseMessages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParseMessages"/> struct.
    /// </summary>
    /// <param name="code">The warning code.</param>
    /// <param name="message">The message for the warning.</param>
    public ParseMessages(string code, string message)
    {
        Code = code ?? String.Empty;
        Message = message ?? String.Empty;
    }

    public string Code { get; }

    public string Message { get; }
}
