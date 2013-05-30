using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Matcher {
    public static class ABool {
        public static IExtendedMatcher<bool?> True() {
            return EqualTo(true);
        }

        public static IExtendedMatcher<bool?> False() {
            return EqualTo(false);
        }

        public static IExtendedMatcher<bool?> EqualTo(bool expect) {
            return FunctionMatcher.For<bool?>
                    (actual => expect == actual,
                    () => String.Format("A bool equal to '{0}'", expect));
        }
    }
}
