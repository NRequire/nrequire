using System;
using System.Linq;
using System.Linq.Expressions;
using NRequire.Matcher;
using System.Collections.Generic;
using System.Collections;

namespace NRequire
{
	//holds a collection of constraints for a given dependency
    internal class WishSet : IMatcher<Version>
    {

        private static readonly Logger Log = Logger.GetLogger(typeof(WishSet));
        //all the additional wishes added to provide additional filtering of the parent provided lists of dependencies
        private readonly List<DependencyWish> m_wishes = new List<DependencyWish>();
        //to allow faster checking if wishes have already been added
        private readonly List<String> m_allVersionStrings;
        //the dependencies which come for the parent list/cache on top of which we apply our own additional filters
        private readonly IList<Dependency> m_parentDependencies;
        //the dependencies which matched once the wishes have been applied. Cache to prevent repeated lookups 
        private IList<Dependency> m_cachedFilteredDeps;
        private IEnumerator<Dependency> m_nextDepsPicker;
        //TODO:move out of here to allow different strategies?
        //first wish added, used to validate all further added wishes
        private readonly DependencyWish m_prototypeWish;

        internal WishSet(DependencyWish wish, IDependencyCache cache)
        {
            m_parentDependencies = cache.FindDependenciesMatching(wish);
            m_prototypeWish = wish;
            m_allVersionStrings = new List<String>();
        }

        internal WishSet(DependencyWish wish, WishSet parentWishList)
        {
            m_parentDependencies = parentWishList.FindMatches();
            m_prototypeWish = wish;
            m_allVersionStrings = new List<string>(parentWishList.m_allVersionStrings);
        }

        public DependencyWish GetFirstWish()
        {
            return m_prototypeWish;
        }

        public bool Match(Version v)
        {
            return m_wishes.All(w=>w.Version.Match(v));
        }
		/// <summary>
		/// Check if there is already a similair wish in the current list
		/// </summary>
		/// <param name="wish">Wish.</param>
        internal bool ContainsVersion(VersionMatcher version)
        {
            return m_allVersionStrings.Contains(version.ToString());
        }
		/// <summary>
		/// Adds the wish if not already a similair wish
		/// </summary>
		/// <param name="wish">Wish.</param>
        /// <returns>true if added, false if there was already a wish matching the same requirements</returns>
        internal bool AddIfNotExists(DependencyWish wish)
        {
            CheckNotPicking();
            var versionString = wish.Version.ToString();
            if (!m_allVersionStrings.Contains(versionString)) {
                m_allVersionStrings.Add(versionString);
                m_wishes.Add(wish);
                m_cachedFilteredDeps = null;//reset cache to recalculate
                return true;
            }
            return false;
        }

        private void CheckNotPicking()
        {
            if (m_nextDepsPicker != null) {
                throw new InvalidOperationException("Already resolving and picking dependencies");
            }
        }
		/// <summary>
		/// If there are still deps which can satisfy the criteria
		/// </summary>
		/// <returns><c>true</c> if this instance can match; otherwise, <c>false</c>.</returns>
        public bool CanMatch()
        {
            return FindMatches().Count() > 0;
        }
		/// <summary>
		/// If there is only a single dependency which can match all the criteria
		/// </summary>
		/// <returns><c>true</c> if this instance is resolved; otherwise, <c>false</c>.</returns>
        public bool IsFixed()
        {
            return FindMatches().Count() == 1;
        }

        public Dependency PickNextFixedDep()
        {
            if (m_nextDepsPicker == null) {
                m_nextDepsPicker = FindMatches().GetEnumerator();
            }
            if (m_nextDepsPicker.MoveNext()) {
                if (Log.IsTraceEnabled()) {
                    Log.Trace("picked from versions : " + String.Join(",", FindMatches().Select(d => d.Version)));
                }
                return m_nextDepsPicker.Current;
            }
            return null;
        }
        /// <summary>
        /// Gets matching dependencies or empty if no matches
        /// </summary>
        public IList<Dependency> FindMatches()
        {
            if (m_cachedFilteredDeps == null) {
                //grab the parent filtered deps and apply our own filters/criteria
                var filtered = new List<Dependency>(m_parentDependencies);
                filtered.RemoveAll(d=>!m_wishes.All(w=>w.Version.Match(d.Version)));
                m_cachedFilteredDeps = filtered;
            }
            return m_cachedFilteredDeps;
        }

        public override string ToString()
        {
            return String.Format("DependencyWishList@{0}<key:{1},wishes:{2}>", base.GetHashCode(), GetKey(), String.Join(",", m_wishes.Select(w=>w.Version.ToString())));
        }

        private String GetKey()
        {
            return m_prototypeWish.Group + "-" + m_prototypeWish.Name;
        }
    }
}

