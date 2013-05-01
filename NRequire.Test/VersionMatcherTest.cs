using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NRequire {
    [TestFixture]
    public class VersionMatcherTest {
        [Test]
        public void SimpleMatchTest() {
            Match("0", "0.0", "0.0.0", "0.0.0.123", "0.1", "0.2", "0.0.1", "0.2.3");
            Match("1", "1.0", "1.0.0", "1.0.0.123", "1.1", "1.2", "1.0.1", "1.2.3");

            NoMatch("1", "2.0", "2.1", "0.0", "0.1", "1.0.SNAPSHOT");
            NoMatch("2", "1.0");

            Match("0.0", "0.0", "0.0.0", "0.0.1", "0.0.0.0", "0.0.0.123", "0.0.2.345");
            Match("1.0", "1.0", "1.0.0", "1.0.1", "1.0.0.0", "1.0.0.123", "1.0.2.345");
            NoMatch("1.0","1.1", "1.2.3", "1.0.SNAPSHOT");
            
            Match("1.2", "1.2", "1.2.0", "1.2.1", "1.2.1.0", "1.2.1.456");
            NoMatch("1.2", "1.0", "1.1", "1.3", "1.2.SNAPSHOT", "1.2.0.SNAPSHOT");
            Match("1.0", "1.0", "1.0.1", "1.0.2", "1.0.2.0", "1.0.2.456");
            NoMatch("1.0", "1.1", "1.1.0");
            

            Match("0.0.0", "0.0.0", "0.0.0.123");
            Match("1.0.0", "1.0.0", "1.0.0.0", "1.0.0.123");
            NoMatch("1.0.0", "1.1.0", "1.0.1");
            Match("1.2.3", "1.2.3", "1.2.3.0", "1.2.3.456");
            NoMatch("1.2.3", "1.2.3.SNAPSHOT");

            Match("1.2.3.456", "1.2.3.456");
            NoMatch("1.2.3.456", "1.2.3", "1.2.3.0","1.2.3.SNAPSHOT");

            Match("1.SNAPSHOT", "1.0.SNAPSHOT", "1.0.0.SNAPSHOT", "1.1.SNAPSHOT", "1.2.SNAPSHOT", "1.2.3.SNAPSHOT");
            Match("1.SNAPSHOT", "1.0.20120103103523", "1.0.0.20120103103523", "1.2.20120103103523", "1.2.0.20120103103523", "1.2.3.20120103103523");
            NoMatch("1.SNAPSHOT", "2.0.20120103103523", "1.0.123", "1.2.456", "1.2.3.456");

            NoMatch("1.2.3.456", "1.2.3", "1.2.3.0", "1.2.3.SNAPSHOT");

            Match("1.2.SNAPSHOT", "1.2.0.SNAPSHOT", "1.2.0.20120103103523");
            NoMatch("1.2.SNAPSHOT", "1.3.SNAPSHOT", "1.2", "1.2.456", "1.2.0");
            
            Match("1.2.SNAPSHOT", "1.2.0.SNAPSHOT", "1.2.0.20120103103523");
            
            Match("1.2.3.SNAPSHOT", "1.2.3.SNAPSHOT", "1.2.3.20120103103523");
            NoMatch("1.2.3.SNAPSHOT", "1.2.3.456", "1.2.4.20120103103523", "1.2.4.SNAPSHOT", "1.2.1.SNAPSHOT", "2.0.0.20120103103523", "2.0.0.SNAPSHOT");

        }

//[ == inclusive
//( == exclusive
//(,1.0]            x <= 1.0
//1.0               "Soft" requirement on 1.0 (just a recommendation . helps select the correct version if it matches all ranges)
//[1.0]             Hard requirement on 1.0
//[1.2,1.3]         1.2 <= x <= 1.3
//[1.0,2.0)         1.0 <= x < 2.0
//[1.5,)            x >= 1.5
//(,1.0],[1.2,)     x <= 1.0 or x >= 1.2. Multiple sets are comma.separated
//(,1.1),(1.1,)     This excludes 1.1 if it is known not to work in combination with this library


        [Test]
        public void SimpleRangeMatchTest() {
            Match("(2,6)", "3.0", "4.0", "5.1");
            NoMatch("(2,6)", "1.0", "2.0", "6.0", "6.1");

            //Match("(2,)", "3.0", "4.0", "5.1");
            //NoMatch("(2,)", "1.0", "2.0", "2.1", "2.1.2");

        }

        [Test]
        public void RangeMatchTest() {
            Match("(2,)", "3.0", "4.0", "5.1");
            NoMatch("(2,)", "1.0", "2.0", "2.1", "2.1.2");

            Match("[2,)", "2.0", "3.0", "4.5.6");
            NoMatch("[2,)", "1.0");

            Match("(2.2,)", "2.3", "3.0", "6.7.8");
            NoMatch("(2.2,)", "1.2.3", "2.0", "2.2");
            
            Match("(2,4]", "3.0", "4.0", "4.1", "4.5.6.789");
            NoMatch("(2,4]", "0.0", "1.0","2.0","2.3.4", "5.0", "5.1", "5.6.7");


            Match("(2.1,4]", "2.2", "2.2.2", "2.2.2.2", "3.0", "3.0.0", "3.4.5", "4.0", "4.5.6");
            NoMatch("(2.1,4]", "1.0", "1.2.3", "2.0", "2.1", "2.1.1","2.1.2", "5.0", "5.0.0");
            
        }

        
        private void Match(String matchExp, params String[] versions) {
            var matcher = VersionMatcher.Parse(matchExp);
            foreach (var v in versions) {
                Assert.IsTrue(matcher.Match(v), "Expected match expr '" + matchExp + "', to match '" + v + "', using matcher " + matcher);
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
