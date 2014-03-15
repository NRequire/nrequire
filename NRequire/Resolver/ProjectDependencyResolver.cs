using System;
using System.Collections.Generic;
using NRequire.Logging;
using NRequire.Model;

namespace NRequire.Resolver {
    
    public class ProjectDependencyResolver {

        private static readonly Logger Log = Logger.GetLogger(typeof(ProjectDependencyResolver));

        public IDependencyCache DepsCache { get; private set; }

        private ProjectDependencyResolver(IDependencyCache cache) {
            DepsCache = cache;
        }

        public static ProjectDependencyResolver WithCache(IDependencyCache cache) {
            return new ProjectDependencyResolver(cache);
        }

        public IList<Dependency> MergeAndResolveDependencies(Solution soln, Project proj) {
            try {
                var wishes = new List<Wish>();
                wishes.AddRange(soln.GetAllWishes());
                wishes.AddRange(proj.GetAllWishes());

                if (Log.IsTraceEnabled()) {
                    Log.Trace("merged wishes=\n" + String.Join("\n",wishes));
                }
                ValidateAll(wishes);

                //now the fun begins. Resolve transitive deps, find closest versions
                //TODO:resolve for all but pick only what the current project needs
                //TODO:keep the calculated cache for how long? how to decide when to recalc all?
                var resolver = new WishDependencyResolver(DepsCache);
                var deps = resolver.Resolve(wishes);

                var projWishes = proj.GetAllWishes();
                //TODO:now line up with deps? to get copy to and additional info added?
                
                return deps;
            } catch (Exception e) {
                throw new ResolverException("Error trying to resolve dependencies for project " + proj + " and solution " + soln, e);
            }
        }

        private void ValidateAll(IEnumerable<Wish> wishes) {
            foreach (var wish in wishes) {
                wish.ValidateRequiredSet();
            }
        }

    /*    private List<Wish> MergeWishes(Solution soln, Project proj) {
            //TODO:remove duplicates and merge wishes, or let cache resolver sort it out? it already does this...

            var mergedWishes = new WishList();
            //not top level? should make transitive and merge only?
            mergedWishes.MergeInChildren(soln.GetAllWishes(),0);
            mergedWishes.MergeInChildren(proj.GetAllWishes(), 0);

            return mergedWishes.ToList();
        }

*/
    }
}
