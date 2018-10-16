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
        private static void AssertNormalized(string origName, string expectedName)
        {
            var orig = new TaggedFilePath(origName, false);
            var normalized = orig.Normalize();

            Assert.AreEqual<string>(expectedName, normalized.Name);
        }

        [TestMethod]
        public void AlreadyNormalized() => AssertNormalized("foo[a b c].txt", "foo[a b c].txt");
    }
}
