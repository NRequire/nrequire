using System;
using System.IO;
using NUnit.Framework;
using TestFirst.Net;
using TestFirst.Net.Matcher;

namespace NRequire
{
    [TestFixture]
    public class NewDependencyCacheTest : BaseDependencyTest
    {
        [Test]
        public void DefaultsCorrectFileLocationsUsed()
        {
            var cache = NewDependencyCache.With()
                .A(ModuleWith().Defaults().Version("1.2.3")
                    .RuntimeWishWith("A","2.0"));

            var cacheDir = cache.GetCacheDir();

            AssertExists(cacheDir, "MyGroup/MyName/1.2.3/arch-any_runtime-any/MyName.dll");
            AssertExists(cacheDir, "MyGroup/MyName/1.2.3/arch-any_runtime-any/MyName.nrequire.module.json");

            var wishes = cache.FindWishesFor(DepWith().Defaults().Version("1.2.3"));
            Expect.That(wishes)
                .Is(AList.InOrder().WithOnly(AWishWith()
                    .Name("A").Group(TestDefaults.Group).Runtime(TestDefaults.Runtime.ToLower()).Arch(TestDefaults.Arch.ToLower()).Ext(TestDefaults.Ext).Version("2.0")));

            var deps = cache.FindDependenciesMatching(WishWith()
                   .Group(TestDefaults.Group).Name(TestDefaults.Name).Runtime(TestDefaults.Runtime).Arch(TestDefaults.Arch));

            Expect.That(deps)
                .Is(AList.WithOnly(ADepWith()
                       .Group(TestDefaults.Group).Name(TestDefaults.Name).Runtime(TestDefaults.Runtime.ToLower()).Arch(TestDefaults.Arch.ToLower()).Version("1.2.3")));

        }

        [Test]
        public void CorrectFileLocationsUsed()
        {
            var cache = NewDependencyCache.With()
                .A(ModuleFrom("SomeGroup:SomeName:3.0:SomeExt:Arch-SomeArch_RunTime-SomeRuntime")
                   .RuntimeWishWith(WishFrom("OtherGroup:OtherName:1.0:OtherExt:Arch-OtherArch_RunTime-OtherRuntime"))
                   .RuntimeWishWith(WishWith().Name("OtherName2").Group("OtherGroup2").Ext("OtherExt2").Arch("OtherArch2").Runtime("OtherRuntime2").Version("1.2"))
            );

            var cacheDir = cache.GetCacheDir();

            AssertExists(cacheDir, "SomeGroup/SomeName/3.0.0/arch-SomeArch_runtime-SomeRuntime/SomeName.SomeExt");
            AssertExists(cacheDir, "SomeGroup/SomeName/3.0.0/arch-SomeArch_runtime-SomeRuntime/SomeName.nrequire.module.json");

            var wishes = cache.FindWishesFor(DepWith().Name("SomeName").Group("SomeGroup").Ext("SomeExt").Arch("somearch").Runtime("someruntime").Version("3.0"));
            Expect.That(wishes)
                .Is(AList.InOrder()
                    .WithOnly(AWishWith().Name("OtherName").Group("OtherGroup").Ext("OtherExt").Arch("otherarch").Runtime("otherruntime").Version("1.0"))
                    .And(AWishWith().Name("OtherName2").Group("OtherGroup2").Ext("OtherExt2").Arch("otherarch2").Runtime("otherruntime2").Version("1.2")));

            var deps = cache.FindDependenciesMatching(WishWith().Name("SomeName").Group("SomeGroup").Ext("SomeExt").Arch("SomeArch").Runtime("SomeRuntime"));
            Expect.That(deps)
                .Is(AList.WithOnly(ADepWith().Name("SomeName").Group("SomeGroup").Ext("SomeExt").Arch("somearch").Runtime("someruntime").Version("3.0.0")));

        }

        private static void AssertExists(DirectoryInfo baseDir,String relPath){
            relPath = relPath.Replace("/", "\\");
            var file = new FileInfo(Path.Combine(baseDir.FullName,relPath));
            Assert.IsTrue(file.Exists,"Expect " + relPath + " to exist but didn't");
        }
    }
}

