using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;
using System.IO.Compression;

namespace TaggingTests
{
    public static class Utils
    {
        private const string TEST_FOLDER = "TestFiles";

        /// <summary>
        /// Resets the testing environment to its default state
        /// </summary>
        public static void ResetTestFolder()
        {
            // Delete the existing test folder
            if (Directory.Exists("TestFiles"))
                Directory.Delete("TestFiles", true);

            // Re-extract it
            Directory.CreateDirectory("TestFiles");
            ZipFile.ExtractToDirectory("TestDirectoryTemplate.zip", "TestFiles");
        }

        /// <summary>
        /// Appends TEST_FOLDER to the beginning of the folder name
        /// Does not include the trailing backslash.
        /// </summary>
        /// <param name="folder"></param>
        public static string GetTestFolder(string folderName)
            => Path.Combine(TEST_FOLDER, folderName);
    }
}
