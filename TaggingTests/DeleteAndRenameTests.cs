using System;
using System.Linq;
using JustTag.Tagging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaggingTests
{
    [TestClass]
    public class DeleteAndRenameTests
    {
        private static void AssertDeleted(string folderName, string tag, params string[] expectedContents)
        {
            Utils.ResetTestFolder();
            string folderPath = Utils.GetTestFolder(folderName);

            // Delete the tag
            TagUtils.DeleteTag(folderPath, tag);

            // Assert that the folder contents matches
            var actualContents = TagUtils.GetMatchingFiles(folderPath, "")
                .Select(f => f.Name);

            bool matches = actualContents.SequenceEqual(expectedContents);
            Assert.IsTrue(matches);
        }

        private static void AssertRenamed(string folderPath, string originalTag, string newTag, params string[] expectedFolderContents)
        {
            Utils.ResetTestFolder();
            folderPath = Utils.GetTestFolder(folderPath);

            // Do the rename
            TagUtils.RenameTag(folderPath, originalTag, newTag);

            // Assert that the files in the folder exactly match what is expected
            var actualFolderContents = TagUtils.GetMatchingFiles(folderPath, "")
                                            .Select(f => f.Name);

            bool matches = expectedFolderContents.SequenceEqual(actualFolderContents);
            Assert.IsTrue(matches);
        }

        [TestMethod]
        public void DeleteFoo() => AssertDeleted
        (
            "find_replace",
            "foo",

            "file0.txt",
            "file1[bar].txt",
            "file2[bar].txt",
            "file3[bar].txt",
            "file4[baz].txt",
            "untagged.txt"
        );

        [TestMethod]
        public void FooToBar() => AssertRenamed
        (
            "find_replace",
            "foo",
            "bar",

            "file0[bar].txt",
            "file1[bar].txt",
            "file2[bar].txt",
            "file3[bar].txt",
            "file4[baz].txt",
            "untagged.txt"
        );

        [TestMethod]
        public void RenameToSelf_ShouldBeNoChange()
        {
            Utils.ResetTestFolder();
            string folderPath = Utils.GetTestFolder("find_replace");

            // Find out what the folder looked like before the rename
            var beforeContents = TagUtils.GetMatchingFiles(folderPath, "")
                .Select(f => f.Name);

            // Do the rename
            TagUtils.RenameTag(folderPath, "foo", "foo");

            // Assert that nothing changed
            var afterContents = TagUtils.GetMatchingFiles(folderPath, "")
                .Select(f => f.Name);

            bool matches = afterContents.SequenceEqual(beforeContents);

            Assert.IsTrue(matches);
        }
    }
}
