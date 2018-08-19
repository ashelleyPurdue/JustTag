using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JustTag.Tagging
{
    /// <summary>
    /// Represents a file or folder path that can have tags.
    /// Does not necessarily point to an *existing* file or folder.
    /// This class is immutable.
    /// </summary>
    public class TaggedFilePath
    {
        public readonly string name;        // The filename without any tags.  Does not include the extension.
        public readonly string[] tags;      // All of the tags belonging to this file
        public readonly string parentFolder;// The full path to the parent folder
        public readonly string extension;   // The file extension(eg: .txt).  Includes the dot.
                                            // If there is no extension, then it will be the empty string.
        public readonly bool isFolder;

        public TaggedFilePath(string filePath, bool isFolder)
        {
            // TODO: Parse the file path
        }

        /// <summary>
        /// Constructs the full path to this 
        /// </summary>
        /// <returns></returns>
        public string GetFullPath() => throw new NotImplementedException();
    }
}
