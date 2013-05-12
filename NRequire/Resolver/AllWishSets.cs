using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NRequire.Matcher;
using System.Collections.Generic;
using System.Collections;

namespace NRequire.Resolver
{
    internal class AllWishSets {

        private static readonly Logger Log = Logger.GetLogger(typeof(AllWishSets));
        private readonly IDependencyCache m_cache;
        private readonly int m_depth;
        private readonly Dictionary<String, WishSet> m_wishSetsByKey;
        //the keys of the wishlists we have added or wrapped. Allows us to determine if we can make a change to a list
        //or if we need to wrap it first
        private readonly List<String> m_localModifiedKeys = new List<String>();

        private IEnumerable<String> LocalKeys {
            get { return m_localModifiedKeys; }
        }

        public AllWishSets(IDependencyCache cache) {
            m_depth = 0;
            m_cache = cache;
            m_wishSetsByKey = new Dictionary<string, WishSet>();
        }

        private AllWishSets(IDependencyCache cache, AllWishSets parent, int depth){
            m_depth = depth;
            m_cache = cache;
            m_wishSetsByKey = new Dictionary<string, WishSet>(parent.m_wishSetsByKey);
        }

        //return a shallow copy of this list
        public AllWishSets NewChild() {
            return new AllWishSets(m_cache, this, m_depth + 1);
        }

        public IEnumerable<WishSet> FindAllUnfixedWishSets() {
            return m_wishSetsByKey.Values.Where(w => !w.IsFixed());
        }

        public bool IsAllResolved() {
            return m_wishSetsByKey.Values.All(w => w.IsFixed());
        }

        public bool WishExists(DependencyWish wish) {
            var key = Key(wish);
            WishSet wishes;
            if (m_wishSetsByKey.TryGetValue(key, out wishes)) {
                return wishes.ContainsVersion(wish.Version);
            }
            return false;
        }

        public IEnumerable<Dependency> FindAllResolved() {
            return m_wishSetsByKey.Values
                .Where(w => w.IsFixed())
                    .Select(w => w.FindMatches().First());
        }

        /// <summary>
        /// Add a wish unless there is already a matching constraint
        /// </summary>
        /// <returns>true if added, false if there was already a wish matching the same requirements</returns>
        public bool AddWish(DependencyWish wish) {
            //only add the wish if not already added in self or a parent
            if (WishExists(wish)) {
                return false;
            }
            return LocalWishSetFor(wish).AddIfNotExists(wish);
        }

        public void ResolveWhatCanBe(){
            var wishesAdded = false;
            //ensure the fixed versions requirements are also added to the requirements
            var keysUpdated = new List<String>();//to prevent pointless recheck of what we already just added
            do {
                wishesAdded = false;
                foreach (var key in LocalKeys.Where(k=>!keysUpdated.Contains(k)).ToList()) {//because list is modified
                    var wishSet = m_wishSetsByKey[key];
                    if( wishSet.IsFixed()){
                        keysUpdated.Add(key);
                        var dep = wishSet.FindMatches().First();//is fixed so should only be one
                        Log.Debug("finding wishes for fixed dependency : " + dep.Summary());
                        foreach (var wish in m_cache.FindWishesFor(dep)) {
                            if (AddWish(wish)) { //if we don't already have it
                                wishesAdded = true;
                            }
                        }
                    }
                }
            } while( wishesAdded );//adding wishes might cause resolution, so lets keep going until no more changes
        }

        public WishSet LocalWishSetFor(DependencyWish wish) {
            var key = Key(wish);
            //already added our filter
            if (m_localModifiedKeys.Contains(key)) {
                return m_wishSetsByKey[key];
            }
            m_localModifiedKeys.Add(key);
            WishSet existingWishes;
            if (m_wishSetsByKey.TryGetValue(key, out existingWishes)) { //if we already have a wishlist for this type then add another version constraint on to it
                var wrappingSet = new WishSet(wish, existingWishes);
                m_wishSetsByKey[key] = wrappingSet;
                Log.Trace("Wrapped existing wishset : " + existingWishes.Summary());
                return wrappingSet;
            } else { //a new key and therefore wishlist
                var newSet = new WishSet(wish, m_cache);
                m_wishSetsByKey[key] = newSet;
                Log.Trace("Added new wishset : " + newSet.Summary());
                return newSet;
            }
        }

        public bool CanMatch() {
            try {
                CanMatchOrThrow();
                return true;
            } catch (ResolverException) {
                return false;
            }
        }

        public void CanMatchOrThrow() {
            foreach (var key in LocalKeys) {
                var wishes = m_wishSetsByKey[key];
                if (!wishes.CanMatch()) {
                    throw new ResolverException( ResolverException.NoSolutions + ", could not find matching dependencies for wishes : " + wishes);
                }
            }
        }

        public void PrintResolvedSoFar() {
            Log.Debug("effectively fixed versions so far");
            var found = false;
            foreach (var wishList in m_wishSetsByKey.Values.Where(w => w.IsFixed())) {
                Log.Debug("-->" + wishList.Summary());
                found = true;
            }
            if (!found) {
                Log.Debug("no fixed versions yet");
            }
        }

        public void PrintNeedResolving() {
            Log.Debug("need to resolve wishes");
            var found = false;
            foreach (var wishList in m_wishSetsByKey.Values.Where(w => !w.IsFixed())) {
                Log.Debug("->" + wishList.Summary());
                found = true;
            }
            if (!found) {
                Log.Debug("nothing to resolve");
            }
        }

        private static String Pad(String padWith, int width) {
            var sb = new StringBuilder();
            for (int i = 0; i < width; i++) {
                sb.Append(padWith);
            }
            return sb.ToString();
        }

        internal static String Key(DependencyWish wish) {
            return wish.Group + "-" + wish.Name;
        }
    }
}

