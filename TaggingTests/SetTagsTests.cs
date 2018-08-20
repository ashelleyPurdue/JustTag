using System;
using System.Collections.Generic;
using System.Linq;
using JustTag.Tagging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaggingTests
{
    [TestClass]
    public class SetTagsTests
    {
        private static void AssertChanged(string origName, string expectedName, params string[] tags)
        {
            TaggedFilePath origPath = new TaggedFilePath(origName, false);
            TaggedFilePath changedPath = origPath.SetTags(tags);

            Assert.AreEqual(expectedName, changedPath.Name);
        }

        [TestMethod]
        public void AddFirstEverTags_NoBrackets() => AssertChanged
        (
            "file.txt",
            "file[tag0 tag1].txt",
            "tag0",
            "tag1"
        );

        [TestMethod]
        public void AddFirstEverTags_EmptyBrackets() => AssertChanged
        (
            "file[].txt",
            "file[tag0 tag1].txt",
            "tag0",
            "tag1"
        );

        [TestMethod]
        public void AddToExistingTags() => AssertChanged
        (
            "file[tag0 tag1].txt",
            "file[tag0 tag1 tag2].txt",
            "tag0",
            "tag1",
            "tag2"
        );

        [TestMethod]
        public void ChangeExistingTag() => AssertChanged
        (
            "file[tag0 tag1].txt",
            "file[tag0 foo].txt",
            "tag0",
            "foo"
        );

        [TestMethod]
        public void ChangeTagsInMiddle_TagAreaShouldBeInSamePlace() => AssertChanged
        (
            "fi[tag0 tag1]le.txt",
            "fi[foo bar]le.txt",
            "foo",
            "bar"
        );

        [TestMethod]
        public void ChangeTagsAtStart_TagAreaShouldBeInSamePlace() => AssertChanged
        (
            "[tag0 tag1]file.txt",
            "[foo bar]file.txt",
            "foo",
            "bar"
        );

        [TestMethod]
        public void RemoveAllTags_ShouldLeaveNoBrackets() => AssertChanged
        (
            "file[tag0 tag1 tag2 tag3].txt",
            "file.txt"
        );

        [TestMethod]
        public void RemoveAllTagsInMiddle_ShouldLeaveNoBrackets() => AssertChanged
        (
            "fi[tag0 tag1]le.txt",
            "file.txt"
        );
    }
}

