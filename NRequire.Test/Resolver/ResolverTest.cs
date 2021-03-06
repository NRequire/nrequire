﻿using System.Linq;
using NRequire.Logging;
using NUnit.Framework;

namespace NRequire.Resolver {
    [TestFixture]
    public class ResolverTest : BaseDependencyTest {
        [Test]
        public void ResolveToLatestMatchingVersion() {
            var cache = NewDependencyCache.With()
                .Dependencies(ModuleWith().Defaults().Versions("1.2.3","1.2.4","1.2.5","1.2.6", "1.2.7"));
            var resolver = ProjectDependencyResolver.WithCache(cache);

            var soln = NewSolution.With()
                //exclude 1.2.7
                .Wish(NewWish.With().Defaults().Version("[1.2.2,1.2.7)"));
            var proj = NewProject.With()
                //version should be merged in from soln
                .RuntimeWish(NewWish.With().Defaults());

            soln.AfterLoad();
            proj.AfterLoad();
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
            var resolver = ProjectDependencyResolver.WithCache(cache);

            var soln = NewSolution.With()
                //exclude 1.2.3
                .Wish(NewWish.With().Defaults().Version("1.2.4"));

            var proj = NewProject.With()
                .RuntimeWish(NewWish.With().Defaults());//should grab version from soln

            soln.AfterLoad();
            proj.AfterLoad();

            var resolvedDeps = resolver.MergeAndResolveDependencies(soln, proj);
            Assert.AreEqual(1, resolvedDeps.Count);
            var actual = resolvedDeps.FirstOrDefault();

        }
    }
}
