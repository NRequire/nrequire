using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NRequire.Logging;
using NRequire.Model;
using Version = NRequire.Model.Version;

namespace NRequire
{
    /// <summary>
    /// In memory cache to use for testing. 
    /// </summary>
    internal class InMemoryDependencyCache : IDependencyCache
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(InMemoryDependencyCache));
        private static readonly SortedList<Version, IResolved> EmptyList = NewSortedList();

        private Dictionary<String, SortedList<Version, IResolved>> m_depsBySignatureKey = new Dictionary<string, SortedList<Version, IResolved>>();
        private Dictionary<String, Module> m_modulesByVersionKey = new Dictionary<string, Module>();

        public static InMemoryDependencyCache With(){
            return new InMemoryDependencyCache();
        }

        public InMemoryDependencyCache A(Module module)
        {
            Add(module);
            return this;
        }

        public InMemoryDependencyCache Add(Module module)
        {
            var key = KeyFor(module);
            
            if (m_depsBySignatureKey.ContainsKey(key)) {
                m_depsBySignatureKey[key].Add(module.Version, module);
            } else {
                var deps = NewSortedList();
                deps.Add(module.Version, module);
                m_depsBySignatureKey.Add(key, deps);
            }
            m_modulesByVersionKey.Add(VersionKeyFor(module), module);
            return this;
        }

        private static SortedList<Version, IResolved> NewSortedList()
        {
            return new SortedList<Version, IResolved>(InvertedComparer.Instance);
        }

        public bool ContainsDependency(IResolved d)
        {
            var deps = FindByKey(KeyFor(d));
            return deps.Count > 0;
        }

        public IList<Resource> GetResourcesFor(IResolved d)
        {
            throw new NotImplementedException();
        }

        public IList<Wish> FindWishesFor(IResolved d)
        {
            var key = VersionKeyFor(d);
            Module node;
            if (m_modulesByVersionKey.TryGetValue(key, out node)) {
                return node.RuntimeWishes.Concat(node.TransitiveWishes).Select(w=>w.Clone()).ToList();
            }
            return new List<Wish>();
        }

        public IList<Dependency> FindDependenciesMatching(Wish wish)
        {
            var deps = FindByKey(KeyFor(wish));
            return deps.Values
                    .Where(d => wish.Version.Match(d.Version))
                    .Select(r => new Dependency{
                        Name = r.Name,
                        Group = r.Group,
                        Ext = r.Ext,
                        Classifiers = r.Classifiers.Clone(),
                        Version = r.Version
                    })
                    .ToList();
        }

        public IList<Version> FindVersionsMatching(Wish wish)
        {
            var deps = FindByKey(KeyFor(wish));
            return deps.Keys.Where(v=>wish.Version.Match(v)).ToList();
        }

        private SortedList<Version,IResolved> FindByKey(String key)
        {
            SortedList<Version, IResolved> deps;
            if (!m_depsBySignatureKey.TryGetValue(key, out deps)) {
                return EmptyList;
            }
            return deps;
        }

        private String VersionKeyFor(IResolved d)
        {
            return KeyFor(d) + "-" + d.Version.ToString();
        }

        private String KeyFor(IResolvable d)
        {
            return d.GetKey();
        }

        private class InvertedComparer : IComparer<Version>
        {
            internal static InvertedComparer Instance = new InvertedComparer();

            public int Compare(Version x, Version y)
            {
                return y.CompareTo(x);
            }
        }
    }
}

