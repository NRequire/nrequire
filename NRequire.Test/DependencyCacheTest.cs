using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using NRequire.Matcher;

namespace NRequire {
    [TestFixture]
    public class DependencyCacheTest : BaseDependencyTest {

        [Test]
        public void FindWishesForDep() {
            var cache = NewDependencyCache.With()
                .A(NewNode("A", "1.0").Wish("C", "[1.0,1.2)"))
                .A(NewNode("A", "1.1").Wish("D", "[1.0,1.1]"))
                .A(NewNode("B", "1.0").Wish("E", "[1.0,1.1]"));

            var actual = cache.FindWishesFor(Dep("A", "1.0"));

            var expect = ListWith(Wish("C", "[1.0,1.2)"));
            AssertAreEqual(expect, actual);
        }

        [Test]
        public void FindDependenciesMatching() {
            var cache = NewDependencyCache.With()
                .A(NewNode("A", "1.0"))
                .A(NewNode("A", "1.1"))
                .A(NewNode("A", "1.3"))
                .A(NewNode("A", "2.0"));

            var actual = cache.FindDependenciesMatching(Wish("A", "[1.1,2.0)"));

            var expect = ListWith(Dep("A", "1.3"),Dep("A", "1.1"));

            AssertAreEqual(expect, actual);
        }


        [Test]
        public void ListVersionsInOrderTest() {
            var classifiersMatching = "key1-val1_key2-val2";
            var classifiersNonMatching = "key1-val1_key2-val2NonMatching";

            var cache = NewDependencyCache.With()
                .Dependency(NewDependency.With().Defaults().Version("1.0.0").Classifiers(classifiersMatching))
                .Dependency(NewDependency.With().Defaults().Version("1.2.3").Classifiers(classifiersMatching))
                .Dependency(NewDependency.With().Defaults().Version("1.2.4").Classifiers(classifiersNonMatching))
                .Dependency(NewDependency.With().Defaults().Version("2.3.4.SNAPSHOT").Classifiers(classifiersMatching))
            ;
            var versions = cache.GetVersionsMatching(NewDependencyWish.With().Defaults().Classifiers(classifiersMatching));
         
            Assert.AreEqual(Versions("2.3.4.SNAPSHOT","1.2.3","1.0.0"), versions);
        }

        [Test]
        public void DependenciesForWish() {
            var classifiersMatching = "key1-val1_key2-val2";
            var classifiersNonMatching = "key1-val1_key2-val2NonMatching";

            var cache = NewDependencyCache.With()
                .Dependency(NewDependency.With().Defaults().Version("1.0.0").Classifiers(classifiersMatching))
                .Dependency(NewDependency.With().Defaults().Version("1.2.3").Classifiers(classifiersMatching))
                .Dependency(NewDependency.With().Defaults().Version("1.2.4").Classifiers(classifiersNonMatching))
                .Dependency(NewDependency.With().Defaults().Version("2.3.4.SNAPSHOT").Classifiers(classifiersMatching))
            ;
            var actual = cache.FindDependenciesMatching(NewDependencyWish.With().Defaults().Classifiers(classifiersMatching));

            Expect
                .That(actual)
                .Is(AList
                    .With(ADependency.With().Version("2.3.4.SNAPSHOT"))
                    .And(ADependency.With().Version("1.2.3"))
                    .And(ADependency.With().Version("1.0.0")));
        }


       
        [Test]
        public void ListWishesForDependency() {

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
