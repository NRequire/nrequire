using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NRequire.Matcher;
using System.Collections.Generic;
using System.Collections;

namespace NRequire.Resolver
{
	//tries to find a list of deps which satsify a set of dependencies (and their wishes) 
	//and a list of wishes  
    internal class DependencyResolverV2
    {
        private readonly Logger Log = Logger.GetLogger(typeof(DependencyResolverV2));
        private TestDependencyCache m_cache;

        private static readonly int RecursionLimitDepth = 150;
        private static readonly int RecursionLimitWidth = 300;

        internal DependencyResolverV2(TestDependencyCache cache)
        {
            m_cache = cache;
        }

        public List<Dependency> Resolve(IList<DependencyWish> require)
        {
            var root = new ResolverLine(m_cache);
            foreach (var wish in require) {
                root.AddWish(wish);
            }
            var resolved = Resolve(root);
            if (Log.IsDebugEnabled()) {
                Log.Debug("resolved dependencies:");
                foreach (var dep in resolved) {
                    Log.Debug(dep.Summary());
                }
            }
            return resolved;
        }

        private List<Dependency> Resolve(ResolverLine firstLine)
        {
            var resolved = new List<Dependency>();
            firstLine.ResolveWhatCanBe();
            firstLine.CanMatchOrThrow();
            if (firstLine.IsAllResolved()) {
                Log.Debug("line resolved, collecting results");
                resolved = firstLine.FindAllResolved();
            } else {
                if (!PickNextLine(firstLine, resolved, 0)) {
                    Log.Info("No solution possible");
                    throw new ResolverException(ResolverException.NoSolutions);
                }
            }
            return resolved.OrderBy(d=>d.Group + "-" + d.Name).ToList();
        }

        private bool PickNextLine(ResolverLine line, List<Dependency> resolved, int depth)
        {
            Log.Trace("Resolving down graph to depth " + depth);
            if (depth > RecursionLimitDepth) {
                throw new ResolverException(ResolverException.InfiniteRecursion + ", in depth");
            }
            var nodePosition = 0;
            for (var nextLine = line.PickNextLine(); nextLine!=null; nextLine = line.PickNextLine()) {
                Log.Debug("picked next line at position " + nodePosition  + " at depth " + depth);
                nodePosition++;
                
                if (nodePosition > RecursionLimitWidth) {
                    throw new ResolverException(ResolverException.InfiniteRecursion + ", in sideways traverse");
                }
                if (nextLine.IsAllResolved()) {
                    Log.Debug("Line resolved, collecting results");
                    var found = nextLine.FindAllResolved();
                    resolved.AddRange(found);
                    return true;
                } else {
                    Log.Trace("try to pick a version and walk down");
                    if (PickNextLine(nextLine, resolved, depth + 1)) {
                        return true;
                    } else {
                        Log.Trace("Couldn't resolve by picking a version, trying sideways at depth " + depth + " position " + nodePosition);
                    }
                }
            }
            return false;
        }
    }
}

