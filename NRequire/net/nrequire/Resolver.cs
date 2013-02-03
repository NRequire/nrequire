using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace net.nrequire {
    public class Resolver {

        private static readonly IDictionary<String, IList<String>> DefaultRelatedByExt = new Dictionary<String, IList<String>>{
            { "dll", new[]{ "xml", "pdb" }},
            { "exe", new[]{ "xml", "pdb" }}
        };

        public IList<SpecificDependency> ResolveDependencies(Solution soln, Project proj) {
            ValidateReadyForMerge(soln.Dependencies);
            ValidateReadyForMerge(proj.Dependencies);

            var deps = MergeDeps(soln, proj);
            ValidateAllSet(deps);

            //now the fun begins. Resolve transitive deps, find closest versions

            return deps.Select((d)=>Resolve(d)).ToList();
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
            var lookup = soln.Dependencies.ToDictionary(d => d.Signature());

            var merged = new Dictionary<string, Dependency>();
            foreach (var dep in proj.Dependencies) {
                var key = dep.Signature();
                if (lookup.ContainsKey(key)) {
                    var solnDep = lookup[key];
                    merged[key] = dep.FillInBlanksFrom(solnDep);
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
                                transitiveDeps[key] = tdep.FillInBlanksFrom(solnTDep);
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

        private static SpecificDependency Resolve(Dependency dep) {
            //TODO:apply version selection, ranges etc
            var specific = new SpecificDependency {
                Arch = dep.Arch,
                ArtifactId = dep.ArtifactId,
                CopyTo = dep.CopyTo,
                Ext = dep.Ext,
                GroupId = dep.GroupId,
                Name = dep.Name,
                Runtime = dep.Runtime,
                Scope = dep.Scope.GetValueOrDefault(Scopes.Compile),
                Url = dep.Url,
                Version = Version.Parse(dep.Version)
            };
            specific.Related = GetClonesWithExtensions(specific,GetRelatedExtensionsFor(dep));
            return specific;
        }

        private static IList<String> GetRelatedExtensionsFor(Dependency d) {
            if (d.HasRelatedDependencies()) {
                return d.Related;
            } else {
                IList<String> related;
                if (DefaultRelatedByExt.TryGetValue(d.Ext, out related)) {
                    return related;
                }
            }
            return null;
        }

        private static IList<SpecificDependency> GetClonesWithExtensions(SpecificDependency d, IList<String> extensions) {
            var clones = new List<SpecificDependency>();
            if (extensions != null && extensions.Count > 0) {
                foreach (var ext in extensions) {
                    var clone = d.Clone();
                    clone.Ext = ext;
                    clones.Add(clone);
                }
            }
            return clones;
        }
    }
}
