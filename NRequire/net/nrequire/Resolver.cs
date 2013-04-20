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

        private static readonly Logger Log = Logger.GetLogger(typeof(Resolver));

        public IDependencyCache DepsCache { get; private set; }

        private Resolver(IDependencyCache cache) {
            DepsCache = cache;
        }

        internal static Resolver WithCache(IDependencyCache cache) {
            return new Resolver(cache);
        }

        public IList<Dependency> ResolveDependencies(Solution soln, Project proj) {
            ValidateReadyForMerge(soln.Dependencies);
            ValidateReadyForMerge(proj.Compile);
            ValidateReadyForMerge(proj.Transitive);

            var deps = MergeDeps(soln, proj).ToList();
            ValidateAllSet(deps);

            //now the fun begins. Resolve transitive deps, find closest versions

            return deps.Select((d)=>Resolve(d)).ToList();
        }

        private void ValidateReadyForMerge(IEnumerable<DependencyWish> deps) {
            foreach (var d in deps) {
                d.ValidateMergeValuesSet();
            }
        }

        private void ValidateAllSet(IEnumerable<DependencyWish> deps) {
            foreach (var d in deps) {
                d.ValidateRequiredSet();
            }
        }

        private DepsList MergeDeps(Solution soln, Project proj) {
            var versions = new DepsList(soln.Dependencies.Concat(soln.Transitive));
            
            var deps = new DepsList();
            deps.MergeInWithLookup(Scopes.Compile,proj.Compile,versions,0);
            deps.MergeInWithLookup(Scopes.Transitive, proj.Transitive, versions, 1);
            deps.MergeInWithLookup(Scopes.Provided, proj.Provided, versions, 1);

            var transitivesToMerge = proj.Transitive.Concat(proj.Compile).Concat(proj.Provided);

            foreach (var dep in transitivesToMerge) {
                deps.MergeInWithLookup(Scopes.Transitive, dep.Transitive, versions, 2);
            }
            //TODO:recursive
            foreach (var dep in deps) {
                deps.MergeInWithLookup(Scopes.Transitive, dep.Transitive, versions, 2);
            } 
            return deps;
        }



        private Dependency Resolve(DependencyWish wish) {
            var versions = DepsCache.GetVersionsMatching(wish);
            if (versions.Count == 0) {
                throw new ResolutionException("Could not resolve dependency {0} as no matching versions were returned from cache", wish);
            }

            var sorted = new List<Version>(versions);
            sorted.Sort();
            sorted.Reverse();

            Log.DebugFormat("Found versions:{0}", String.Join(",", sorted));

            var version = sorted.FirstOrDefault((v) => wish.Version.Match(v));
            Log.DebugFormat("Matched version {0} for dep {1}", version, wish);

            //check cache got it right
            if (version == null) {
                throw new ResolutionException("Could not resolve dependency {0} as no matching versions could be found", wish);
            }

            //TODO:apply version selection, ranges etc
            var dep = new Dependency {
                Arch = wish.Arch,
                CopyTo = wish.CopyTo,
                Ext = wish.Ext,
                Group = wish.Group,
                Name = wish.Name,
                Runtime = wish.Runtime,
                Scope = wish.Scope.GetValueOrDefault(Scopes.Compile),
                Url = wish.Url,
                Version = version
            };
            dep.Related = GetClonesWithExtensions(dep,GetRelatedExtensionsFor(wish));
            return dep;
        }

        private static IList<String> GetRelatedExtensionsFor(DependencyWish d) {
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

        private static IList<Dependency> GetClonesWithExtensions(Dependency d, IList<String> extensions) {
            var clones = new List<Dependency>();
            if (extensions != null && extensions.Count > 0) {
                foreach (var ext in extensions) {
                    var clone = d.Clone();
                    clone.Ext = ext;
                    clones.Add(clone);
                }
            }
            return clones;
        }

        private class DepsList : IEnumerable<DependencyWish> {
            //TODO:add list per depth?
            private static readonly Logger Log = Logger.GetLogger(typeof(DepsList));

            private readonly IDictionary<string, DependencyWish> m_deps;

            internal DepsList() {
                m_deps = new Dictionary<string, DependencyWish>();
            }

            internal DepsList(IEnumerable<DependencyWish> deps) {
                m_deps = deps.ToDictionary(d => d.Signature());
            }

            internal IList<DependencyWish> ToList() {
                return new List<DependencyWish>(m_deps.Values);
            }

            internal void MergeInWithLookup(Scopes scope, IList<DependencyWish> additional, DepsList lookup, int level) {
                foreach (var wish in additional) {
                    var sig = wish.Signature();
                    Log.Trace("merging in wish " + sig);
                    if (!m_deps.ContainsKey(sig)) {
                        DependencyWish wishToAdd;
                        if (lookup.ContainsKey(sig)) {
                            var solnDep = lookup.GetByKey(sig);
                            wishToAdd = wish.FillInBlanksFrom(solnDep);
                        } else {
                            wishToAdd = wish.Clone();
                        }
                        
                        wishToAdd.Depth = level;
                        wishToAdd.Scope = scope;

                        wishToAdd.ValidateRequiredSet();
                        m_deps[sig] = wishToAdd;

                        if (Log.IsTraceEnabled()) {
                            Log.Trace("added wish " + wishToAdd);
                        }
                    }
                }
            }

            private bool ContainsKey(String key) {
                return m_deps.ContainsKey(key);
            }

            private DependencyWish GetByKey(string key) {
                return m_deps[key];
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            public IEnumerator<DependencyWish> GetEnumerator() {
                return new List<DependencyWish>(m_deps.Values).GetEnumerator();
            }
        }
    }
}
