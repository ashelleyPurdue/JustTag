using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using IWshRuntimeLibrary;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;

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
            ImageSource imageSource;
            Icon icon = Icon.ExtractAssociatedIcon(file.FullName);

            using (Bitmap bmp = icon.ToBitmap())
            {
                MemoryStream stream = new MemoryStream();
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                imageSource = BitmapFrame.Create(stream);
            }

            return imageSource;
        }
    }
}
