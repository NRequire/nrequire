using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRequire.Test;

namespace NRequire.Matcher {
    public class AWish : AnAbstractDependency<AWish,Wish> {
        public static AWish With() {
            return new AWish();
        }

        public AWish Version(String expect) {
            Version(AString.EqualTo(expect));
            return Self;
        }

        public AWish Version(IExtendedMatcher<String> matcher) {
            AddProperty<String>("Version", (actual) => { return actual.Version==null?null:actual.Version.ToString();}, matcher);
            return Self;
        }
    }
}
