using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {
    public class Project {

        public IList<Dependency> Dependencies { get;set;}
        public Dependency DependencyDefaults { get; set; }

        public Project() {
            Dependencies = new List<Dependency>();
            DependencyDefaults = Dependency.DefaultDependency();
        }

        public void ApplyDefaults() {
            DependencyDefaults = DependencyDefaults.MergeWithDefault(Dependency.DefaultDependency());
            Dependencies = Dependency.MergeWithDefault(Dependencies, DependencyDefaults);
        }
    }
}
