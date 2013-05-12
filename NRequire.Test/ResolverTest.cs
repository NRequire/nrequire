using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace NRequire {
    [TestFixture]
    public class ResolverTest {
        [Test]
        public void ResolveToLatestMatchingVersion() {
            var cache = ADependencyCache.With()
                .Dependencies(ADependency.With().Defaults().Versions("1.2.3","1.2.4","1.2.5","1.2.6", "1.2.7"));
            var resolver = DependencyResolverV1.WithCache(cache);

            var soln = ASolution.With()
                //exclude 1.2.7
                .Dependency(ADependencyWish.With().Defaults().Version("[1.2.2,1.2.7)"));
            var proj = AProject.With()
                .CompileDependency(ADependencyWish.With().Defaults());
            
            var resolvedDeps = resolver.ResolveDependencies(soln, proj);
            Assert.AreEqual(1, resolvedDeps.Count);
            var actual = resolvedDeps.FirstOrDefault();

            var def = ADependencyWish.With().Defaults();

            Assert.AreEqual("1.2.6", actual.Version.ToString());
            Assert.AreEqual(def.Group, actual.Group);
            Assert.AreEqual(def.Name, actual.Name);
            Assert.AreEqual(def.Runtime, actual.Runtime);
            Assert.AreEqual(def.Arch, actual.Arch);
            Assert.AreEqual(def.Ext, actual.Ext);
        }

        [Test]
        public void ResolveTransitiveDepsTest() {
            Logger.SetLevel(Logger.Level.Trace);
            var cache = ADependencyCache.With()
                .Dependencies(ADependency.With().Defaults().Versions("1.2.3", "1.2.4"));
            var resolver = DependencyResolverV1.WithCache(cache);

            var soln = ASolution.With()
                //exclude 1.2.7
                .Dependency(ADependencyWish.With().Defaults().Version("1.2.4")
                
                );
            var proj = AProject.With()
                .CompileDependency(ADependencyWish.With().Defaults());

            var resolvedDeps = resolver.ResolveDependencies(soln, proj);
            Assert.AreEqual(1, resolvedDeps.Count);
            var actual = resolvedDeps.FirstOrDefault();

        }
        private static IList<Dependency> Deps(params Dependency[] deps) {
            return new List<Dependency>(deps);
        }
    }
}
