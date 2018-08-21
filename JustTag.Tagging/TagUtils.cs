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
            SortFunction sortFunction = SortMethodExtensions.GetSortFunction(sortMethod);

            // Get all files/folders that match the filter
            DirectoryInfo folder = new DirectoryInfo(folderPath);

            IEnumerable<TaggedFilePath> files =
                from FileInfo fsInfo in folder.EnumerateFiles()
                let path = new TaggedFilePath(fsInfo.FullName, false)
                where filter.Matches(path)
                orderby sortFunction(new System.IO.FileInfo(fsInfo.FullName)) ascending   // TODO: Replace this with a TaggedFileName
                select path;

            IEnumerable<TaggedFilePath> folders =
                from DirectoryInfo fsInfo in folder.EnumerateDirectories()
                let path = new TaggedFilePath(fsInfo.FullName, true)
                where filter.Matches(path)
                orderby sortFunction(new System.IO.DirectoryInfo(fsInfo.FullName)) ascending   // TODO: Replace this with a TaggedFileName
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
    }
}
