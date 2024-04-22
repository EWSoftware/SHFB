using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sandcastle.Core
{
    /// <summary>
    /// Extension methods for manipulating strings representing paths
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// Ensures that <see cref="Path.DirectorySeparatorChar"/> is the directory separator character used in a path
        /// </summary>
        /// <param name="source">The source path</param>
        /// <returns>The source path, but using the platform specific directory separator</returns>
        public static string EnsurePlatformPathSeparators(this string source)
        {
            var altDirectorySeparatorChar = Path.DirectorySeparatorChar == '/'
                ? '\\'
                : '/';
            return source == null
                ? source
                : source.Replace(altDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }
    }
}
