using System;
using System.Collections.Generic;
using System.Linq;
using JustTag.Tagging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaggingTests
{
    [TestClass]
    public class TaggedFilePathTests
    {
        private static void AssertTags(string fileName, params string[] expectedTags)
        {
            IEnumerable<string> actualTags = new TaggedFilePath(fileName, false).tags;
            Assert.IsTrue(actualTags.SequenceEqual(expectedTags));
        }

        [TestMethod]
        public void NoBrackets_NoTags_File() => AssertTags("foo.txt", new string[] { });

        [TestMethod]
        public void NoBracket_NoTags_Folder() => AssertTags("foo", new string[] { });

        [TestMethod]
        public void EmptyBrackets_NoTags_File() => AssertTags("foo[].txt", new string[] { });

        [TestMethod]
        public void EmptyBrackets_NoTags_Folder() => AssertTags("foo[]", new string[] { });

        [TestMethod]
        public void SingleTag() => AssertTags("name[tag].txt", "tag");

        [TestMethod]
        public void TwoTags() => AssertTags("name[foo bar].txt", "foo", "bar");

        [TestMethod]
        public void TagsAtBeginning_File() => AssertTags("[foo bar]name.txt", "foo", "bar");

        [TestMethod]
        public void TagsAtBeginning_Folder() => AssertTags("[foo bar]name", "foo", "bar");

        [TestMethod]
        public void TagsInMiddle_File() => AssertTags("na[foo bar]me.txt", "foo", "bar");

        [TestMethod]
        public void TagsInMiddle_Folder() => AssertTags("na[foo bar]me", "foo", "bar");
    }
}

