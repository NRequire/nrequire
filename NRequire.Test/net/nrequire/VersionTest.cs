using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace net.nrequire {
    [TestFixture]
    public class VersionTest {

        [Test]
        public void MajorOnly() {
            var v = Parse("1");
            Assert.AreEqual(1, v.Major);
            Assert.AreEqual(0, v.Minor);
            Assert.AreEqual(0, v.Revision);
            Assert.AreEqual("0", v.Qualifier);
            Assert.AreEqual(0, v.Build);

            Assert.IsFalse(v.IsTimestamped);
            Assert.IsFalse(v.IsSnapshot);
        }

        [Test]
        public void MajorMinor() {
            var v = Parse("1.2");
            Assert.AreEqual(1, v.Major);
            Assert.AreEqual(2, v.Minor);
            Assert.AreEqual(0, v.Revision);
            Assert.AreEqual("0", v.Qualifier);
            Assert.AreEqual(0, v.Build);

            Assert.IsFalse(v.IsTimestamped);
            Assert.IsFalse(v.IsSnapshot);
        }

        [Test]
        public void MajorMinorRevision() {
            var v = Parse("1.2.3");
            Assert.AreEqual(1, v.Major);
            Assert.AreEqual(2, v.Minor);
            Assert.AreEqual(3, v.Revision);
            Assert.AreEqual("0", v.Qualifier);
            Assert.AreEqual(0, v.Build);

            Assert.IsFalse(v.IsTimestamped);
            Assert.IsFalse(v.IsSnapshot);
        }

        [Test]
        public void ExactVersionParsed() {
            var v = Parse("1.2.3-20111124102345-456");
            Assert.AreEqual(1, v.Major);
            Assert.AreEqual(2, v.Minor);
            Assert.AreEqual(3, v.Revision);
            Assert.AreEqual("20111124102345", v.Qualifier);
            Assert.AreEqual(456, v.Build);

            Assert.IsTrue(v.IsTimestamped); 
            Assert.IsFalse(v.IsSnapshot);
        }

        [Test]
        public void SnapshotParsed() {
            var v = Parse("1.2.3-SNAPSHOT");
            Assert.AreEqual(1, v.Major);
            Assert.AreEqual(2, v.Minor);
            Assert.AreEqual(3, v.Revision);
            Assert.AreEqual("SNAPSHOT", v.Qualifier);
            Assert.AreEqual(0, v.Build);

            Assert.IsTrue(v.IsSnapshot);
            Assert.IsFalse(v.IsTimestamped);

        }

        private Version Parse(String s) {
            return Version.Parse(s);
        }

    }
}
