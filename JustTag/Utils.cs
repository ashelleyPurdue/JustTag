﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using IWshRuntimeLibrary;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Windows;
using System.Text.RegularExpressions;

namespace JustTag
{
    public static class Utils
    {
        // Maps file extensions to icons
        private static Dictionary<string, ImageSource> fileIconCache = new Dictionary<string, ImageSource>();

        /// <summary>
        /// Follows a shortcut and returns the FileSystemInfo that it points to.
        /// </summary>
        /// <param name="shortcut"></param>
        /// <returns></returns>
        public static FileSystemInfo GetShortcutTarget(FileSystemInfo shortcut)
        {
            // Get the target path
            IWshShell shell = new WshShell();
            IWshShortcut lnk = shell.CreateShortcut(shortcut.FullName);
            string target = lnk.TargetPath;

            // Put it in a FileSystemInfo of the correct type
            if (Directory.Exists(target))
                return new DirectoryInfo(target);

            return new FileInfo(target);
        }

        /// <summary>
        /// Returns a shuffled version of the given list.
        /// What'd you expect?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static List<T> ShuffleList<T>(List<T> list)
        {
            Random randGen = new Random();
            List<T> originalList = new List<T>();
            List<T> outputList = new List<T>();

            originalList.AddRange(list);
            
            while(originalList.Count > 0)
            {
                int index = randGen.Next(originalList.Count);
                outputList.Add(originalList[index]);
                originalList.RemoveAt(index);
            }

            return outputList;
        }

        /// <summary>
        /// Returns an ImageSource with the given file's icon
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static ImageSource GetFileIcon(FileSystemInfo file)
        {
            // If it's already in the cache, just return it from there
            string ext = file.Extension.ToLower();

            if (fileIconCache.ContainsKey(ext))
                return fileIconCache[ext];

            // It's not in the cache, so load it.
            ImageSource imageSource;
            System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(file.FullName);

            using (System.Drawing.Bitmap bmp = icon.ToBitmap())
            {
                MemoryStream stream = new MemoryStream();
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                imageSource = BitmapFrame.Create(stream);
            }

            // Add it to the cache and return it
            fileIconCache.Add(ext, imageSource);
            return imageSource;
        }

        /// <summary>
        /// Shorthand for constructing a FormattedText object with the given control's font parameters
        /// </summary>
        /// <param name="text"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static FormattedText GetFormattedText(string text, System.Windows.Controls.Control control)
        {
            Typeface typeface = new Typeface
            (
                control.FontFamily,
                control.FontStyle,
                control.FontWeight,
                control.FontStretch
            );

            return new FormattedText
            (
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                control.FontSize,
                Brushes.Black
            );
        }

        /// <summary>
        /// Extracts all the tags from the given file name
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string[] GetFileTags(string fileName)
        {
            // This horrible regex just extracts everything in between '[' and ']'.
            string betweenBrackets = Regex.Match(fileName, @"\[([^)]*)\]").Groups[1].Value;

            // Split them by space
            return betweenBrackets.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Renames the given file so it has the given tags
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="newTags"></param>
        public static void ChangeFileTags(FileSystemInfo file, string[] newTags)
        {
            // Find the new name
            string newName = ChangeFileTags(file.Name, newTags);

            // Find the full path
            string parentPath;
            if (file is FileInfo)
                parentPath = (file as FileInfo).Directory.FullName;
            else
                parentPath = (file as DirectoryInfo).Parent.FullName;

            string newPath = System.IO.Path.Combine(parentPath, newName);

            // Rename it
            if (file is FileInfo)
                (file as FileInfo).MoveTo(newPath);
            else
                (file as DirectoryInfo).MoveTo(newPath);
        }

        /// <summary>
        /// Renames the given file so it has the given tags
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="newTags"></param>
        private static string ChangeFileTags(string fileName, string[] newTags)
        {
            // Get the stuff before and after the tags
            string beforeTags = fileName.Split('[', '.')[0];
            string extension = System.IO.Path.GetExtension(fileName);

            // If the new tags list is empty, don't even bother
            // with the brackets.
            if (newTags.Length == 0)
                return beforeTags + extension;

            // Make a new tag string from the array
            StringBuilder builder = new StringBuilder();

            builder.Append("[");
            for (int i = 0; i < newTags.Length; i++)
            {
                // Add a space if this isn't the first
                if (i != 0)
                    builder.Append(" ");

                // Add the tag
                builder.Append(newTags[i]);
            }
            builder.Append("]");

            // Jam them together to make the new filename
            return beforeTags + builder.ToString() + extension;
        }


    }
}
