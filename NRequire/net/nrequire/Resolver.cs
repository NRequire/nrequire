using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Collections;
namespace net.nrequire {
    public class Resolver {

        private static readonly IDictionary<String, IList<String>> DefaultRelatedByExt = new Dictionary<String, IList<String>>{
            { "dll", new[]{ "xml", "pdb" }},
            { "exe", new[]{ "xml", "pdb" }}
        };

        public IList<SpecificDependency> ResolveDependencies(Solution soln, Project proj) {
            ValidateReadyForMerge(soln.Dependencies);
            ValidateReadyForMerge(proj.Compile);
            ValidateReadyForMerge(proj.Transitive);

            var deps = MergeDeps(soln, proj).ToList();
            ValidateAllSet(deps);

            //now the fun begins. Resolve transitive deps, find closest versions

            return deps.Select((d)=>ResolveToSpecific(d)).ToList();
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

        private DepsList MergeDeps(Solution soln, Project proj) {
            var versions = new DepsList(soln.Dependencies);
            
            var deps = new DepsList();
            deps.MergeInWithLookup(Scopes.Compile,proj.Compile,versions,0);
            deps.MergeInWithLookup(Scopes.Transitive, proj.Transitive, versions, 1);
            deps.MergeInWithLookup(Scopes.Provided, proj.Provided, versions, 1);

            foreach (var dep in proj.Transitive) {
                deps.MergeInWithLookup(Scopes.Transitive, dep.Dependencies, versions, 2);
            }
            foreach (var dep in proj.Compile) {
                deps.MergeInWithLookup(Scopes.Transitive,dep.Dependencies, versions, 2);
            }
            foreach (var dep in proj.Provided) {
                deps.MergeInWithLookup(Scopes.Transitive, dep.Dependencies, versions, 2);
            }
            foreach (var dep in deps) {
                deps.MergeInWithLookup(Scopes.Transitive, dep.Dependencies, versions, 2);
            } 
            return deps;
        }

        private class DepsList :IEnumerable<Dependency>{
            private readonly IDictionary<string, Dependency> m_deps;

            internal DepsList() {
                m_deps = new Dictionary<string, Dependency>();
            }
            
            internal DepsList(IList<Dependency> deps) {
                m_deps = deps.ToDictionary(d => d.Signature());
            }

            internal IList<Dependency> ToList() {
                return new List<Dependency>(m_deps.Values);
            }

            internal void MergeInWithLookup(Scopes scope, IList<Dependency> additional, DepsList lookup, int level) {
                foreach (var dep in additional) {
                    
                    var key = dep.Signature();
                    if (!m_deps.ContainsKey(key)) {
                        Dependency depToAdd;
                        if (lookup.ContainsKey(key)) {
                            var solnDep = lookup.GetByKey(key);
                            depToAdd = dep.FillInBlanksFrom(solnDep);
                        } else {
                            depToAdd = dep.Clone();
                        }
                        
                        depToAdd.Depth = level;
                        depToAdd.Scope = scope;

                        depToAdd.ValidateRequiredSet();
                        m_deps[key] = depToAdd;
                    }
                }
            }

            private bool ContainsKey(String key) {
                return m_deps.ContainsKey(key);
            }

            private Dependency GetByKey(string key) {
                return m_deps[key];
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            public IEnumerator<Dependency> GetEnumerator() {
                return new List<Dependency>(m_deps.Values).GetEnumerator();
            }
        }

        private static SpecificDependency ResolveToSpecific(Dependency dep) {
            //TODO:apply version selection, ranges etc
            var specific = new SpecificDependency {
                Arch = dep.Arch,
                CopyTo = dep.CopyTo,
                Ext = dep.Ext,
                Group = dep.Group,
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
