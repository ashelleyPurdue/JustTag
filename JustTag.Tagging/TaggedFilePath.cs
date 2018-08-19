using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace JustTag.Tagging
{
    /// <summary>
    /// Represents a file or folder path that can have tags.
    /// Does not necessarily point to an *existing* file or folder.
    /// This class is immutable.
    /// </summary>
    public class TaggedFilePath
    {
        private static Regex tagAreaRegex;

        public readonly string name;                // The filename, including the tags and the extension
        public readonly IEnumerable<string> tags;   // All of the tags belonging to this file
        public readonly string parentFolder;        // The full path to the parent folder
        public readonly string extension;           // The file extension(eg: .txt).  Includes the dot.
                                                    // If there is no extension, then it will be the empty string.
        public readonly bool isFolder;

        static TaggedFilePath()
        {
            // Build the tag area regex so we can use it to search for the tag area
            // when parsing a file path.
            string openBracket = @"\[";
            string closeBracket = @"\]";
            string validTagChar = @"[^\]]";

            string pattern = openBracket + validTagChar + "*" + closeBracket;

            tagAreaRegex = new Regex(pattern, RegexOptions.Compiled);
        }

        /// <summary>
        /// Parses the given file path
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="isFolder"></param>
        internal TaggedFilePath(string filePath, bool isFolder)
        {
            this.isFolder = isFolder;

            // Parse it into its parts using filesystem info
            FileSystemInfo parsed;

            if (isFolder)
                parsed = new DirectoryInfo(filePath);
            else
                parsed = new FileInfo(filePath);

            // Extract all the parts from the filesystem info
            name = parsed.Name;
            extension = parsed.Extension;
            parentFolder = parsed.GetParent().FullName;

            // Search for the tag area so we can extract the tags
            Match tagAreaMatch = tagAreaRegex.Match(name, 0);

            // If no tags were found, just set it to an empty array.
            if (!tagAreaMatch.Success)
            {
                this.tags = new string[] { };
                return;
            }

            // Get the tags from the tag area
            string tagArea = tagAreaMatch.Value;
            string withoutBrackets = tagArea.Substring(1, tagArea.Length - 2);

            this.tags = withoutBrackets.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Used internally for creating mutated versions of this file path
        /// </summary>
        private TaggedFilePath(string name, IEnumerable<string> tags, string parentFolder, string extension, bool isFolder)
        {
            this.name = name;
            this.tags = tags;
            this.parentFolder = parentFolder;
            this.extension = extension;
            this.isFolder = isFolder;
        }

        /// <summary>
        /// Creates a duplicate, but with the tags set to the given array.
        /// </summary>
        /// <param name="newTags"></param>
        /// <returns></returns>
        public TaggedFilePath SetTags(string[] newTags) => new TaggedFilePath
        (
            name,
            newTags,
            parentFolder,
            extension,
            isFolder
        );

        /// <summary>
        /// Constructs the full path to this file
        /// </summary>
        /// <returns></returns>
        public string GetFullPath() => throw new NotImplementedException();
    }
}
