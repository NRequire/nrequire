using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {
    public class Solution {

        private static readonly DependencyWish DefaultDependencyValues = new DependencyWish { Arch = "any", Runtime = "any", Ext="dll", Scope = Scopes.Compile };
        //TODO:add Repos:[] {name:central,url:path/to/some/repo,layout:maven|nget|...}
        public IList<DependencyWish> Dependencies { get; set; }
        //only for transitive stuff, not per top level
        public IList<DependencyWish> Transitive { get; set; }

        public DependencyWish DependencyDefaults { get; set; }
        public String SolutionFormat { get;set; }

        public Solution() {
            Dependencies = new List<DependencyWish>();
            Transitive = new List<DependencyWish>();
            DependencyDefaults = DefaultDependencyValues.Clone();
        }

        public void AfterLoad() {
            if (SolutionFormat != "1") {
                throw new ArgumentException("This solution only supports format version 1. Instead got " + SolutionFormat);
            }
            //apply defaults
            DependencyDefaults = DependencyDefaults == null ? DefaultDependencyValues.Clone() : DependencyDefaults.FillInBlanksFrom(DefaultDependencyValues);
            Dependencies = DependencyWish.FillInBlanksFrom(Dependencies, DependencyDefaults);
            Transitive = DependencyWish.FillInBlanksFrom(Transitive, DependencyDefaults);
            //TODO: check no duplicated deps, need to pick a list


        }
    }
}
