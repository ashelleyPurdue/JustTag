using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JustTag.Tagging
{
    /// <summary>
    /// There are a few properties that DirectoryInfo and FileInfo share,
    /// but with different names.  These extension methods provide a common
    /// interface to both of them, eliminating the need for extra if-statements
    /// and casts.
    /// </summary>
    internal static class FileSystemInfoExtensions
    {
        /// <summary>
        /// Gets the parent folder.
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static DirectoryInfo GetParent(this FileSystemInfo me)
        {
            if (me is DirectoryInfo)
                return ((DirectoryInfo)me).Parent;

            // It's not a directory, so it must be a file.
            return ((FileInfo)me).Directory;
        }
    }
}
