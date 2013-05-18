using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Matcher {
    public static class AString {

        public static IExtendedMatcher<String> StartingWith(String expect) {
            if (expect == null) {
            throw new ArgumentNullException("Start can't start with a null");
            }
            return FunctionMatcher.For<String>
                (actual=>actual.StartsWith(expect),
                 ()=>String.Format("AString starting with '{0}'",expect));
        }


        public static IExtendedMatcher<String> EqualTo(String expect) {
            if (expect == null) {
                return Null();
            }
            return FunctionMatcher.For<String>
                    (actual=>expect == actual,
                    ()=>String.Format("AString equal to '{0}'",expect));
        }

        public static IExtendedMatcher<String> EqualToIgnoreCase(String expect) {
            if (expect == null) {
                return Null();
            }
            expect = expect.ToLowerInvariant();
            return FunctionMatcher.For<String>
                    (actual => expect == actual.ToLowerInvariant(),
                    () => String.Format("AString equal to '{0}' (ignoring case)", expect));
        }

        public static IExtendedMatcher<String> Null() {
            return FunctionMatcher.For<String>
                    (actual => actual ==null,
                    () => String.Format("AString equal to null"));
        }
    }
}
