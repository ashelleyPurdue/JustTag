using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;

namespace JustTag.Tagging
{
    public class TagUtils
    {
        /// <summary>
        /// Changes the tags on the specified file and applies the change
        /// on the filesystem.  Returns the path to the renamed file.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public static TaggedFilePath ChangeTagsAndSave(TaggedFilePath file, string[] newTags)
        {
            // Get the new path for the file
            TaggedFilePath changed = file.SetTags(newTags);

            // Move the file on the filesystem
            if (file.IsFolder)
                Directory.Move(file.FullPath, changed.FullPath);
            else
                File.Move(file.FullPath, changed.FullPath);

            // Return the changed path
            return changed;
        }
    }
}
