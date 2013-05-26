using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NRequire.Matcher;
using System.Collections.Generic;
using System.Collections;

namespace NRequire.Resolver
{
    [TestFixture]
    public class WishResolverTest : BaseDependencyTest {
        //TODO:classifiers
        //TODO:groups
        //TODO:no matches
        //TODO:multiple unstabl matches?
        //TODO:smallest version change picking?
        //TODO:picking strategy

        [Test]
        public void ResolveSingleDep() {
            //var cache = new InMemoryDependencyCache();
            var cache = NewDependencyCache.With()
                .A(ModuleWith("A", "1.0").RuntimeWishWith("B", "[1.0]"))
                .A(ModuleWith("B", "1.0"));

            var actual = new WishResolver(cache).Resolve(ListWith(WishWith("A", "1.0")));

            Expect.That(actual).Is(AList.InOrder().WithOnly(ADepWith("A", "1.0"), ADepWith("B", "1.0")));
        }

        [Test]
        public void ResolveManyDeps() {
            var cache = InMemoryDependencyCache.With()
                .A(ModuleWith("A", "1.0").RuntimeWishWith("C", "[1.0,1.2)"))
                .A(ModuleWith("B", "1.0").RuntimeWishWith("D", "[1.0,1.1]"))
                .A(ModuleWith("C", "1.0"))
                .A(ModuleWith("D", "1.0").RuntimeWishWith("E", "[1.0,1.2]"))
                .A(ModuleWith("D", "1.1").RuntimeWishWith("E", "[1.1,1.2]"))
                .A(ModuleWith("E", "1.0"))
                .A(ModuleWith("F", "1.0"));

            var require = ListWith(WishWith("A", "1.0"), WishWith("B", "1.0"));

            var actual = new WishResolver(cache).Resolve(require);

            Expect
                .That(actual)
                .Is(AList.InOrder().WithOnly(
                        ADepWith("A", "1.0"),
                        ADepWith("B", "1.0"),
                        ADepWith("C", "1.0"),
                        ADepWith("D", "1.0"),
                        ADepWith("E", "1.0")));

        }

        [Test]
        public void ResolveManyDepsRequiringLesserVersions() {
            var cache = InMemoryDependencyCache.With()
                .A(ModuleWith("A", "1.0").RuntimeWishWith("C", "[1.0,1.2)"))
                .A(ModuleWith("B", "1.0").RuntimeWishWith("D", "[1.0,1.1]"))
                .A(ModuleWith("C", "1.0"))
                .A(ModuleWith("C", "1.1").RuntimeWishWith("E", "[1.3,1.4]"))
                .A(ModuleWith("D", "1.0").RuntimeWishWith("E", "[1.0,1.2]"))
                .A(ModuleWith("D", "1.1").RuntimeWishWith("E", "[1.1,1.4]"))
                .A(ModuleWith("D", "1.2").RuntimeWishWith("E", "[1.2,1.5]"))
                .A(ModuleWith("E", "1.0"))
                .A(ModuleWith("E", "1.1"))
                .A(ModuleWith("E", "1.2"))
                .A(ModuleWith("E", "1.3"))
                .A(ModuleWith("E", "1.4"))
                .A(ModuleWith("E", "1.5"))
                .A(ModuleWith("F", "1.0"));

            var require = ListWith(WishWith("A", "1.0"), WishWith("B", "1.0"));

            var actual = new WishResolver(cache).Resolve(require);

            Expect
                .That(actual)
                .Is(AList.InOrder().WithOnly(
                    ADepWith("A", "1.0"),
                    ADepWith("B", "1.0"),
                    ADepWith("C", "1.1"),
                    ADepWith("D", "1.1"),
                    ADepWith("E", "1.4")));
        }
        [Test]
        public void CircularDeps() {
            var cache = InMemoryDependencyCache.With()
                .A(ModuleWith("A", "1.0").RuntimeWishWith("B", "[1.0,1.2]").RuntimeWishWith("C", "[1.0,1.2]"))
                .A(ModuleWith("B", "1.0").RuntimeWishWith("A", "[1.0,1.1]"))
                .A(ModuleWith("C", "1.0").RuntimeWishWith("A", "[1.0,1.1]"))
                .A(ModuleWith("C", "1.1").RuntimeWishWith("A", "[1.2]"));

            var require = ListWith(WishWith("A", "1.0"), WishWith("B", "1.0"));

            var actual = new WishResolver(cache).Resolve(require);

            Expect
                .That(actual)
                .Is(AList.InOrder().WithOnly(
                    ADepWith("A", "1.0"),
                    ADepWith("B", "1.0"),
                    ADepWith("C", "1.0")));

        }

        [Test]
        public void NoSolution() {
            var cache = InMemoryDependencyCache.With()
                .A(ModuleWith("A", "1.0").RuntimeWishWith("B", "[1.0,1.1]"))

                .A(ModuleWith("B", "1.0").RuntimeWishWith("C", "[1.0]").RuntimeWishWith("E", "1.1"))
                .A(ModuleWith("B", "1.1").RuntimeWishWith("C", "[1.1]").RuntimeWishWith("E", "1.0"))

                .A(ModuleWith("C", "1.0").RuntimeWishWith("D", "[1.0]"))
                .A(ModuleWith("C", "1.1").RuntimeWishWith("D", "[1.1]"))

                .A(ModuleWith("D", "1.0").RuntimeWishWith("E", "1.0"))
                .A(ModuleWith("D", "1.1").RuntimeWishWith("E", "1.1"))

                .A(ModuleWith("E", "1.0"))
                .A(ModuleWith("E", "1.1"));

            var require = ListWith(WishWith("A", "1.0"));

            ResolverException thrown = null;
            try {
                new WishResolver(cache).Resolve(require);
            } catch (ResolverException e) {
                thrown = e;
            }

            Assert.IsNotNull(thrown);
            if (!thrown.Message.Contains(ResolverException.NoSolutions)) {
                System.Console.WriteLine(thrown);
                Assert.Fail("unexpected error message, got : " + thrown.Message);
            }
        }

        [Test]
        public void DepsReturnedShouldBeScopeRuntime() {
            var cache = CacheWith()
                .A(ModuleFrom("group:mydep:1.0")
                    .RuntimeWishFrom("group:myruntime:1.0"))
                .A(ModuleFrom("group:myruntime:1.0"));

            var resolved = WishResolver
                .WithCache(cache)
                .Resolve(ListWith(WishFrom("group:mydep:[1.0,2.0]:::runtime")));

            Expect
               .That(resolved)
               .Is(AList.InOrder()
                    .With(ADependency.From("group:mydep:1.0").Scope(Scopes.Runtime))
                    .And(ADependency.From("group:myruntime:1.0").Scope(Scopes.Runtime)));
        }

        [Test]
        public void TransitivesOnlyNotResolved() {
            var cache = CacheWith()
                .A(ModuleFrom("group:mydep:1.0")
                    .RuntimeWishFrom("group:myruntime:1.0")//should be included
                    .TransitiveWishFrom("group:mytransitive:1.0"))//should be ignored
                .A(ModuleFrom("group:myruntime:1.0")
                    .TransitiveWishFrom("group:mytransitive2:1.0"))
                .A(ModuleFrom("group:mytransitive:1.0"))//not used, ensure no filtered cose it don't exist
                .A(ModuleFrom("group:mytransitive2:1.0"));//not used, ensure no filtered cose it don't exist

            var resolved = WishResolver
                .WithCache(cache)
                .Resolve(ListWith(WishFrom("group:mydep:[1.0,2.0]:::runtime")));

            Expect
               .That(resolved)
               .Is(AList.InOrder()
                    .With(ADependency.From("group:mydep:1.0"))
                    .And(ADependency.From("group:myruntime:1.0")));
        }
        [Test]
        public void TransitiveRequiredResolvedSimple() {
            var cache = CacheWith()
                .A(ModuleFrom("group:mydep:1.0")
                    .RuntimeWishFrom("group:myruntime:1.0")
                    .TransitiveWishFrom("group:mytransitive:1.0"))//not resolved by default
                .A(ModuleFrom("group:myruntime:1.0")//include a dep which force transitive above to be resolved
                    .RuntimeWishFrom("group:mytransitive:1.0"))
                .A(ModuleFrom("group:mytransitive:1.0"));//ignored as not matched

            var resolved = WishResolver
                .WithCache(cache)
                .Resolve(ListWith(WishFrom("group:mydep:[1.0,2.0]:::runtime")));

            Expect
               .That(resolved)
               .Is(AList.InOrder()
                    .With(ADependency.From("group:mydep:1.0"))
                    .And(ADependency.From("group:myruntime:1.0"))
                    .And(ADependency.From("group:mytransitive:1.0")));
        }

        [Test]
        public void TransitiveRequiredResolved() {
            var cache = CacheWith()
                .A(ModuleFrom("group:mydep:1.0")
                    .RuntimeWishFrom("group:myruntime:1.0")
                    .TransitiveWishFrom("group:mytransitive:1.0"))//not resolved by default
                .A(ModuleFrom("group:myruntime:1.0")//include a dep which force transitive above to be resolved
                    .RuntimeWishFrom("group:myruntime2:1.0"))
                .A(ModuleFrom("group:myruntime2:1.0")
                    .RuntimeWishFrom("group:mytransitive:[1.0]"))//force transitive above to be resolved
                .A(ModuleFrom("group:mytransitive:1.0"));//ignored as not matched

            var resolved = WishResolver
                .WithCache(cache)
                .Resolve(ListWith(WishFrom("group:mydep:[1.0,2.0]:::runtime")));

            Expect
               .That(resolved)
               .Is(AList.InOrder()
                    .With(ADependency.From("group:mydep:1.0"))
                    .And(ADependency.From("group:myruntime:1.0"))
                    .And(ADependency.From("group:myruntime2:1.0"))
                    .And(ADependency.From("group:mytransitive:1.0")));
        }

        
    }
}

