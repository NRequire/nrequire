using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {
    public class Solution {

        private static readonly Dependency DefaultDependencyValues = new Dependency { Arch = "any", Runtime = "any", Ext="dll" };

        public IList<Dependency> Dependencies { get;set;}
        public Dependency DependencyDefaults { get; set; }

        public Solution() {
            Dependencies = new List<Dependency>();
            DependencyDefaults = DefaultDependencyValues.Clone();
        }

        public void ApplyDefaults() {
            DependencyDefaults = DependencyDefaults == null ? DefaultDependencyValues.Clone() : DependencyDefaults.MergeWithDefault(DefaultDependencyValues);
            Dependencies = Dependency.MergeWithDefault(Dependencies, DependencyDefaults);
        }
    }
}
