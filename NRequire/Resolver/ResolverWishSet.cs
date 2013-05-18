using System;
using System.Linq;
using System.Linq.Expressions;
using NRequire.Matcher;
using System.Collections.Generic;
using System.Collections;

namespace NRequire
{
	//holds a collection of constraints for a given dependency
    internal class ResolverWishSet : IMatcher<Version>
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(ResolverWishSet));
        //all the additional wishes added to provide additional filtering of the parent provided lists of dependencies
        private readonly List<Wish> m_wishes = new List<Wish>();
        //to allow faster checking if wishes have already been added
        private readonly List<String> m_allVersionStrings;
        //the dependencies which come for the parent list/cache on top of which we apply our own additional filters
        private readonly IList<Dependency> m_parentDependencies;
        //the dependencies which matched once the wishes have been applied. Cache to prevent repeated lookups 
        private IList<Dependency> m_cachedFilteredDeps;
        private IEnumerator<Dependency> m_nextDepsPicker;
        //TODO:move out of here to allow different strategies?
        //first wish added, used to validate all further added wishes
        public Wish FirstWish  { get; private set; }
        private readonly String m_key;
        public Scopes HighestScope { get; private set; }


        internal ResolverWishSet(Wish wish, IDependencyCache cache)
        {
            FirstWish = wish;
            HighestScope = wish.Scope;

            m_parentDependencies = cache.FindDependenciesMatching(wish);
            m_allVersionStrings = new List<String>();
            m_key = wish.Signature();
        }

        internal ResolverWishSet(Wish wish, ResolverWishSet parentWishList)
        {
            m_parentDependencies = parentWishList.FindMatchingDependencies();
            FirstWish = wish;
            m_allVersionStrings = new List<string>(parentWishList.m_allVersionStrings);
            m_key = wish.Signature();
            HighestScope = parentWishList.HighestScope;
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
        internal bool AddIfNotExists(Wish wish)
        {
            CheckNotPicking();
            CheckKeys(wish);

            if (wish.Scope > HighestScope ) {
                HighestScope = wish.Scope;
            }
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

        private void CheckKeys(Wish wish) {
            if (wish.Signature() != m_key) {
                throw new ArgumentException(String.Format("wish signatures don't match. Expected '{0}' but got '{1}'", m_key, wish.Signature()));
            }
        }
		/// <summary>
		/// If there are still deps which can satisfy the criteria
		/// </summary>
		/// <returns><c>true</c> if this instance can match; otherwise, <c>false</c>.</returns>
        public bool CanMatch()
        {
            return FindMatchingDependencies().Count() > 0;
        }

        public bool RequiresResolution(){
            return !HasOnlyTransitive();
        }

        public bool HasOnlyTransitive() {
            return HighestScope <= Scopes.Transitive;
        }

		/// <summary>
		/// If there is only a single dependency which can match all the criteria
		/// </summary>
		/// <returns><c>true</c> if this instance is resolved; otherwise, <c>false</c>.</returns>
        public bool IsFixed()
        {
            return FindMatchingDependencies().Count() == 1;
        }

        public bool ContainsScope(Scopes scope)
        {
            return scope <= HighestScope;
        }

        public Dependency PickNextFixedDependency()
        {
            if (m_nextDepsPicker == null) {
                m_nextDepsPicker = FindMatchingDependencies().GetEnumerator();
            }
            if (m_nextDepsPicker.MoveNext()) {
                if (Log.IsTraceEnabled()) {
                    Log.Trace("picked from versions : " + String.Join(",", FindMatchingDependencies().Select(d => d.Version)));
                }
                return m_nextDepsPicker.Current;
            }
            return null;
        }
        /// <summary>
        /// Gets matching dependencies or empty if no matches
        /// </summary>
        public IList<Dependency> FindMatchingDependencies()
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
            return String.Format("DependencyWishList@{0}<key={1},wishes={2}>", base.GetHashCode(), m_key, String.Join(",", m_wishes.Select(w=>w.Version.ToString())));
        }
    }
}

