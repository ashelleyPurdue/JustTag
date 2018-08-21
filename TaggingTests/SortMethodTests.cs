using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using JustTag.Tagging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaggingTests
{
    [TestClass]
    public class SortMethodTests
    {
        /// <summary>
        /// Just like AssertResults, but it also tests a given sort method as well.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="sortMethod"></param>
        /// <param name="filter"></param>
        /// <param name="expectedResults"></param>
        private static void AssertSorted
        (
            string folderPath,
            string filter,
            SortMethod sortMethod,
            bool descending,
            params string[] expectedResults)
        {
            // Reset the testing directory before comparing, in case a previous test
            // changed it
            Utils.ResetTestFolder();

            // Get the actual results
            string    fullPath  = Utils.GetTestFolder(folderPath);
            TagFilter tagFilter = new TagFilter(filter);

            var actualResults =
                TagUtils.GetMatchingFiles(fullPath, tagFilter, sortMethod, descending)
                .Select(f => f.Name);

            // Compare them to the expected results
            bool matches = actualResults.SequenceEqual(expectedResults);
            Assert.IsTrue(matches);
        }

        [TestMethod]
        public void AlphabeticSorting() => AssertSorted
        (
            "alphabet_sorting",
            "",
            SortMethod.name,
            false,

            "aa.txt",
            "ab.txt",
            "ac.txt",
            "ba.txt",
            "bb.txt",
            "bc.txt",
            "ca.txt",
            "cb.txt",
            "cc.txt"
        );

        [TestMethod]
        public void ComicSort() => AssertSorted
        (
            "untagged_filter_testing",
            ":untagged:",
            SortMethod.comic,
            false,

            "untagged0.txt",
            "untagged1.txt",
            "untagged2.txt",
            "untagged3.txt"
        );

        [TestMethod]
        public void CreationDateSort_File()
        {
            // We need to create our own test files for this,
            // because zip archives don't store the creation date.

            // Create the test folder
            string targetDir = Utils.GetTestFolder("creation_date_sort_files");

            if (Directory.Exists(targetDir))
                Directory.Delete(targetDir, true);

            Directory.CreateDirectory(targetDir);

            // Create all the files in the order we want them sorted
            List<string> files = new List<string>();
            DateTime baseTime = DateTime.Now;

            for (int i = 0; i < 10; i++)
            {
                string fname = Path.GetRandomFileName();
                string path = Path.Combine(targetDir, fname);

                files.Add(fname);
                File.Create(path).Close();
                File.SetCreationTime(path, baseTime.AddDays(i));
            }

            // Test the sorting
            var sorted = TagUtils.GetMatchingFiles(targetDir, new TagFilter(""), SortMethod.date)
                .Select(f => f.Name);

            bool matches = files.SequenceEqual(sorted);
            Assert.IsTrue(matches);
        }

        [TestMethod]
        public void CreationDateSort_Folder()
        {
            // We need to create our own test folders for this,
            // because zip archives don't store the creation date.
            // TODO: Find some way to fix the duplicated code, somehow.

            // Create the test folder
            string targetDir = Utils.GetTestFolder("creation_date_sort_files");

            if (Directory.Exists(targetDir))
                Directory.Delete(targetDir, true);

            Directory.CreateDirectory(targetDir);

            // Create all the files in the order we want them sorted
            List<string> folders = new List<string>();
            DateTime baseTime = DateTime.Now;

            for (int i = 0; i < 10; i++)
            {
                string fname = Path.GetRandomFileName().Split('.')[0];  // Removes everything after the '.'
                string path = Path.Combine(targetDir, fname);

                folders.Add(fname);
                Directory.CreateDirectory(path);
                Directory.SetCreationTime(path, baseTime.AddDays(i));
            }

            // Test the sorting
            var sorted = TagUtils.GetMatchingFiles(targetDir, new TagFilter(""), SortMethod.date)
                .Select(f => f.Name);

            bool matches = folders.SequenceEqual(sorted);
            Assert.IsTrue(matches);
        }

        [TestMethod]
        public void DescendingReversesSortOrder()
        {
            string targetDir = Utils.GetTestFolder("simple_cases");
            Utils.ResetTestFolder();

            // Check every sort method to see if the "descending" toggle
            // produces the reverse.
            foreach (SortMethod sortMethod in Enum.GetValues(typeof(SortMethod)))
            {
                // Don't check "shuffle", because it's supposed to be random
                // and there's no concept of "descending" a random order.
                if (sortMethod == SortMethod.shuffle)
                    continue;

                var ascending  = TagUtils.GetMatchingFiles(targetDir, "", sortMethod, false).Select(f => f.Name);
                var descending = TagUtils.GetMatchingFiles(targetDir, "", sortMethod, true).Select(f => f.Name);

                bool matches = ascending.SequenceEqual(descending.Reverse());
                Assert.IsTrue(matches);
            }
        }
    }
}
