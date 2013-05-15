using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Matcher {
    public interface  IExtendedMatcher<in T> : IMatcher<T> {

        bool Match(T instance, IMatchDiagnostics diag);
    }
}
