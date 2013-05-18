using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Matcher {
    public static class AnInstance {

        public static IExtendedMatcher<T> SameAs<T>(T expect) where T : class {
            if (expect == null) {
                return Null<T>();
            }
            return FunctionMatcher.For<T>(
                (actual) => Object.ReferenceEquals(expect,actual),
                () => "same instance as " + expect.ToString());
        }

        public static IExtendedMatcher<T> EqualTo<T>(T expect) where T:class{
            if (expect == null) {
                return Null<T>();
            }
            return FunctionMatcher.For<T>(
                (actual)=>expect.Equals(actual),
                ()=>"AnInstance equal to " + expect.ToString());
        }

        public static IExtendedMatcher<T> Null<T>() where T:class {
            return FunctionMatcher.For<T>(
                (actual) => actual == null,
                () => "AnInstance equal to null");
        }
    }
}
