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

        /// <summary>
        /// Searches the given folder for all children that match the given filter
        /// Folders are placed before files in the output
        /// </summary>
        /// <param name="folderPath">A path to a folder(NOT a file)</param>
        /// <param name="filter">The filter that all returned files must match</param>
        /// <param name="sortMethod">The method used to sort the output</param>
        /// <param name="descending"></param>
        /// <returns>An enumerable of TaggedFilePaths that match the given filter</returns>
        public static IEnumerable<TaggedFilePath> GetMatchingFiles(
            string folderPath,
            string filter,
            SortMethod sortMethod = SortMethod.name,
            bool descending = false)
        {
            TagFilter tagFilter = new TagFilter(filter);
            return GetMatchingFiles(folderPath, tagFilter, sortMethod, descending);
        }

        /// <summary>
        /// Searches the given folder for all children that match the given filter
        /// Folders are placed before files in the output
        /// </summary>
        /// <param name="folderPath">A path to a folder(NOT a file)</param>
        /// <param name="filter">The filter that all returned files must match</param>
        /// <param name="sortMethod">The method used to sort the output</param>
        /// <param name="descending"></param>
        /// <returns>An enumerable of TaggedFilePaths that match the given filter</returns>
        public static IEnumerable<TaggedFilePath> GetMatchingFiles(
            string folderPath,
            TagFilter filter,
            SortMethod sortMethod = SortMethod.name,
            bool descending = false)
        {
            // Decide which funciton will be used to sort the files in the list
            SortFunction sortFunction = sortMethod.GetSortFunction();

            // Get all files/folders that match the filter
            DirectoryInfo folder = new DirectoryInfo(folderPath);

            IEnumerable<TaggedFilePath> files =
                from FileInfo fsInfo in folder.EnumerateFiles()
                let path = new TaggedFilePath(fsInfo.FullName, false)
                where filter.Matches(path)
                orderby sortFunction(path) ascending
                select path;

            IEnumerable<TaggedFilePath> folders =
                from DirectoryInfo fsInfo in folder.EnumerateDirectories()
                let path = new TaggedFilePath(fsInfo.FullName, true)
                where filter.Matches(path)
                orderby sortFunction(path) ascending
                select path;

            // Sort them by descending, if the box is checked
            if (descending)
            {
                files = files.Reverse();
                folders = folders.Reverse();
            }

            // Combine the folders and files into the same list
            // Folders are added first for easy navigation
            return folders.Concat(files);
        }

        /// <summary>
        /// Repalces all instances of originalTag with newTag
        /// inside the specified folder
        /// </summary>
        /// <param name="folderPath">A path to a folder(NOT a file)</param>
        /// <param name="originalTag">The tag to be renamed</param>
        /// <param name="newTag">What the tag should be renamed to</param>
        public static void RenameTag(string folderPath, string originalTag, string newTag)
        {
            // Don't change anything if the original tag is the same as the new one
            if (originalTag == newTag)
                return;

            // Search for all the files with the original tag
            var filter = new TagFilter(originalTag);
            DirectoryInfo folder = new DirectoryInfo(folderPath);

            var files = folder.EnumerateFileSystemInfos()
                                .Select(f => new TaggedFilePath(f.FullName, f is DirectoryInfo))
                                .Where(f => filter.Matches(f));

            // Replace the original tag on each file that has it
            foreach (TaggedFilePath file in files)
            {
                List<string> tags = file.Tags.ToList();

                // We don't want files to end up with two of the same tag.
                // If the new tag is already present, then just delete
                // the old one.
                if (tags.Contains(newTag))
                {
                    tags.Remove(originalTag);
                    ChangeTagsAndSave(file, tags.ToArray());
                    continue;
                }

                // Replace the original tag with the new one, since we know
                // it won't produce a duplicate now.
                for (int i = 0; i < tags.Count; i++)
                {
                    if (tags[i] == originalTag)
                        tags[i] = newTag;
                }

                ChangeTagsAndSave(file, tags.ToArray());
            }
        }

        /// <summary>
        /// Removes all instances of the specified tag from
        /// the specified folder
        /// </summary>
        /// <param name="folderPath">A path to a folder(NOT a file)</param>
        /// <param name="tag">The tag to delete</param>
        public static void DeleteTag(string folderPath, string tag)
        {
            DirectoryInfo folder = new DirectoryInfo(folderPath);

            var filter = new TagFilter(tag);
            var filesToChange = folder.EnumerateFileSystemInfos()
                                    .Select(f => new TaggedFilePath(f.FullName, f is DirectoryInfo))
                                    .Where(f => filter.Matches(f));

            // Look at each file that has the tag
            foreach (TaggedFilePath file in filesToChange)
            {
                // Remove the tag from this file's list
                List<string> tags = file.Tags.ToList();
                tags.RemoveAll(t => t == tag);

                // Apply it to the disk
                ChangeTagsAndSave(file, tags.ToArray());
            }
        }
    }
}
