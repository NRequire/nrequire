//
//  Copyright 2013  bert
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
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

