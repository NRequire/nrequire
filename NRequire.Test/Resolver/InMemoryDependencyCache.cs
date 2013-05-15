using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NRequire.Matcher;
using System.Collections.Generic;
using System.Collections;

namespace NRequire
{
	/// <summary>
    /// In memory cache to use for testing. 
    /// </summary>
    internal class InMemoryDependencyCache : IDependencyCache
    {
		
        private static readonly Logger Log = Logger.GetLogger(typeof(InMemoryDependencyCache));
        private Dictionary<String,SortedList<Version,Dependency>> m_deps = new Dictionary<string, SortedList<Version,Dependency>>();
        private Dictionary<String,DependencyNode> m_nodesByKeyAndVersion = new Dictionary<string, DependencyNode>();
        private static readonly SortedList<Version,Dependency> EmptyList = NewSortedList();

        public DependencyNode Add(DependencyNode node)
        {
            var key = KeyFor(node);
			
            if (m_deps.ContainsKey(key)) {
                m_deps[key].Add(node.Version, node);
            } else {
                var deps = NewSortedList();
                deps.Add(node.Version, node);
                m_deps.Add(key, deps);
            }
            m_nodesByKeyAndVersion.Add(VersionKeyFor(node), node);
            return node;
        }

        private static SortedList<Version,Dependency> NewSortedList()
        {
            return new SortedList<Version,Dependency>(InvertedComparer.Instance);
        }

        public bool ContainsDependency(Dependency d)
        {
            var deps = FindByKey(KeyFor(d));
            return deps.Count > 0;
        }

        public Resource GetResourceFor(Dependency d)
        {
            return null;
        }

        public IList<DependencyWish> FindWishesFor(Dependency d)
        {
            var key = VersionKeyFor(d);
            DependencyNode node;
            if (m_nodesByKeyAndVersion.TryGetValue(key, out node)) {
                return node.Wishes;
            }
            return new List<DependencyWish>();
        }

        public IList<Dependency> FindDependenciesMatching(DependencyWish wish)
        {
            var deps = FindByKey(KeyFor(wish));
            return deps.Values.Where(d=>wish.Version.Match(d.Version)).ToList();
        }

        public IList<Version> GetVersionsMatching(DependencyWish wish)
        {
            var deps = FindByKey(KeyFor(wish));
            return deps.Keys.Where(v=>wish.Version.Match(v)).ToList();
        }

        private SortedList<Version,Dependency> FindByKey(String key)
        {
            SortedList<Version,Dependency> deps;
            if (!m_deps.TryGetValue(key, out deps)) {
                return EmptyList;
            }
            return deps;
        }

        private String VersionKeyFor(Dependency d)
        {
            return KeyFor(d) + "-" + d.Version.ToString();
        }

        private String KeyFor(AbstractDependency d)
        {
            return KeyFor(d.Group, d.Name);
        }

        private String KeyFor(String group, String name)
        {
            return group + "-" + name;
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

