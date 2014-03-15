﻿using System;
using System.Collections.Generic;
using NRequire.Resolver;

namespace NRequire.Model {
    public class WishList {
        Dictionary<String, Wish> m_wishesByKey = new Dictionary<String, Wish>();

        public void MergeInChildren(List<Wish> wishes, int depth) {
            wishes.ForEach(w => MergeChild(w));
        }

        public void MergeChild(Wish wish) {
            var key = wish.GetKey();

            Wish existing;
            if (m_wishesByKey.TryGetValue(key, out existing)) {
                m_wishesByKey[key] = wish.CloneAndFillInBlanksFrom(existing);
            } else {
                m_wishesByKey[key] = wish;
            }
        }

        public void AddOrFailIfExists(List<Wish> wishes, int depth) {
            wishes.ForEach(w => AddOrFailIfExists(w));
        }

        public void AddOrFailIfExists(Wish wish) {
            var key = wish.GetKey();
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
