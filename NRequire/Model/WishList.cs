using System;
using System.Collections.Generic;
using NRequire.Resolver;

namespace NRequire.Model {
    public class WishList {
        private Dictionary<Key, Wish> m_wishesByKey = new Dictionary<Key, Wish>();

        public void MergeInChildren(List<Wish> wishes, int depth) {
            wishes.ForEach(MergeInChild);
        }

        public void MergeInChild(Wish wish) {
            var key = wish.Key;

            Wish existing;
            if (m_wishesByKey.TryGetValue(key, out existing)) {
                m_wishesByKey[key] = wish.CloneAndFillInBlanksFrom(existing);
            } else {
                m_wishesByKey[key] = wish;
            }
        }

        public void AddOrFailIfExists(List<Wish> wishes, int depth) {
            wishes.ForEach(AddOrFailIfExists);
        }

        public void AddOrFailIfExists(Wish wish) {
            var key = wish.Key;
            if (m_wishesByKey.ContainsKey(key)) {
                throw new ResolverException("Duplicate wish :" + wish);
            }
            m_wishesByKey[key] = wish;
        }

        public List<Wish> ToList() {
            return new List<Wish>(m_wishesByKey.Values);
        }
    }
}
