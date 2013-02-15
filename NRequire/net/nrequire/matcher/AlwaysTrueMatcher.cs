using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire.matcher {
    internal class AlwaysTrueMatcher<T>:IMatcher<T> {
        public bool Match(T actual) {
            return true;
        }
    }
}
