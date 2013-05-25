using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire {
    internal static class Extensions {

        internal static string SafeToSummary(this IResolvable resolved) {
            if (resolved == null) {
                return null;
            }
            return resolved.ToSummary();
        }


        //internal static string SafeToSummary(this AbstractDependency dep) {
        //    if (dep == null) {
        //        return null;
        //    }
        //    return dep.ToSummary();
        //}

        internal static string SafeToSummary(this ResolverWishSet set) {
            if (set == null) {
                return null;
            }
            return set.ToSummary();
        }

    }
}
