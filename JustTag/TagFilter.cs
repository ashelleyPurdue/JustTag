using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustTag
{
    public class TagFilter
    {
        public List<string> requiredTags;
        public List<string> forbiddenTags;

        public TagFilter(List<string> requiredTags, List<string> forbiddenTags)
        {
            this.requiredTags = requiredTags;
            this.forbiddenTags = forbiddenTags;
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
