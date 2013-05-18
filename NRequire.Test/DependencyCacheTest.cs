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
                .A(ModuleWith("A", "1.0").RequiresWishWith("C", "[1.0,1.2)"))
                .A(ModuleWith("A", "1.1").RequiresWishWith("D", "[1.0,1.1]"))
                .A(ModuleWith("B", "1.0").RequiresWishWith("E", "[1.0,1.1]"));

            var actual = cache.FindWishesFor(DepWith("A", "1.0"));

            Expect.That(actual)
                .Is(AList.InOrder().WithOnly(AWishWith("C", "[1.0,1.2)")));
        }

        [Test]
        public void FindDependenciesMatching() {
            var cache = NewDependencyCache.With()
                .A(ModuleWith("A", "1.0"))
                .A(ModuleWith("A", "1.1"))
                .A(ModuleWith("A", "1.3"))
                .A(ModuleWith("A", "2.0"));

            var actual = cache.FindDependenciesMatching(WishWith("A", "[1.1,2.0)"));

            Expect.That(actual)
                .Is(AList.InOrder().With(ADepWith("A", "1.3")).And(ADepWith("A", "1.1")));
        }


        [Test]
        public void ListVersionsInOrderTest() {
            var classifiersMatching = "key1-val1_key2-val2";
            var classifiersNonMatching = "key1-val1_key2-val2NonMatching";

            var cache = NewDependencyCache.With()
                .A(ModuleWith().Defaults().Version("1.0.0").Classifiers(classifiersMatching))
                .A(ModuleWith().Defaults().Version("1.2.3").Classifiers(classifiersMatching))
                .A(ModuleWith().Defaults().Version("1.2.4").Classifiers(classifiersNonMatching))
                .A(ModuleWith().Defaults().Version("2.3.4.SNAPSHOT").Classifiers(classifiersMatching))
            ;
            var deps = cache.FindDependenciesMatching(WishWith().Defaults().Classifiers(classifiersMatching));
         
            Expect
                .That(deps)
                    .Is(AList.InOrder().WithOnly(
                        ADepWith().Version("2.3.4.SNAPSHOT"),
                        ADepWith().Version("1.2.3"),
                        ADepWith().Version("1.0.0")));
        }

        [Test]
        public void DependenciesWithClassifiersAnyVersionForWish() {
            var classifiersMatching = "key1-val1_key2-val2";
            var classifiersNonMatching = "key1-val1_key2-val2NonMatching";

            var cache = NewDependencyCache.With()
                .A(ModuleWith().Defaults().Version("1.0.0").Classifiers(classifiersMatching))
                .A(ModuleWith().Defaults().Version("1.2.3").Classifiers(classifiersMatching))
                .A(ModuleWith().Defaults().Version("1.2.4").Classifiers(classifiersNonMatching))
                .A(ModuleWith().Defaults().Version("2.3.4.SNAPSHOT").Classifiers(classifiersMatching))
            ;
            var actual = cache.FindDependenciesMatching(NewWish.With().Defaults().Classifiers(classifiersMatching));

            Expect
                .That(actual)
                .Is(AList.InOrder()
                    .With(ADepWith().Version("2.3.4.SNAPSHOT"))
                    .And(ADepWith().Version("1.2.3"))
                    .And(ADepWith().Version("1.0.0")));
        }

        [Test]
        public void ListWishesForDependencyClassfiers() {
            var classifiersMatching = "key1-val1_key2-val2";
            var classifiersNonMatching = "key1-val1_key2-val2NonMatching";

            var cache = NewDependencyCache.With()
                .A(ModuleWith().Defaults().Version("1.0.0").Classifiers(classifiersMatching))
                .A(ModuleWith().Defaults().Version("1.2.3").Classifiers(classifiersMatching))
                .A(ModuleWith().Defaults().Version("2.0.0").Classifiers(classifiersNonMatching))
                .A(ModuleWith().Defaults().Version("1.2.5").Classifiers(classifiersNonMatching))
            ;
            var actual = cache.FindDependenciesMatching(NewWish.With().Defaults().Classifiers(classifiersMatching));

            Expect
                .That(actual)
                .Is(AList.InOrder()
                    .With(ADepWith().Version("1.2.3"))
                    .And(ADepWith().Version("1.0.0")));
        }

        [Test]
        public void ListWishesForDependencyVersion() {

            var cache = NewDependencyCache.With()
                .A(ModuleWith().Defaults().Version("1.0.0"))
                .A(ModuleWith().Defaults().Version("1.2.0"))
                .A(ModuleWith().Defaults().Version("1.2.5"))
                .A(ModuleWith().Defaults().Version("1.3.0"))
                .A(ModuleWith().Defaults().Version("2.0.0"))
            ;
            var actual = cache.FindDependenciesMatching(NewWish.With().Defaults().Version("[1.2,1.3)"));

            Expect
                .That(actual)
                .Is(AList.InOrder()
                    .With(ADepWith().Version("1.2.5"))
                    .And(ADepWith().Version("1.2.0")));
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
