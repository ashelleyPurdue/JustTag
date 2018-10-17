using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JustTag.Tagging;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace TaggingTests
{
    [TestClass]
    public class NormalizeTests
    {
        private static void Check(string origName, string expectedName)
        {
            string normalized = new TaggedFilePath(origName, false).GetNormalizedName();
            Assert.AreEqual<string>(expectedName, normalized);
        }

        [TestMethod]
        public void AlreadyNormalized() => Check
        (
            "foo[a b c].txt",
            "foo[a b c].txt"
        );

        [TestMethod]
        public void AlreadyNormalizedNoExtension() => Check
        (
            "foo[a b c]",
            "foo[a b c]"
        );

        [TestMethod]
        public void Duplicates() => Check
        (
            "foo[a a b c].txt",
            "foo[a b c].txt"
        );

        [TestMethod]
        public void WrongOrder() => Check
        (
            "foo[b a c].txt",
            "foo[a b c].txt"
        );

        [TestMethod]
        public void AtBeginning() => Check
        (
            "[a b c]foo.txt",
            "foo[a b c].txt"
        );

        [TestMethod]
        public void InMiddle() => Check
        (
            "fo[a b c]o.txt",
            "foo[a b c].txt"
        );

        [TestMethod]
        public void BeginningOfExtension() => Check
        (
           "foo.[a b c]txt",
           "foo[a b c].txt"
        );

        [TestMethod]
        public void MiddleOfExtension() => Check
        (
            "foo.t[a b c]xt",
            "foo[a b c].txt"
        );

        [TestMethod]
        public void AfterExtension() => Check
        (
            "foo.txt[a b c]",
            "foo[a b c].txt"
        );
    }
}
