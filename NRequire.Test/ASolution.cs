using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire {
    internal class ASolution : Solution {

        internal static ASolution With() {
            return new ASolution();
        }

        internal ASolution Dependency(DependencyWish d) {
            base.Dependencies.Add(d.Clone());
            return this;
        }

        new internal ASolution Transitive(DependencyWish d) {
            base.Transitive.Add(d.Clone());
            return this;
        }

    }
}
