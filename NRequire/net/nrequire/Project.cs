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

            var lookup = solnDeps.ToDictionary(d => d.Signature());
     
            var merged = new Dictionary<string,Dependency>();
            foreach (var dep in Dependencies) {
                var key = dep.Signature();
                if (lookup.ContainsKey(key)) {
                    var solnDep = lookup[key];
                    merged[key] = dep.Clone().MergeWithParent(solnDep);
                } else {
                    merged[key] = dep;
                }
            }
            var transitiveDeps =  new Dictionary<string,Dependency>();
            foreach (var dep in merged.Values) {
                if (dep.Dependencies != null) {
                    foreach (var tdep in dep.Dependencies) {
                        var key = tdep.Signature();
                        if(!transitiveDeps.ContainsKey(key)){//only add it if not already defined
                            if (lookup.ContainsKey(key)) {//try to merge with solutions version
                                var solnTDep = lookup[key];
                                transitiveDeps[key] = tdep.Clone().MergeWithParent(solnTDep);
                            } else {
                                transitiveDeps[key] = tdep;
                            }
                        }
                    }
                }
            }
            //now resolve transitive deps
            var deps = new List<Dependency>(merged.Values);
            deps.AddRange(transitiveDeps.Values);
            Dependencies = deps;
        }

        internal IList<Dependency> GetResolvedDependencies() {
            return Dependencies;
        }
    }
}
