using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace net.nrequire {
    [TestFixture]
    public class VersionMatcherTest {
        [Test]
        public void MatchTest() {
            Match("1", "1.0");
            Match("1", "1.0.0");
            Match("1", "1.0.0-0-123");
            Match("1", "1.0.0-123");

            Match("1.0", "1.0");
            Match("1.0", "1.0.0");
            
            NoMatch("1.0", "1.1", "2.0", "0.0");

            Match("1.0.0", "1.0.0");

        }

        private void Match(String matchExp, params String[] versions) {
            var matcher = VersionMatcher.Parse(matchExp);
            foreach (var s in versions) {
                Assert.IsTrue(matcher.Match(s), "Expected match expr '" + matchExp + "', to match '" + s + "'");
            }
        }

        private void NoMatch(String matchExp, params String[] versions) {
            var matcher = VersionMatcher.Parse(matchExp);
            foreach (var s in versions) {
                Assert.IsFalse(matcher.Match(s), "Expected match expr '" + matchExp + "', to NOT match '" + s + "'");
            }
        }
    }
}
