using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;
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

        public string Name => beforeTags + GetTagArea() + afterTags;// The filename, including the tags and the extension
        public string ParentFolder { get; private set; }            // The full path to the parent folder
        public string Extension => Path.GetExtension(Name);         // The file extension(eg: .txt).  Includes the dot.
                                                                    // If there is no extension, then it will be the empty string.
        public bool IsFolder { get; private set; }                  // Whether or not this is a file or a folder.
        public string FullPath => Path.Combine(ParentFolder, Name);

        public IReadOnlyList<string> Tags => tags;  // All of the tags belonging to this file.

        private string beforeTags;  // The portion of Name before the tag area
        private string afterTags;   // The portion of Name after  the tag area.
        private string[] tags;      // The array backing the Tags property.
        private bool hasTagArea;    // Whether or not a pair of brackets [] was found in the file name

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
        public TaggedFilePath(string filePath, bool isFolder)
        {
            IsFolder = isFolder;

            // Parse it into its parts using filesystem info
            FileSystemInfo parsed;

            if (isFolder)
                parsed = new DirectoryInfo(filePath);
            else
                parsed = new FileInfo(filePath);

            // Get the parent folder
            ParentFolder = parsed.GetParent().FullName;

            // Search for the tag area so we can extract the tags
            Match match = tagAreaRegex.Match(parsed.Name, 0);
            hasTagArea = match.Success;

            // If no tags were found, just set it to an empty array.
            if (!hasTagArea)
            {
                tags = new string[] { };
                beforeTags = parsed.Name;
                afterTags = "";
                return;
            }

            // Get the sections of the name before and after the
            // tag area
            string tagArea = match.Value;
            beforeTags = parsed.Name.Substring(0, match.Index);
            afterTags = parsed.Name.Substring(match.Index + tagArea.Length);

            // Get the tags from the tag area
            string withoutBrackets = tagArea.Substring(1, tagArea.Length - 2);

            tags = withoutBrackets.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// So we can use object intializer syntax
        /// </summary>
        internal TaggedFilePath() { }

        /// <summary>
        /// Whether or not this path leads to an actual item on the file system
        /// </summary>
        /// <returns></returns>
        public bool Exists() => IsFolder ? Directory.Exists(FullPath) : File.Exists(FullPath);

        /// <summary>
        /// Creates a duplicate, but with the tags set to the given array.
        /// </summary>
        public TaggedFilePath SetTags(string[] newTags)
        {
            // Create a duplicate, but with the tags set to newTags.
            TaggedFilePath output = new TaggedFilePath
            {
                beforeTags = beforeTags,
                afterTags = afterTags,
                hasTagArea = newTags.Length == 0 ? false : true,
                tags = newTags,
                ParentFolder = ParentFolder,
                IsFolder = IsFolder
            };

            // If there isn't already a tag area, then we need to insert
            // one before the extension.
            if (!hasTagArea && Extension != "")
            {
                int lengthWithoutExt = Name.Length - Extension.Length;
                output.beforeTags = Name.Substring(0, lengthWithoutExt);
                output.afterTags = Extension;
            }

            // Return it
            return output;
        }

        /// <summary>
        /// Creates a duplicate, but with the following changes:
        ///     * The tag area is always at the end of the file name, but before the extension.
        ///     * The tags are always sorted in alphabetical order
        ///     * Duplicate tags are removed
        /// </summary>
        /// <returns></returns>
        public TaggedFilePath Normalize()
        {
            // Alphabetize and de-dupe the tags
            string[] normalizedTags = tags
                .Distinct()
                .OrderBy(tag => tag)
                .ToArray();

            // Remove the old tags, then add the new ones.
            // Removing the old tags first ensures that the new tags are put in the correct spot
            return this
                .SetTags(new string[] { })
                .SetTags(normalizedTags);
        }

        /// <summary>
        /// Converts it to a FileSystemInfo.
        /// This is to help migrate the project to TaggedFilePath.
        /// By the end of the migration, all uses of this should be
        /// gone and this function should be deleted.
        /// </summary>
        /// <returns></returns>
        public System.IO.FileSystemInfo ToFSInfo()
        {
            if (IsFolder)
                return new System.IO.DirectoryInfo(FullPath);
            else
                return new System.IO.FileInfo(FullPath);
        }

        private string GetTagArea()
        {
            // If there is no tag area, we can just do the empty string
            if (!hasTagArea)
                return "";

            var builder = new StringBuilder();

            // Start with the opening bracket
            builder.Append('[');

            // Add all the tags, separated by a space
            for (int i = 0; i < tags.Length; i++)
            {
                if (i != 0)
                    builder.Append(' ');

                builder.Append(tags[i]);
            }

            // Finish with the closing bracket
            builder.Append(']');
            return builder.ToString();
        }
    }
}
