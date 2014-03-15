using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Version = NRequire.Model.Version;

namespace NRequire.Matcher {

    [TestFixture]
    public class ExactMatcherTest {

        [Test]
        public void MajorTest() {
            Match('=', "2", "2.0", "2.3","2.3.4");
            Match('[', "2", "2.0", "2.3", "3.0", "3.4.5", "3.4.5.6");
            Match('(', "2", "3.0");
        }


        [Test]
        public void MajorMinorRevisionTest() {
            Match('=', "2.3.4", "2.3.4");
            Match('[', "2.3.4", "2.3.4");
            Match('(', "2.3.4", "2.3.5");

        }

        private void Match(char symbol, string s, params String[] versions) {
            var matcher = ExactMatcher.Parse(symbol, s);
            foreach( var v in versions){
                if(!matcher.Match(Version.Parse(v))){
                    Assert.Fail("Did not match {0} using match string {1} and symbol {2}", v,s, symbol);
                }
            }
        }
    }
}
