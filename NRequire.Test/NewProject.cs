using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire {
    internal class NewProject : Project {
        internal static NewProject With() {
            return new NewProject();
        }

        internal NewProject CompileDependency(DependencyWish d) {
            base.Compile.Add(d.Clone());
            return this;
        }

        internal NewProject TransitiveDependency(DependencyWish d) {
            base.Transitive.Add(d.Clone());
            return this;
        }
    }
}
