using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Collections;
using NRequire.Resolver;

namespace NRequire {
    
    public class ProjectResolver {

        private static readonly Logger Log = Logger.GetLogger(typeof(ProjectResolver));

        public IDependencyCache DepsCache { get; private set; }

        private ProjectResolver(IDependencyCache cache) {
            DepsCache = cache;
        }

        internal static ProjectResolver WithCache(IDependencyCache cache) {
            return new ProjectResolver(cache);
        }

        public IList<Dependency> MergeAndResolveDependencies(Solution soln, Project proj) {
            try {
                var wishes = MergeWishes(soln, proj).ToList();

                if (Log.IsTraceEnabled()) {
                    Log.Trace("merged wishes=\n" + String.Join("\n",wishes));
                }
                ValidateAllSet(wishes);

                //now the fun begins. Resolve transitive deps, find closest versions
                //TODO:resolve for all but pick only what the current project needs
                //TODO:keep the calculated cache for how long? how to decide when to recalc all?
                var resolver = new WishResolver(DepsCache);
                var deps = resolver.Resolve(wishes);

                var projWishes = proj.GetAllWishes();
                //TODO:now line up with deps? to get copy to and additional info added?
                
                return deps;
            } catch (Exception e) {
                throw new ResolutionException("Error trying to resolve dependencies for project " + proj + " and solution " + soln, e);
            }
        }

        private void ValidateAllSet(List<Wish> wishes) {
            foreach (var wish in wishes) {
                wish.ValidateRequiredSet();
            }
        }

        private List<Wish> MergeWishes(Solution soln, Project proj) {
            //TODO:remove duplicates and merge wishes, or let cache resolver sort it out? it already does this...

            var mergedWishes = new WishList();
            //not top level? should make transitive and merge only?
            mergedWishes.MergeInChildren(soln.GetAllWishes(),0);
            mergedWishes.MergeInChildren(proj.GetAllWishes(), 0);

            return mergedWishes.ToList();
        }
    }
}
