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

        public Project MergeWith(Solution soln) {
            //pick just the ones mentioned in the project
            var deps = MergeDeps(Dependencies, soln.Dependencies);
            return new Project() { Dependencies = deps};
        }

        private static IList<Dependency> MergeDeps(IList<Dependency> projDeps, IList<Dependency> solnDeps) {
            if (projDeps == null || projDeps.Count == 0) {
                return new List<Dependency>();
            }
            if (solnDeps == null || solnDeps.Count == 0) {
                return new List<Dependency>(projDeps);
            }

            var merged = projDeps.ToDictionary(d => d.Signature());
            foreach (var solnDep in solnDeps) {
                var key = solnDep.Signature();
                //only add if project lists it
                if (merged.ContainsKey(key)) {
                    var projDep = merged[key];
                    merged.Remove(key);
                    merged[key] = projDep.Clone().MergeWithParent(solnDep);
                } 
            }

            return new List<Dependency>(merged.Values);
        }
    }
}
