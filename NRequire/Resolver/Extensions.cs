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
            return d.Group + ":" + d.Name + ":" + d.Version + ":" + d.Ext;
        }

        internal static String Summary(this Wish wish)
        {
            if (wish == null) {
                return null;
            }
            return wish.Group + ":" + wish.Name + ":" + wish.Version + ":" + wish.Ext;
        }

        internal static String Summary(this ResolverWishSet wishList)
        {
            if (wishList == null) {
                return null;
            }
            return wishList.ToString();
        }
    }
}

