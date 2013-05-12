using System;

namespace NRequire.Resolver
{
    static class Extensions
    {

        internal static String Summary(this Dependency d)
        {
            if (d == null) {
                return null;
            }
            return d.Name + "-" + d.Version;
        }

        internal static String Summary(this DependencyWish wish)
        {
            if (wish == null) {
                return null;
            }
            return wish.Name + "-" + wish.Version;
        }

        internal static String Summary(this WishSet wishList)
        {
            if (wishList == null) {
                return null;
            }
            return wishList.ToString();
        }
    }
}

