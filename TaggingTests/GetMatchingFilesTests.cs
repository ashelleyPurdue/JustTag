using System;
using System.Linq;
using JustTag.Tagging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaggingTests
{
    [TestClass]
    public class GetMatchingFilesTests
    {
        private static void AssertResults
        (
            string folderPath,
            string filter,
            params string[] expectedResults)
        {
            // Reset the testing directory before comparing, in case a previous test
            // changed it
            Utils.ResetTestFolder();

            // Get the actual results
            string fullPath = Utils.GetTestFolder(folderPath);
            TagFilter tagFilter = new TagFilter(filter);

            var actualResults = TagUtils.GetMatchingFiles(fullPath, tagFilter);

            // Convert them to their file names so we can compare them as strings
            var actualResultsPaths = actualResults.Select(f => f.Name);

            // Sort them before comparing them, because we don't care about order.
            actualResultsPaths = actualResultsPaths.OrderBy(s => s).ToArray();
            expectedResults = expectedResults.OrderBy(s => s).ToArray();

            // Compare them to the expected results
            bool matches = actualResultsPaths.SequenceEqual(expectedResults);
            Assert.IsTrue(matches);
        }



        [TestMethod]
        public void OnlyBaz() => AssertResults
        (
            "simple_cases",
            "baz",

            "file2[baz].txt",
            "file5[foo bar baz fizz buzz].txt",
            "file8[baz buzz foo].txt",
            "file9[baz fizz].txt",
            "file16[foo bar baz].txt",
            "file17[baz foo bar].txt"
        );

        [TestMethod]
        public void NotFoo() => AssertResults
        (
            "simple_cases",
            "-foo",

            "file1[bar].txt",
            "file2[baz].txt",
            "file6[fizz buzz].txt",
            "file7[bar fizz].txt",
            "file9[baz fizz].txt",
            "file10[bar].txt",
            "file11[bar].txt",
            "file18[fizz buzz].txt",
            "file19[fizz].txt"
        );

        [TestMethod]
        public void BarNotFoo() => AssertResults
        (
            "simple_cases",
            "bar -foo",

            "file1[bar].txt",
            "file7[bar fizz].txt",
            "file10[bar].txt",
            "file11[bar].txt"
        );

        [TestMethod]
        public void UntaggedFilter_File() => AssertResults
        (
            "untagged_filter_testing",
            ":untagged:",

            "untagged0.txt",
            "untagged1.txt",
            "untagged2.txt",
            "untagged3.txt"
        );

        [TestMethod]
        public void UntaggedFilter_Folder() => AssertResults
        (
            "folders",
            ":untagged:",

            "no_tags"
        );
    }
}
