using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {
    public class Solution {

        public IList<Dependency> Dependencies { get;set;}

        public Solution() {
            Dependencies = new List<Dependency>();
        }

        public void ApplyDefaults() {
            Dependencies = Dependency.MergeWithDefault(Dependencies);
        }
    }
}
