using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Matcher {
    internal class AlwaysTrueMatcher<T>:IMatcher<T> {
        public bool Match(T actual) {
            return true;
        }
    }
}
