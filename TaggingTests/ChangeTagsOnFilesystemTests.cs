using System;
using System.Linq;
using System.Collections.Generic;
using JustTag.Tagging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alphaleonis.Win32.Filesystem;
using System.Text;

namespace TaggingTests
{
    [TestClass]
    public class ChangeTagsOnFilesystemTests
    {
        private static void AssertChanged(string origPath, bool isFolder, params string[] tags)
        {
            origPath = Utils.GetTestFolder(origPath);

            // Move the file
            var orig = new TaggedFilePath(origPath, isFolder);
            var changed = TagUtils.ChangeTagsAndSave(orig, tags);

            // Assert that it has been renamed.
            Assert.IsFalse(orig.Exists());
            Assert.IsTrue(changed.Exists());

            // Assert that the new path actually has the new tags
            bool matches = changed.Tags.SequenceEqual(tags);
            Assert.IsTrue(matches);
        }

        [TestInitialize]
        public void SetUp() => Utils.ResetTestFolder();

        [TestMethod]
        public void AddFirstTags_File() => AssertChanged
        (
            @"untagged_filter_testing\untagged0.txt",
            false,
            "foo",
            "bar",
            "baz"
        );

        [TestMethod]
        public void AddFirstTags_Folder() => AssertChanged
        (
            @"folders\no_tags",
            true,
            "foo",
            "bar",
            "baz"
        );

        [TestMethod]
        public void RemoveAllTags_File() => AssertChanged
        (
            @"simple_cases\file0[foo].txt",
            false
        // No tags
        );
        
        [TestMethod]
        public void CreateLongPath()
        {
            string origPath = "simple_cases\\file0[foo].txt";

            // Create a ridiculously-long tag
            // Sadly, a path component can only be 255 chars long, even WITH
            // LongPath support in play.  So we have a limit to how big the
            // tag we add can be.  However, the full path can be up to about
            // 3,000 chars long in total.  It's just the individual pieces that
            // need to be limited to 255.
            var tagBuilder = new StringBuilder();
            tagBuilder.Append("tag");

            int tagLen = 255 - "file0[].txt".Length;    // The filename is capped at 255
            while (tagBuilder.Length < tagLen)
                tagBuilder.Append('0');

            string bigTag = tagBuilder.ToString();

            // Add this huge tag to some poor, innocent file and assert
            // that it was done successfully.
            AssertChanged(origPath, false, bigTag);
        }
    }
}

