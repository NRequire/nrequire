using NUnit.Framework;
using TestFirst.Net;
using TestFirst.Net.Matcher;

namespace NRequire.Resolver {
    [TestFixture]
    public class ProjectResolverTest  : BaseDependencyTest {
        
        [Test]
        public void ResolveProjectAndSolution() {
            var cache = NewDependencyCache.With()
                .A(ModuleFrom("group1:name1:1.0"))
                .A(ModuleFrom("group1:name1:2.0"))

                .A(ModuleFrom("group2:name2:1.0"))
                .A(ModuleFrom("group2:name2:2.0"))
                
                .A(ModuleFrom("group3:name3:1.0"))
                .A(ModuleFrom("group3:name3:2.0"))
                .A(ModuleFrom("group3:name3:3.0"))
                .A(ModuleFrom("group3:name3:4.0"))
                
                 //ignored transitives
                .A(ModuleFrom("group4:name4:4.0"))
                .A(ModuleFrom("group5:name5:5.0"))
                ;
            var resolver = ProjectDependencyResolver.WithCache(cache);

            var soln = NewSolution.With()
                .Wish("group1:name1:1.0")
                    .Wish("group2:name2:2.0")
                    //ignored
                    .Wish("group4:name4:4.0")
                ;
            var proj = NewProject.With()
                
                .RuntimeWish("group1:name1:[1.0,2.0]")
                .RuntimeWish("group2:name2:")
                .RuntimeWish("group3:name3:3.0")
                //ignored
                .TransitiveWish("group5:name5:5.0")
                ;
            soln.AfterLoad();
            proj.AfterLoad();
            var deps = resolver.MergeAndResolveDependencies(soln,proj);

            Expect.That(deps)
                .Is(AList.InOrder().WithOnly(
                    ADepFrom("group1:name1:1.0"),
                    ADepFrom("group2:name2:2.0"),
                    ADepFrom("group3:name3:3.0")));
        }


    }
}
