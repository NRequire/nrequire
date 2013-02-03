using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {
    public class Project {
        private static readonly Dependency DefaultDependencyValues = new Dependency { Arch = "any", Runtime = "any" };

        public IList<Dependency> Dependencies { get;set;}
        public Dependency DependencyDefaults { get; set; }

        public Project() {
            Dependencies = new List<Dependency>();
            DependencyDefaults = DefaultDependencyValues.Clone();
        }

        public void ApplyDefaults() {
            DependencyDefaults = DependencyDefaults == null ? DefaultDependencyValues.Clone() : DependencyDefaults.FillInBlanksFrom(DefaultDependencyValues);
            Dependencies = Dependency.FillInBlanksFrom(Dependencies, DependencyDefaults);
        }
    }
}
