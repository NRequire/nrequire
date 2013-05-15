using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NRequire.Matcher;
using System.Collections.Generic;
using System.Collections;

namespace NRequire.Resolver {
    [TestFixture]
    public class DependencyWishSetTest : BaseDependencyTest {

        [Test]
        public void DependencyWishList_Resolved() {
            var cache = new InMemoryDependencyCache();
            cache.Add(NewNode("A", "1.0"));
            cache.Add(NewNode("A", "1.1"));
            cache.Add(NewNode("A", "1.3"));
            cache.Add(NewNode("A", "2.0"));

            var wish = Wish("A", "[1.1,2.0)");

            var set = new WishSet(Wish("A", "1.3"), cache);

            set.AddIfNotExists(Wish("A", "1.3"));
            Assert.IsTrue(set.IsFixed(), "expect resolved as only one matching dep");
            Assert.IsTrue(set.CanMatch(), "expect can match");
        }

        [Test]
        public void DependencyWishList_HasMultipleMatches() {
            var cache = new InMemoryDependencyCache();
            cache.Add(NewNode("A", "1.0"));
            cache.Add(NewNode("A", "1.1"));
            cache.Add(NewNode("A", "1.3"));
            cache.Add(NewNode("A", "2.0"));

            var wish = Wish("A", "[1.1,2.0)");

            var set = new WishSet(wish, cache);

            set.AddIfNotExists(Wish("A", "[1.0,2.0]"));
            Assert.IsFalse(set.IsFixed(), "expect not resolved as more than one matching dep");
            Assert.IsTrue(set.CanMatch(), "expect can match");

        }

        [Test]
        public void DependencyWishlist_HasNoMatches() {
            var cache = new InMemoryDependencyCache();
            cache.Add(NewNode("A", "1.0"));
            cache.Add(NewNode("A", "1.1"));
            cache.Add(NewNode("A", "1.3"));
            cache.Add(NewNode("A", "2.0"));

            var wish = Wish("A", "[1.1,2.0)");

            var set = new WishSet(wish, cache);

            set.AddIfNotExists(Wish("A", "(3.0]"));
            Assert.IsFalse(set.IsFixed(), "expect not resolved as no matching deps");
            Assert.IsFalse(set.CanMatch(), "expect no possible matches");

        }

    }
}
