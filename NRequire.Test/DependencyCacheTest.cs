using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace NRequire {
    [TestFixture]
    public class DependencyCacheTest {

        [Test]
        public void ListVersionsInOrderTest() {
            var classifiersMatching = "key1-val1_key2-val2";
            var classifiersNonMatching = "key1-val1_key2-val2NonMatching";

            var cache = ADependencyCache.With()
                .Dependency(ADependency.With().Defaults().Version("1.0.0").Classifiers(classifiersMatching))
                .Dependency(ADependency.With().Defaults().Version("1.2.3").Classifiers(classifiersMatching))
                .Dependency(ADependency.With().Defaults().Version("1.2.4").Classifiers(classifiersNonMatching))
                .Dependency(ADependency.With().Defaults().Version("2.3.4.SNAPSHOT").Classifiers(classifiersMatching))
            ;
            var versions = cache.GetVersionsMatching(ADependencyWish.With().Defaults().Classifiers(classifiersMatching));
         
            Assert.AreEqual(Versions("2.3.4.SNAPSHOT","1.2.3","1.0.0"), versions);
        }

        private static IList<Version> Versions(params String[] versionStrings) {
            var list = new List<Version>();
            foreach (var s in versionStrings) {
                list.Add(Version.Parse(s));
            }
            return list;
        }
    }
}
