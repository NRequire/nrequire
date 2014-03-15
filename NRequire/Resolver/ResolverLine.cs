using System;
using System.Collections.Generic;
using System.Linq;
using NRequire.Logging;
using NRequire.Model;
using NRequire.Util;

namespace NRequire.Resolver
{
    class ResolverLine
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(ResolverLine));
     
        private readonly int m_depth;
        private readonly IDependencyCache m_cache;
        ///all the inherited wishlists, plus our own wrapped or added ones by key
        private AllWishSets m_wishSets;

        internal ResolverLine(IDependencyCache cache):this(cache, new AllWishSets(cache), 0)
        {
        }

        private ResolverLine(IDependencyCache cache, AllWishSets wishSets, int depth)
        {
            m_cache = cache;
            m_depth = depth;
            m_wishSets = wishSets;
        }
    	/// <summary>
    	/// Pick the next child line where one more dependency is picked and any resolution without picking another aribitrary version
        /// is also performed
    	/// </summary>
    	/// <returns>Pick the next line with atleast one wish resolved by picking some version. More wishes could be resolved due to this
        /// adding additional wishes for the picked version which causes other wishes to resolve to a fixed dep</returns>
        internal ResolverLine PickNextLine()
        {
            Log.Trace("PickNextLine");
            m_wishSets.PrintResolvedSoFar();
            m_wishSets.PrintNeedResolving();
            //TODO:introduce a strategy here to allow different pick strategies
            //pick a version of each requirement. Pick versions with the least amount of change first
            //a.k.a build incr first, then minor, then major
            foreach (var unfixed in m_wishSets.FindAllUnfixedWishSets().Where(w => !w.HasOnlyTransitive())) {
                Dependency fixedDep;
                do {
                    var nextWishSetToBeFixed = m_wishSets.LocalWishSetFor(unfixed.FirstWish);
                    Log.Debug("picking dep for wishes : " + nextWishSetToBeFixed.ToSummary());
                    fixedDep = nextWishSetToBeFixed.PickNextFixedDependency();
                    Log.Debug("picked dep : " + fixedDep.SafeToSummary());
                    if (fixedDep == null) {
                        Log.Debug("no more options for : " + nextWishSetToBeFixed.SafeToSummary());
                        //no more deps to pick from, lets try to pick another wish to fix tos a set dependency
                        continue;
                    }
                    var nextLine = NewChildLine();
                    var fixedWish = new Wish(fixedDep){Scope = unfixed.HighestScope};
                    Log.Debug("fixing dep using wish : " + fixedWish.SafeToSummary());
                    nextLine.AddWish(fixedWish);
                    nextLine.ResolveWhatCanBe();
                    if (nextLine.CanMatch()) {
                        Log.Debug("returning next line");
                        return nextLine;
                    } else {
                        Log.Debug("Can't match next line, trying next new line");
                    }
                } while( fixedDep != null );
            }
            Log.Trace("no next candidates, returning null");
            return null;
        }
        /// <summary>
        /// Return a list of all resolved dependencies including parent ones
        /// </summary>
        public List<Dependency> FindAllResolved()
        {
            return m_wishSets.FindAllResolvedDeps().ToList();
        }

        public void ResolveWhatCanBe()
        {
            m_wishSets.ResolveWhatCanBe();
        }
        /// <summary>
        /// Add a wish unless there is already a matching constraint
        /// </summary>
        /// <returns>true if added, false if there was already a wish matching the same requirements</returns>
        internal bool AddWish(Wish wish)
        {
            return m_wishSets.AddWish(wish);
        }

        private ResolverLine NewChildLine()
        {
            return new ResolverLine(m_cache, m_wishSets.NewChild(), m_depth + 1);
        }

        /// <summary>
        /// Are all the wishes we have so far resolved?
        /// </summary>
        public bool IsAllResolved()
        {
            return m_wishSets.IsAllResolved();
        }

        private bool CanMatch()
        {
            return m_wishSets.CanMatch();
        }

        internal void CanMatchOrThrow()
        {
            m_wishSets.CanMatchOrThrow();
        }

        private static String Key(Wish wish)
        {
            return wish.GetKey();
        }
    }
}
