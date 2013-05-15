using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NRequire.Matcher;
using NRequire.Resolver;

namespace NRequire 
{
    public abstract class BaseDependencyTest {

        protected Dependency Dep(String name, String version) {
            return new Dependency { Group = "group", Name = name, Version = Version.Parse(version) };
        }

        protected DependencyWish Wish(String name, String version) {
            return new DependencyWish { Group = "group", Name = name, Version = VersionMatcher.Parse(version) };
        }

        protected DependencyNode NewNode(String name, String version) {
            return new DependencyNode { Group = "group", Name = name, Version = Version.Parse(version) };
        }

        protected List<DependencyWish> ListWith(params DependencyWish[] wishes){
            return new List<DependencyWish>(wishes);
        }

        protected List<Dependency> ListWith(params Dependency[] deps){
            return new List<Dependency>(deps);
        }

        protected void AssertAreEqual(IList<DependencyWish> expect, IList<DependencyWish> actual) {
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

        protected void AssertAreEqual(IList<Dependency> expect, IList<Dependency> actual) {
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

        protected void Fail<T>(String msg, IEnumerable<T> expect, IEnumerable<T> actual) {
            Console.WriteLine("Failed because:" + msg);

            Console.WriteLine("expected:");
            Print(expect);
            Console.WriteLine("actual:");
            Print(actual);
            Assert.Fail(msg);
        }

        private void Print<T>(IEnumerable<T> deps) {
            int i = 0;
            foreach (var d in deps) {
                Console.WriteLine(i + " " + d);
                i++;
            }
        }
    }
}
