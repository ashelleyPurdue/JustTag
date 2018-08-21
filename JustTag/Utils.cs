using System;
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
        public static HashSet<string> allKnownTags = new HashSet<string>();

        // Maps file extensions to icons
        private static Dictionary<string, ImageSource> fileIconCache = new Dictionary<string, ImageSource>();

        /// <summary>
        /// Like modulo, except it works with negative numbers
        /// </summary>
        /// <param name="index"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int WrapIndex(int index, int max)
        {
            // Positive numbers are easy
            if (index >= 0)
                return index % max;

            int output = index;
            while (output < 0)
                output += max;

            return output;
        }

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
        /// Returns if the given tag is valid.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool IsTagValid(string tag)
        {
            char[] forbiddenChars = new char[]
            {
                '[',    // Used to denote the start of the tags list
                ']',    // Used to denote the end of the tags list
                ':',    // Used as part of the ":untagged:" command.  Also it's not valid in a file name.
                '-',    // Used to exclude tags in filters
                ' ',    // Used to separate tags
            };

            // The empty string is not a valid tag.
            if (tag == "")
                return false;

            foreach (char c in tag)
            {
                if (forbiddenChars.Contains(c))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns if the given file is an image
        /// Just does a naive check of the file extention :(
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsImageFile(FileSystemInfo file)
        {
            if (!(file is FileInfo))
                return false;

            // Is this all of them?  I have this tingling
            // feeling I'm missing one.
            string[] formats = new string[]
            {
                ".jpg",
                ".jpeg",
                ".bmp",
                ".png",
                ".tiff",
                ".gif"
            };

            string ex = file.Extension.ToLower();
            return formats.Contains(ex);
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
    }
}
