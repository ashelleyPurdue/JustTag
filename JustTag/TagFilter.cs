using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustTag
{
    public class TagFilter
    {
        private List<string> requiredTags = new List<string>();
        private List<string> forbiddenTags = new List<string>();

        private bool untagged = false;

        public TagFilter(string filter)
        {
            // Parse the filter
            string[] filterWords = filter.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // HACK: If any of the words are ":untagged:", show only files without any tags.
            if (filterWords.Contains(":untagged:"))
            {
                untagged = true;
                return;
            }

            foreach (string word in filterWords)
            {
                // Sort each word into either forbidden or required tags
                // Anything with a '-' at the start means it's a forbidden tag.
                if (word[0] == '-')
                {
                    forbiddenTags.Add(word.Substring(1));
                    continue;
                }

                requiredTags.Add(word);
            }
        }

        /// <summary>
        /// Returns whether or not the given file name matches the tag filter
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool Matches(string fileName)
        {
            // Get all the tags from the filename
            string[] tags = Utils.GetFileTags(fileName);

            // HACK: If we're searching for untagged files, then
            // return true if there are no tags
            if (untagged)
                return tags.Length == 0;

            // Return false if any of the required tags are missing
            foreach (string t in requiredTags)
                if (!tags.Contains(t))
                    return false;

            // Return false if any of the forbidden tags are present
            foreach (string t in forbiddenTags)
                if (tags.Contains(t))
                    return false;

            // It passed the filter
            return true;
        }
    }
}
