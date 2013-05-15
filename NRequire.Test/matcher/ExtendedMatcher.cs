using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Matcher {
    public abstract class ExtendedMatcher<T>:IExtendedMatcher<T> {

        public bool Match(T actual) {
            return Match(actual, NillMatchDiagnostics.Instance);
        }

        public abstract bool Match(T actual, IMatchDiagnostics diagnostics);
    }
}
