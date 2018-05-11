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
        public string beforeTags;   // The part of the filename before the tags
        public string afterTags;    // The part of the filename after the tags
        public List<string> tags;   // The tags.  Will be null if no tag area is found

        /// <summary>
        /// Parses the given file name(NOT the full path, just the name!)
        /// </summary>
        /// <param name="fileName"></param>
        public TaggedFileName(string fileName)
        {
            // Search for the tag area.
            Match tagArea = Regex.Match(fileName, @"\[.*\]");

            // If no tag area was found, use default values
            if (!tagArea.Success)
            {
                beforeTags = fileName;
                afterTags = "";
                tags = null;

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
        public string ToString()
        {
            // If there is no tag area, then just return beforeTags
            if (tags == null)
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
