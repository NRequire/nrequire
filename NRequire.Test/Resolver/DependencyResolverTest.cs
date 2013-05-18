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
    public class DependencyResolverTest : BaseDependencyTest
    {
        //TODO:classifiers
        //TODO:groups
        //TODO:no matches
        //TODO:multiple unstabl matches?
        //TODO:smallest version change picking?
        //TODO:picking strategy

        [Test]
        public void ResolveSingleDep()
        {
            //var cache = new InMemoryDependencyCache();
            var cache = NewDependencyCache.With()
                .A(ModuleWith("A", "1.0").RequiresWishWith("B", "[1.0]"))
                .A(ModuleWith("B", "1.0"));

            var actual = new WishResolver(cache).Resolve(ListWith(WishWith("A","1.0")));

            Expect.That(actual).Is(AList.InOrder().WithOnly(ADepWith("A","1.0"),ADepWith("B","1.0")));
        }

        [Test]
	    public void ResolveManyDeps()
        {
            var cache =InMemoryDependencyCache.With()
                .A(ModuleWith("A", "1.0").RequiresWishWith("C", "[1.0,1.2)"))
                .A(ModuleWith("B", "1.0").RequiresWishWith("D", "[1.0,1.1]"))
                .A(ModuleWith("C", "1.0"))
                .A(ModuleWith("D", "1.0").RequiresWishWith("E", "[1.0,1.2]"))
                .A(ModuleWith("D", "1.1").RequiresWishWith("E", "[1.1,1.2]"))
                .A(ModuleWith("E", "1.0"))
                .A(ModuleWith("F", "1.0"));

            var require = ListWith(WishWith("A","1.0"),WishWith("B","1.0"));

            var actual = new WishResolver(cache).Resolve(require);

            Expect
                .That(actual)
                .Is(AList.InOrder().WithOnly(
                        ADepWith("A","1.0"),
                        ADepWith("B","1.0"),
                        ADepWith("C","1.0"),
                        ADepWith("D","1.0"),
                        ADepWith("E","1.0")));

        }

        [Test]
        public void ResolveManyDepsRequiringLesserVersions()
        {
            var cache = InMemoryDependencyCache.With()
                .A(ModuleWith("A", "1.0").RequiresWishWith("C", "[1.0,1.2)"))
                .A(ModuleWith("B", "1.0").RequiresWishWith("D", "[1.0,1.1]"))
                .A(ModuleWith("C", "1.0"))
                .A(ModuleWith("C", "1.1").RequiresWishWith("E", "[1.3,1.4]"))
                .A(ModuleWith("D", "1.0").RequiresWishWith("E", "[1.0,1.2]"))
                .A(ModuleWith("D", "1.1").RequiresWishWith("E", "[1.1,1.4]"))
                .A(ModuleWith("D", "1.2").RequiresWishWith("E", "[1.2,1.5]"))
                .A(ModuleWith("E", "1.0"))
                .A(ModuleWith("E", "1.1"))
                .A(ModuleWith("E", "1.2"))
                .A(ModuleWith("E", "1.3"))
                .A(ModuleWith("E", "1.4"))
                .A(ModuleWith("E", "1.5"))
                .A(ModuleWith("F", "1.0"));
            
            var require = ListWith(WishWith("A","1.0"),WishWith("B","1.0"));

            var actual = new WishResolver(cache).Resolve(require);

            Expect
                .That(actual)
                .Is(AList.InOrder().WithOnly(
                    ADepWith("A","1.0"),
                    ADepWith("B","1.0"),
                    ADepWith("C","1.1"),
                    ADepWith("D","1.1"),
                    ADepWith("E","1.4")));
        }
        [Test]
        public void CircularDeps()
        {
            var cache = InMemoryDependencyCache.With()
                .A(ModuleWith("A", "1.0").RequiresWishWith("B", "[1.0,1.2]").RequiresWishWith("C", "[1.0,1.2]"))
                .A(ModuleWith("B", "1.0").RequiresWishWith("A", "[1.0,1.1]"))
                .A(ModuleWith("C", "1.0").RequiresWishWith("A", "[1.0,1.1]"))
                .A(ModuleWith("C", "1.1").RequiresWishWith("A", "[1.2]"));

            var require = ListWith(WishWith("A", "1.0"),WishWith("B", "1.0"));

            var actual = new WishResolver(cache).Resolve(require);

            Expect
                .That(actual)
                .Is(AList.InOrder().WithOnly(
                    ADepWith("A", "1.0"),
                    ADepWith("B", "1.0"),
                    ADepWith("C", "1.0")));

        }

        [Test]
        public void NoSolution()
        {
            var cache = InMemoryDependencyCache.With()
                .A(ModuleWith("A", "1.0").RequiresWishWith("B", "[1.0,1.1]"))

                .A(ModuleWith("B", "1.0").RequiresWishWith("C", "[1.0]").RequiresWishWith("E", "1.1"))
                .A(ModuleWith("B", "1.1").RequiresWishWith("C", "[1.1]").RequiresWishWith("E", "1.0"))

                .A(ModuleWith("C", "1.0").RequiresWishWith("D", "[1.0]"))
                .A(ModuleWith("C", "1.1").RequiresWishWith("D", "[1.1]"))

                .A(ModuleWith("D", "1.0").RequiresWishWith("E", "1.0"))
                .A(ModuleWith("D", "1.1").RequiresWishWith("E", "1.1"))

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
    }
}

