using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace NRequire {
    [TestFixture]
    public class ResolverTest : BaseDependencyTest {
        [Test]
        public void ResolveToLatestMatchingVersion() {
            var cache = NewDependencyCache.With()
                .Dependencies(ModuleWith().Defaults().Versions("1.2.3","1.2.4","1.2.5","1.2.6", "1.2.7"));
            var resolver = ProjectResolver.WithCache(cache);

            var soln = NewSolution.With()
                //exclude 1.2.7
                .Dependency(NewWish.With().Defaults().Version("[1.2.2,1.2.7)"));
            var proj = NewProject.With()
                //version should be merged in from soln
                .RuntimeWish(NewWish.With().Defaults());
            
            var resolvedDeps = resolver.MergeAndResolveDependencies(soln, proj);
            Assert.AreEqual(1, resolvedDeps.Count);
            var actual = resolvedDeps.FirstOrDefault();

            var def = NewWish.With().Defaults().Build();

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
            var cache = NewDependencyCache.With()
                .Dependencies(ModuleWith().Defaults().Versions("1.2.3", "1.2.4"));
            var resolver = ProjectResolver.WithCache(cache);

            var soln = NewSolution.With()
                //exclude 1.2.3
                .Dependency(NewWish.With().Defaults().Version("1.2.4"));

            var proj = NewProject.With()
                .RuntimeWish(NewWish.With().Defaults());//should grab version from soln

            var resolvedDeps = resolver.MergeAndResolveDependencies(soln, proj);
            Assert.AreEqual(1, resolvedDeps.Count);
            var actual = resolvedDeps.FirstOrDefault();

        }
    }
}
