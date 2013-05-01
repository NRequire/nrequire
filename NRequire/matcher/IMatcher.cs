using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Matcher {

    internal interface IMatcher<T> {
        bool Match(T obj);
    }

}
