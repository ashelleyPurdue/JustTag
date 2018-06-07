using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JustTag
{
    public class TaggedFileName
    {
        private static Regex tagAreaRegex;

        public string beforeTags;   // The part of the filename before the tags
        public string afterTags;    // The part of the filename after the tags
        public readonly List<string> tags;

        static TaggedFileName()
        {
            // Build the tag area regex
            string openBracket = @"\[";
            string closeBracket = @"\]";
            string validTagChar = @"[^\]]";

            string pattern = openBracket + validTagChar + "*" + closeBracket;

            tagAreaRegex = new Regex(pattern, RegexOptions.Compiled);
        }

        /// <summary>
        /// Parses the given file name(NOT the full path, just the name!)
        /// </summary>
        /// <param name="fileName"></param>
        public TaggedFileName(string fileName)
        {
            // Search for the tag area.
            Match tagArea = tagAreaRegex.Match(fileName);

            // If no tag area was found, use default values
            if (!tagArea.Success)
            {
                beforeTags = fileName;
                afterTags = "";
                tags = new List<string>();

                return;
            }

            // Get the before and after
            beforeTags = fileName.Substring(0, tagArea.Index);
            afterTags = fileName.Substring(tagArea.Index + tagArea.Length);

            // Separate the tags and put them in the list
            string withBrackets = tagArea.Value;
            string withoutBrackets = withBrackets.Substring(1, withBrackets.Length - 2);

            tags = new List<string>(withoutBrackets.Split(' '));
        }

        /// <summary>
        /// Returns the final filename, with the tags included.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // If there are no tags, then just return the normal name
            if (tags.Count == 0)
                return beforeTags;

            StringBuilder builder = new StringBuilder();

            // Append the before tags stuff
            builder.Append(beforeTags);
            builder.Append('[');

            for (int i = 0; i < tags.Count; i++)
            {
                // Append the tag
                builder.Append(tags[i]);

                // If this isn't the last tag, append space
                if (i < tags.Count - 1)
                    builder.Append(' ');
            }

            // Append the after tags stuff
            builder.Append(']');
            builder.Append(afterTags);

            return builder.ToString();
        }
    }
}
