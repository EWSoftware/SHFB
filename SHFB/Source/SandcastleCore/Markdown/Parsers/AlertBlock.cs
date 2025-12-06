//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : AlertBlock.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/24/2025
// Note    : Based on Markdig.Extensions.Alerts.AlertBlock.  Copyright (c) Alexandre Mutel. All rights
//           reserved.  Licensed under the BSD-Clause 2 license.  https://github.com/xoofx/markdig
//
// This file contains the alert block definition
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/20/2025  EFW  Created the code
//===============================================================================================================

using Markdig.Helpers;
using Markdig.Syntax;

namespace Sandcastle.Core.Markdown.Parsers;

/// <summary>
/// A class is used to represent an alert block in Markdown
/// </summary>
public class AlertBlock : QuoteBlock
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="parser">The alert block parser</param>
    /// <param name="kind">The alert kind</param>
    public AlertBlock(AlertBlockParser parser, StringSlice kind) : base(parser)
    {
        Kind = kind;
    }

    /// <summary>
    /// Gets or sets the kind of the alert block (e.g <c>NOTE</c>, <c>TIP</c>, <c>IMPORTANT</c>, <c>WARNING</c>,
    /// <c>CAUTION</c>).
    /// </summary>
    /// <remarks>For the MAML alert, the type may be followed by an optional title</remarks>
    public StringSlice Kind { get; set; }
}
