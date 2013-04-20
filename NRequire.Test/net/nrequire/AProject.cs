using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {
    internal class AProject : Project {
        internal static AProject With() {
            return new AProject();
        }

        internal AProject CompileDependency(DependencyWish d) {
            base.Compile.Add(d.Clone());
            return this;
        }

        internal AProject TransitiveDependency(DependencyWish d) {
            base.Transitive.Add(d.Clone());
            return this;
        }
    }
}
