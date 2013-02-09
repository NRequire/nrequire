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
            Match("1", "1.0", "1.0.0","1.0.0-123", "1.1", "1.2", "1.0.1", "1.2.3" );
            NoMatch("1", "2.0", "2.1", "0.0", "0.1", "1.0-SNAPSHOT");

            Match("1.0", "1.0", "1.0.0", "1.0.1", "1.0.0-0","1.0.0-123", "1.0.2-345");
            NoMatch("1.0","1.1", "1.2.3", "1.0-SNAPSHOT");
            
            Match("1.2", "1.2", "1.2.0", "1.2.1", "1.2.1-0", "1.2.1-456");
            NoMatch("1.2", "1.0", "1.1", "1.3", "1.2-SNAPSHOT", "1.2.0-SNAPSHOT");
            Match("1.0", "1.0", "1.0.1", "1.0.2", "1.0.2-0", "1.0.2-456");
            NoMatch("1.0", "1.1", "1.1.0");
            

            Match("1.0.0", "1.0.0");
            NoMatch("1.0.0", "1.1.0", "1.0.1");
            Match("1.2.3", "1.2.3", "1.2.3-0", "1.2.3-456");
            NoMatch("1.2.3", "1.2.3-SNAPSHOT");

            Match("1.2.3-456", "1.2.3-456");
            NoMatch("1.2.3-456", "1.2.3", "1.2.3-0","1.2.3-SNAPSHOT");

            Match("1-SNAPSHOT", "1.0-SNAPSHOT", "1.0.0-SNAPSHOT", "1.1-SNAPSHOT", "1.2-SNAPSHOT", "1.2.3-SNAPSHOT");
            Match("1-SNAPSHOT", "1.0-20120103103523", "1.0.0-20120103103523", "1.2-20120103103523", "1.2.0-20120103103523", "1.2.3-20120103103523");
            NoMatch("1-SNAPSHOT", "2.0-20120103103523", "1.0-123", "1.2-456", "1.2.3-456");

            NoMatch("1.2.3-456", "1.2.3", "1.2.3-0", "1.2.3-SNAPSHOT");

            Match("1.2-SNAPSHOT", "1.2.0-SNAPSHOT", "1.2.0-20120103103523");
            NoMatch("1.2-SNAPSHOT", "1.3-SNAPSHOT", "1.2", "1.2-456", "1.2.0");
            
            Match("1.2-SNAPSHOT", "1.2.0-SNAPSHOT", "1.2.0-20120103103523");
            
            Match("1.2.3-SNAPSHOT", "1.2.3-SNAPSHOT", "1.2.3-20120103103523");
            NoMatch("1.2.3-SNAPSHOT", "1.2.3-456", "1.2.4-20120103103523", "1.2.4-SNAPSHOT", "1.2.1-SNAPSHOT", "2.0.0-20120103103523", "2.0.0-SNAPSHOT");

        }

        private void Match(String matchExp, params String[] versions) {
            var matcher = VersionMatcher.Parse(matchExp);
            foreach (var v in versions) {
                Assert.IsTrue(matcher.Match(v), "Expected match expr '" + matchExp + "', to match '" + v + "'");
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
