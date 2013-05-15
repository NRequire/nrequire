using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire {
    internal class NewSolution : Solution {

        internal static NewSolution With() {
            return new NewSolution();
        }

        internal NewSolution Dependency(DependencyWish d) {
            base.Dependencies.Add(d.Clone());
            return this;
        }

        new internal NewSolution Transitive(DependencyWish d) {
            base.Transitive.Add(d.Clone());
            return this;
        }

    }
}
