using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace net.nrequire {
    [TestFixture]
    public class DependencyCacheTest {

        [Test]
        public void ListVersionsInOrderTest() {
            var resourceDir = FileUtil.DirectoryFor<DependencyCacheTest>();
            var localCacheDir = new DirectoryInfo(Path.Combine(resourceDir.FullName, "LocalCache"));

            var cache = new DependencyCache { CacheDir = localCacheDir };
            var versions = cache.GetVersionsMatching(new Dependency { Group = "Group0", Name = "Name0", ClassifiersString = "key1-val1_key2-val2" });
         
            Assert.AreEqual(Versions("2.3.4-SNAPSHOT","1.2.3","1.0.0"), versions);
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
