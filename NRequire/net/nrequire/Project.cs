using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {
    public class Project {

        public IList<Dependency> Dependencies { get; set; }

        public Project() {
            Dependencies = new List<Dependency>();
        }

        public void ValidateDependenciesSet() {
            foreach (var dep in Dependencies) {
                dep.ValidateRequiredSet();
            }
        }

        public void ApplyDefaults() {
            Dependencies = Dependency.MergeWithDefault(Dependencies);
        }

        public void ApplySolution(Solution soln) {
            //pick just the ones mentioned in the project
            MergeWith(soln.Dependencies);
        }

        private void MergeWith(IList<Dependency> solnDeps) {
            if (solnDeps == null || solnDeps.Count == 0) {
                return;
            }

            var merged = Dependencies.ToDictionary(d => d.Signature());
            foreach (var solnDep in solnDeps) {
                var key = solnDep.Signature();
                //only add if project lists it
                if (merged.ContainsKey(key)) {
                    var projDep = merged[key];
                    merged.Remove(key);
                    merged[key] = projDep.Clone().MergeWithParent(solnDep);
                } 
            }

            Dependencies = new List<Dependency>(merged.Values);
        }
    }
}
