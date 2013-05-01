using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace NRequire {
    [TestFixture]
    public class VersionTest {

        [Test]
        public void MajorOnly() {
            CheckParse(
                new V(1,0,0,null,0,Q.None),
                "1.0", "1.0.0", "1.0.0", "1.0.0.0");

            CheckInvalidParse("0", "1", "-1.0","1.-0");
        }

        [Test]
        public void MajorMinor() {
            CheckParse(
                new V(1, 2, 0, null, 0, Q.None),
                "1.2", "1.2.0", "1.2.0", "1.2.0.0");
            CheckInvalidParse("-1.2", "1.-2");
        }

        [Test]
        public void MajorMinorRevision() {
            CheckParse(
                new V(1, 2, 3, null, 0, Q.None),
                "1.2.3", "1.2.3.0");
            CheckInvalidParse("-1.2.3", "1.2.-3");
        }

        [Test]
        public void WithTimestamp() {
            //TODO:fail on 1.timestamp?
            CheckParse(
                new V(1, 0, 0, "20111124102345", 0, Q.Ts),
                "1.0.20111124102345", "1.0.0.20111124102345");
            CheckParse(
                new V(1, 2, 0, "20111124102345", 0, Q.Ts),
                "1.2.20111124102345", "1.2.0.20111124102345");

            CheckParse(
                new V(1, 2, 3, "20111124102345", 0, Q.Ts),
                "1.2.3.20111124102345");
        }

        [Test]
        public void WithSnapshot() {
            CheckParse(
                new V(1, 0, 0, "SNAPSHOT", 0, Q.Snapshot),
                "1.SNAPSHOT", "1.0.SNAPSHOT", "1.0.0.SNAPSHOT");
            CheckParse(
                new V(1, 2, 0, "SNAPSHOT", 0, Q.Snapshot),
                "1.2.SNAPSHOT", "1.2.0.SNAPSHOT");

            CheckParse(
                new V(1, 2, 3, "SNAPSHOT", 0, Q.Snapshot),
                "1.2.3.SNAPSHOT");
        }

        [Test]
        public void WithQualifier() {
            CheckParse(
                new V(1, 0, 0, "myqual", 0, Q.Other),
                "1.0.myqual", "1.0.0.myqual");
            CheckParse(
                new V(1, 2, 0, "myqual", 0, Q.Other),
                "1.2.myqual", "1.2.0.myqual");

            CheckParse(
                new V(1, 2, 3, "myqual", 0, Q.Other),
                "1.2.3.myqual");

            CheckParse(
                new V(1, 2, 3, "rc1", 0, Q.Other),
                "1.2.3.rc1");
            CheckParse(
                new V(1, 0, 0, "rc1", 0, Q.Other),
                "1.0.rc1", "1.0.0.rc1");
        }

        [Test]
        public void WithBuild() {
            CheckParse(
                new V(1, 2, 0, "456", 456, Q.Build),
                "1.2.0.456");

            CheckParse(
                new V(1, 2, 3, "456", 456, Q.Build),
                "1.2.3.456");

            CheckInvalidParse("1.0.0.123.456","1.0.foo.456","1.0.SNAPSHOT.456");
        }
        

        [Test]
        public void ToStringTests() {
            CheckToString("1.0.0", "1.0", "1.0.0", "1.0.0.0");
            CheckToString("1.2.0", "1.2", "1.2.0", "1.2.0.0");
            CheckToString("1.2.3", "1.2.3", "1.2.3.0");

            CheckToString("1.0.0.SNAPSHOT", "1.0.SNAPSHOT", "1.0.0.SNAPSHOT");
            CheckToString("1.2.0.SNAPSHOT", "1.2.SNAPSHOT", "1.2.0.SNAPSHOT");
            CheckToString("1.2.3.SNAPSHOT", "1.2.3.SNAPSHOT");
        }

        [Test]
        public void ToMatchStringTests() {
            CheckMatchString("0.0.0.0", "0.0", "0.0.0", "0.0.0.0"); 
            CheckMatchString("1.0.0.0", "1.0", "1.0.0", "1.0.0.0");
            CheckMatchString("1.2.0.0", "1.2", "1.2.0", "1.2.0.0");
            CheckMatchString("1.2.3.4", "1.2.3.4");
            CheckMatchString("1.2.3.SNAPSHOT", "1.2.3.SNAPSHOT");
            CheckMatchString("1.2.3.20120102100422", "1.2.3.20120102100422");
        }

        [Test]
        public void CompareToTests() {
            CheckCompareTo("0.0", 0, "0.0", "0.0.0", "0.0.0.0"); 
            CheckCompareTo("1.0", 0, "1.0", "1.0.0", "1.0.0.0");
            CheckCompareTo("1.2.3.4", 0, "1.2.3.4");
            CheckCompareTo("1.2.3.0", 0, "1.2.3");
            
            CheckCompareTo("1.1", 1, "1.0", "1.0.0", "1.0.0.0");
            CheckCompareTo("1.2.3", 1, "1.0", "1.2", "1.2.2");
            CheckCompareTo("1.2.3.4", 1, "1.2.3.3");
            CheckCompareTo("1.2.3.SNAPSHOT", 1, "1.2.3.3");
            CheckCompareTo("1.2.3.SNAPSHOT", 1, "1.2.3.20120103095436");

        }

        private void CheckParse(V expect, params String[] parseStrings) {
            foreach (var s in parseStrings) {
                var actual = Version.Parse(s);
                Assert.AreEqual(expect.Major, actual.Major, "invalid Major for " + s);
                Assert.AreEqual(expect.Minor, actual.Minor, "invalid Minor for " + s);
                Assert.AreEqual(expect.Revision, actual.Revision, "invalid Revision for " + s);
                Assert.AreEqual(expect.Qualifier, actual.Qualifier, "invalid Qualifier for " + s);
                Assert.AreEqual(expect.Build, actual.Build, "invalid Build for " + s);

                String msg = "invalid for " + s;
                switch (expect.Is) {
                    case Q.None:
                        Assert.IsFalse(actual.IsQualified, msg);
                        Assert.IsFalse(actual.IsBuild, msg);
                        Assert.IsFalse(actual.IsSnapshot, msg);
                        Assert.IsFalse(actual.IsTimestamped, msg);
                        break;
                    case Q.Build:
                        Assert.IsTrue(actual.IsQualified, msg);
                        Assert.IsTrue(actual.IsBuild, msg);
                        Assert.IsFalse(actual.IsSnapshot, msg);
                        Assert.IsFalse(actual.IsTimestamped, msg);
                        break;
                    case Q.Other:
                        Assert.IsTrue(actual.IsQualified, msg);
                        Assert.IsFalse(actual.IsBuild, msg);
                        Assert.IsFalse(actual.IsSnapshot, msg);
                        Assert.IsFalse(actual.IsTimestamped, msg);
                        break;
                    case Q.Snapshot:
                        Assert.IsTrue(actual.IsQualified, msg);
                        Assert.IsFalse(actual.IsBuild, msg);
                        Assert.IsTrue(actual.IsSnapshot, msg);
                        Assert.IsFalse(actual.IsTimestamped, msg);
                        break;
                    case Q.Ts:
                        Assert.IsTrue(actual.IsQualified, msg);
                        Assert.IsFalse(actual.IsBuild, msg);
                        Assert.IsFalse(actual.IsSnapshot, msg);
                        Assert.IsTrue(actual.IsTimestamped, msg);
                        break;
                }
            }
        }

        private void CheckInvalidParse(params String[] parseStrings) {
            foreach (var s in parseStrings) {
                ArgumentException thrown = null;
                try {
                    Version.Parse(s);
                } catch (ArgumentException e) {
                    thrown = e;
                }
                Assert.NotNull(thrown,"expected parse to fail for input:" + s);
            }

        }
        private void CheckToString(String expect,params String[] parseStrings) {
            foreach(var s in parseStrings){
                var actual = Version.Parse(s).ToString();
                Assert.AreEqual(expect, actual, "expect:" + expect + ", but was:" + actual + ", for:" + s);
            }
        }

        private void CheckMatchString(String expect, params String[] parseStrings) {
            foreach (var s in parseStrings) {
                var actual = Version.Parse(s).MatchString;
                Assert.AreEqual(expect, actual, "expect:" + expect + ", but was:" + actual + ", for:" + s);
            }
        }

        private void CheckCompareTo(String have, int expect, params String[] parseStrings) {
            var version = Version.Parse(have);
            foreach (var s in parseStrings) {
                var other = Version.Parse(s);
                var actual = version.CompareTo(other);

                Assert.AreEqual(expect, actual,
                    String.Format( "expect:{0}.CompareTo({1}) to equal {2} but was {3}",version,other,expect,actual));

                var actualReverse = other.CompareTo(version);

                Assert.AreEqual(-expect, actualReverse,
                    String.Format("expect:{0}.CompareTo({1}) to equal {2} but was {3}", other, version, -expect, actualReverse));

            }
        }

        private Version Parse(String s) {
            return Version.Parse(s);
        }

        enum Q {
            None, Snapshot, Ts, Build, Other
        }
        private class V {
            internal V() {
            }
            internal V(int major,int minor,int revision,string qual,int build, Q isQual) {
                Major = major;
                Minor = minor;
                Revision = revision;
                Qualifier = qual;
                Build = build;
                Is = isQual;
                
            }

            internal int Major { get; set; }
            internal int Minor { get; set; }
            internal int Revision { get; set; }
            internal int Build { get; set; }
            internal String Qualifier { get; set; }
            internal Q Is{ get; set; }


        }

    }
}
