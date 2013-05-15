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
    public class DependencyResolverV2Test : BaseDependencyTest
    {
        //TODO:classifiers
        //TODO:groups
        //TODO:no matches
        //TODO:multiple unstabl matches?
        //TODO:smallest version change picking?
        //TODO:picking strategy

        [Test]
	    public void ResolveSimple()
        {
            var cache = new InMemoryDependencyCache();
            cache.Add(NewNode("A", "1.0").Wish("C", "[1.0,1.2)"));
            cache.Add(NewNode("B", "1.0").Wish("D", "[1.0,1.1]"));
            cache.Add(NewNode("C", "1.0"));
            cache.Add(NewNode("D", "1.0").Wish("E", "[1.0,1.2]"));
            cache.Add(NewNode("D", "1.1").Wish("E", "[1.1,1.2]"));
            cache.Add(NewNode("E", "1.0"));
            cache.Add(NewNode("F", "1.0"));

            var require = new List<DependencyWish>();
            require.Add(Wish("A","1.0"));
            require.Add(Wish("B","1.0"));

            var expect = new List<Dependency>();
            expect.Add(Dep("A","1.0"));
            expect.Add(Dep("B","1.0"));
            expect.Add(Dep("C","1.0"));
            expect.Add(Dep("D","1.0"));
            expect.Add(Dep("E","1.0"));

            var actual = new DependencyResolverV2(cache).Resolve(require);

            AssertAreEqual(expect, actual);
  
        }

        [Test]
        public void ResolveSimple2()
        {
            var cache = new InMemoryDependencyCache();
            cache.Add(NewNode("A", "1.0").Wish("C", "[1.0,1.2)"));
            cache.Add(NewNode("B", "1.0").Wish("D", "[1.0,1.1]"));
            cache.Add(NewNode("C", "1.0"));
            cache.Add(NewNode("C", "1.1").Wish("E", "[1.3,1.4]"));
            cache.Add(NewNode("D", "1.0").Wish("E", "[1.0,1.2]"));
            cache.Add(NewNode("D", "1.1").Wish("E", "[1.1,1.4]"));
            cache.Add(NewNode("D", "1.2").Wish("E", "[1.2,1.5]"));
            cache.Add(NewNode("E", "1.0"));
            cache.Add(NewNode("E", "1.1"));
            cache.Add(NewNode("E", "1.2"));
            cache.Add(NewNode("E", "1.3"));
            cache.Add(NewNode("E", "1.4"));
            cache.Add(NewNode("E", "1.5"));
            cache.Add(NewNode("F", "1.0"));
            
            var require = new List<DependencyWish>();
            require.Add(Wish("A","1.0"));
            require.Add(Wish("B","1.0"));
            
            var expect = new List<Dependency>();
            expect.Add(Dep("A","1.0"));
            expect.Add(Dep("B","1.0"));
            expect.Add(Dep("C","1.1"));
            expect.Add(Dep("D","1.1"));
            expect.Add(Dep("E","1.4"));
            
            var actual = new DependencyResolverV2(cache).Resolve(require);
            
            AssertAreEqual(expect, actual);
            
        }

        [Test]
        public void CircularDeps()
        {
            var cache = new InMemoryDependencyCache();
            cache.Add(NewNode("A", "1.0").Wish("B", "[1.0,1.2]").Wish("C", "[1.0,1.2]"));
            cache.Add(NewNode("B", "1.0").Wish("A", "[1.0,1.1]"));
            cache.Add(NewNode("C", "1.0").Wish("A", "[1.0,1.1]"));
            cache.Add(NewNode("C", "1.1").Wish("A", "[1.2]"));

            var require = new List<DependencyWish>();
            require.Add(Wish("A", "1.0"));
            require.Add(Wish("B", "1.0"));

            var expect = new List<Dependency>();
            expect.Add(Dep("A", "1.0"));
            expect.Add(Dep("B", "1.0"));
            expect.Add(Dep("C", "1.0"));

            var actual = new DependencyResolverV2(cache).Resolve(require);

            AssertAreEqual(expect, actual);

        }

        [Test]
        public void NoSolution()
        {
            var cache = new InMemoryDependencyCache();
            cache.Add(NewNode("A", "1.0").Wish("B", "[1.0,1.1]"));

            cache.Add(NewNode("B", "1.0").Wish("C", "[1.0]").Wish("E", "1.1"));
            cache.Add(NewNode("B", "1.1").Wish("C", "[1.1]").Wish("E", "1.0"));

            cache.Add(NewNode("C", "1.0").Wish("D", "[1.0]"));
            cache.Add(NewNode("C", "1.1").Wish("D", "[1.1]"));

            cache.Add(NewNode("D", "1.0").Wish("E", "1.0"));
            cache.Add(NewNode("D", "1.1").Wish("E", "1.1"));

            cache.Add(NewNode("E", "1.0"));
            cache.Add(NewNode("E", "1.1"));

            var require = new List<DependencyWish>();
            require.Add(Wish("A", "1.0"));

            ResolverException thrown = null;
            try {
                new DependencyResolverV2(cache).Resolve(require);
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

