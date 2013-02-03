﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace net.nrequire {
    public class Resolver {

        public IList<Dependency> ResolveProjectDeps(Solution soln, Project proj) {
            soln.ApplyDefaults();
            proj.ApplyDefaults();

            ValidateReadyForMerge(soln.Dependencies);
            ValidateReadyForMerge(proj.Dependencies);

            var deps = MergeDeps(soln, proj);
            ValidateAllSet(deps);

            return deps;
        }

        private void ValidateReadyForMerge(IEnumerable<Dependency> deps) {
            foreach (var d in deps) {
                d.ValidateMergeValuesSet();
            }
        }

        private void ValidateAllSet(IEnumerable<Dependency> deps) {
            foreach (var d in deps) {
                d.ValidateRequiredSet();
            }
        }

        private IList<Dependency> MergeDeps(Solution soln, Project proj) {
            if (soln.Dependencies.Count == 0) {
                return new List<Dependency>(proj.Dependencies);
            }

            var lookup = soln.Dependencies.ToDictionary(d => d.Signature());

            var merged = new Dictionary<string, Dependency>();
            foreach (var dep in proj.Dependencies) {
                var key = dep.Signature();
                if (lookup.ContainsKey(key)) {
                    var solnDep = lookup[key];
                    merged[key] = dep.Clone().MergeWithParent(solnDep);
                } else {
                    merged[key] = dep.Clone();
                }
            }
            var transitiveDeps = new Dictionary<string, Dependency>();
            foreach (var dep in merged.Values) {
                if (dep.Dependencies != null) {
                    foreach (var tdep in dep.Dependencies) {
                        var key = tdep.Signature();
                        if (!transitiveDeps.ContainsKey(key)) {//only add it if not already defined
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
            return deps;
        }

        internal IList<Dependency> ToSpecificDependencies() {
            return Dependencies;
        }
    }
}