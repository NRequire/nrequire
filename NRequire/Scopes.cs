using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire {
    public enum Scopes {
        Transitive = 0,
        Provided = 1,
        //Compile = 2,
        Runtime = 2
    }
}
