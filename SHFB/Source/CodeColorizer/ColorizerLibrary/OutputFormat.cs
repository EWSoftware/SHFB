//===============================================================================================================
// System  : Code Colorizer Library
// File    : CodeColorizer.cs
// Author  : Eric Woodruff
// Updated : 12/18/2012
// Compiler: Microsoft Visual C#
//
// This contains an enumerated type that defines the output format
//
//===============================================================================================================

namespace ColorizerLibrary
{
    /// <summary>
    /// This enumerated type defines the output format of the associated XSL transformation file
    /// used to colorize the text.
    /// </summary>
    public enum OutputFormat
    {
        /// <summary>HTML</summary>
        Html,
        /// <summary>XAML flow document</summary>
        FlowDocument
    }
}
