using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {
    public class Solution {

        private static readonly Dependency DefaultDependencyValues = new Dependency { Arch = "any", Runtime = "any", Ext="dll", Scope = Scopes.Compile };
        //TODO:add Repos:[] {name:central,url:path/to/some/repo,layout:maven|nget|...}
        public IList<Dependency> Dependencies { get;set; }
        public Dependency DependencyDefaults { get; set; }

        public Solution() {
            Dependencies = new List<Dependency>();
            DependencyDefaults = DefaultDependencyValues.Clone();
        }

        public void ApplyDefaults() {
            DependencyDefaults = DependencyDefaults == null ? DefaultDependencyValues.Clone() : DependencyDefaults.FillInBlanksFrom(DefaultDependencyValues);
            Dependencies = Dependency.FillInBlanksFrom(Dependencies, DependencyDefaults);
        }
    }
}
