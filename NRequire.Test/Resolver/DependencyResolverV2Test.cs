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
	public class DependencyResolverV2Test
    {
        [Test]
        public void CacheRequires()
        {
            var cache = new TestDependencyCache();
            cache.Add(Node("A", "1.0").Requires("C", "[1.0,1.2)"));
            cache.Add(Node("A", "1.1").Requires("D", "[1.0,1.1]"));
            cache.Add(Node("B", "1.0").Requires("E", "[1.0,1.1]"));

            var actual = cache.FindWishesFor(Dep("A","1.0"));

            var expect = new List<DependencyWish>();
            expect.Add(Wish("C","[1.0,1.2)"));

            AssertAreEqual(expect, actual);
        }

        [Test]
        public void CacheFind()
        {
            var cache = new TestDependencyCache();
            cache.Add(Node("A", "1.0"));
            cache.Add(Node("A", "1.1"));
            cache.Add(Node("A", "1.3"));
            cache.Add(Node("A", "2.0"));

            var actual = cache.FindDependenciesMatching(Wish("A","[1.1,2.0)"));

            var expect = new List<Dependency>();
            expect.Add(Dep("A","1.3"));
            expect.Add(Dep("A","1.1"));

            AssertAreEqual(expect, actual);
        }

        [Test]
        public void DependencyWishList_Resolved()
        {
            var cache = new TestDependencyCache();
            cache.Add(Node("A", "1.0"));
            cache.Add(Node("A", "1.1"));
            cache.Add(Node("A", "1.3"));
            cache.Add(Node("A", "2.0"));

            var wish = Wish("A", "[1.1,2.0)");
      
            var list = new WishSet(Wish("A","1.3"), cache);

            list.AddIfNotExists(Wish("A","1.3"));
            Assert.IsTrue(list.IsFixed(), "expect resolved as only one matching dep");
            Assert.IsTrue(list.CanMatch(), "expect can match");
        }

        [Test]
        public void DependencyWishList_HasMultipleMatches()
        {
            var cache = new TestDependencyCache();
            cache.Add(Node("A", "1.0"));
            cache.Add(Node("A", "1.1"));
            cache.Add(Node("A", "1.3"));
            cache.Add(Node("A", "2.0"));
      
            var wish = Wish("A", "[1.1,2.0)");
      
            var list = new WishSet(wish, cache);
      
            list.AddIfNotExists(Wish("A","[1.0,2.0]"));
            Assert.IsFalse(list.IsFixed(), "expect not resolved as more than one matching dep");
            Assert.IsTrue(list.CanMatch(), "expect can match");

        }

        [Test]
        public void DependencyWishlist_HasNoMatches()
        {
            var cache = new TestDependencyCache();
            cache.Add(Node("A", "1.0"));
            cache.Add(Node("A", "1.1"));
            cache.Add(Node("A", "1.3"));
            cache.Add(Node("A", "2.0"));

            var wish = Wish("A", "[1.1,2.0)");
        
            var list = new WishSet(wish, cache);

            list.AddIfNotExists(Wish("A","(3.0]"));
            Assert.IsFalse(list.IsFixed(), "expect not resolved as no matching deps");
            Assert.IsFalse(list.CanMatch(), "expect no possible matches");
      
        }

        [Test]
	    public void ResolveSimple()
        {
            var cache = new TestDependencyCache();
            cache.Add(Node("A", "1.0").Requires("C", "[1.0,1.2)"));
            cache.Add(Node("B", "1.0").Requires("D", "[1.0,1.1]"));
            cache.Add(Node("C", "1.0"));
            cache.Add(Node("D", "1.0").Requires("E", "[1.0,1.2]"));
            cache.Add(Node("D", "1.1").Requires("E", "[1.1,1.2]"));
            cache.Add(Node("E", "1.0"));
            cache.Add(Node("F", "1.0"));

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
            var cache = new TestDependencyCache();
            cache.Add(Node("A", "1.0").Requires("C", "[1.0,1.2)"));
            cache.Add(Node("B", "1.0").Requires("D", "[1.0,1.1]"));
            cache.Add(Node("C", "1.0"));
            cache.Add(Node("C", "1.1").Requires("E", "[1.3,1.4]"));
            cache.Add(Node("D", "1.0").Requires("E", "[1.0,1.2]"));
            cache.Add(Node("D", "1.1").Requires("E", "[1.1,1.4]"));
            cache.Add(Node("D", "1.2").Requires("E", "[1.2,1.5]"));
            cache.Add(Node("E", "1.0"));
            cache.Add(Node("E", "1.1"));
            cache.Add(Node("E", "1.2"));
            cache.Add(Node("E", "1.3"));
            cache.Add(Node("E", "1.4"));
            cache.Add(Node("E", "1.5"));
            cache.Add(Node("F", "1.0"));
            
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
            var cache = new TestDependencyCache();
            cache.Add(Node("A", "1.0").Requires("B", "[1.0,1.2]").Requires("C", "[1.0,1.2]"));
            cache.Add(Node("B", "1.0").Requires("A", "[1.0,1.1]"));
            cache.Add(Node("C", "1.0").Requires("A", "[1.0,1.1]"));
            cache.Add(Node("C", "1.1").Requires("A", "[1.2]"));

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
            var cache = new TestDependencyCache();
            cache.Add(Node("A", "1.0").Requires("B", "[1.0,1.1]"));

            cache.Add(Node("B", "1.0").Requires("C", "[1.0]").Requires("E", "1.1"));
            cache.Add(Node("B", "1.1").Requires("C", "[1.1]").Requires("E", "1.0"));

            cache.Add(Node("C", "1.0").Requires("D", "[1.0]"));
            cache.Add(Node("C", "1.1").Requires("D", "[1.1]"));

            cache.Add(Node("D", "1.0").Requires("E", "1.0"));
            cache.Add(Node("D", "1.1").Requires("E", "1.1"));

            cache.Add(Node("E", "1.0"));
            cache.Add(Node("E", "1.1"));

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
        //TODO:classifiers
        //TODO:groups
        //TODO:no matches
        //TODO:multiple unstabl matches?
        //TODO:smallest version change picking?
        //TODO:picking strategy

        private Dependency Dep(String name, String version)
        {
            return new Dependency { Group="group", Name = name, Version = Version.Parse(version) };
        }

        private DependencyWish Wish(String name, String version)
        {
            return new DependencyWish { Group="group", Name = name, Version = VersionMatcher.Parse(version) };
        }

        private DependencyNode Node(String name, String version)
        {
            return new DependencyNode { Group="group", Name = name, Version = Version.Parse(version) };
        }

        private void AssertAreEqual(IList<DependencyWish> expect, IList<DependencyWish> actual)
        {
            if (expect.Count != actual.Count) {
                Fail("Counts don't match", expect, actual);
            }
            for (int i = 0; i < expect.Count; i++) {
                if (expect[i].Name != actual[i].Name) {
                    Fail("Names don't match at " + i, expect, actual);
                }
                if (!expect[i].Version.Equals(actual[i].Version)) {
                    Fail("Versions don't match at " + i, expect, actual);
                }
            }
        }

        private void AssertAreEqual(IList<Dependency> expect, IList<Dependency> actual)
        {
            if (expect.Count != actual.Count) {
                Fail("Counts don't match", expect, actual);
            }
            for (int i = 0; i < expect.Count; i++) {
                if (expect[i].Name != actual[i].Name) {
                    Fail("Names don't match at " + i, expect, actual);
                }
                if (!expect[i].Version.Equals(actual[i].Version)) {
                    Fail("Versions don't match at " + i, expect, actual);
                }
            }
        }

        private void Fail<T>(String msg, IEnumerable<T> expect, IEnumerable<T> actual)
        {
            Console.WriteLine("Failed because:" + msg);
      
            Console.WriteLine("expected:");
            Print(expect);
            Console.WriteLine("actual:");
            Print(actual);
            Assert.Fail(msg);
        }

        private void Print<T>(IEnumerable<T> deps)
        {
            int i = 0;
            foreach (var d in deps) {
                Console.WriteLine(i + " " + d);
                i++;
            }
        }
    }
}

