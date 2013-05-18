using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Matcher {
    public static class AnInt {
        public static IExtendedMatcher<int?> EqualTo(int expect) {
            return FunctionMatcher.For<int?>
                    (actual => expect == actual,
                    () => String.Format("AnInt equal to '{0}'", expect));
        }
    }
}
