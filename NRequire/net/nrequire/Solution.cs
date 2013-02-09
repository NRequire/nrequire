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
        public String SolutionFormat { get;set; }

        public Solution() {
            Dependencies = new List<Dependency>();
            DependencyDefaults = DefaultDependencyValues.Clone();
        }

        public void AfterLoad() {
            if (SolutionFormat != "1") {
                throw new ArgumentException("This solution only supports format version 1. Instead got " + SolutionFormat);
            }
            //apply defaults
            DependencyDefaults = DependencyDefaults == null ? DefaultDependencyValues.Clone() : DependencyDefaults.FillInBlanksFrom(DefaultDependencyValues);
            Dependencies = Dependency.FillInBlanksFrom(Dependencies, DependencyDefaults);
        
        }
    }
}
