using System;
using System.Collections.Generic;

namespace NRequire {

    /// <summary>
    /// Provides ability to track where resources were loaded from
    /// </summary>
    public class SourceLocations {

        private readonly Dictionary<String,ISource> m_sourcesByName = new Dictionary<String,ISource>();

        public SourceLocations() {
        }

        public SourceLocations(String location) {
            Add(FromName(location));
        }

        public SourceLocations(ISource location) {
            Add(location);
        }

        public static ISource FromName(string sourceName) {
            return new NamedSource(sourceName);
        }

        public SourceLocations Add(SourceLocations locations) {
            if (locations == null) {
                return this;
            }
            foreach (var location in locations.m_sourcesByName.Values) {
                Add(location);
            }
            return this;
        }

        public SourceLocations Add(ISource source) {
            if (!m_sourcesByName.ContainsKey(source.SourceName)) {
                m_sourcesByName.Add(source.SourceName,source);
            }
            return this;
        }

        public override String ToString() {
            return "from:" + String.Join(",", m_sourcesByName.Keys);
        }

        public static void AddToSourceLocations(IEnumerable<Object> items, SourceLocations locations) {
            if (locations == null || items == null) {
                return;
            }
            foreach (var item in items) {
                AddToSourceLocations(item, locations);
            }
        }

        public static void AddToSourceLocations(Object obj, SourceLocations locations) {
            if (locations == null) {
                return;
            }
            var source = GetOrSet(obj);
            if (source != null) {
                foreach (var location in locations.m_sourcesByName.Values) {
                    source.Add(location);
                }
            }
        }

        public static void AddToSourceLocations(Object obj,ISource location) {
            var source = GetOrSet(obj);
            if (source != null) {
                source.Add(location);
            }
        }

        private static SourceLocations GetOrSet(Object obj) {
            var taker = obj as ITakeSourceLocation;
            if (taker != null) {
                var source = taker.Source;
                if (source == null) {
                    source = new SourceLocations();
                    taker.Source = source;
                }
                return source;
            }
            return null;
        }

        private class NamedSource : ISource {
            public String SourceName {get;private set; }
            internal NamedSource(String name) {
                SourceName = name;
            }
        }
    }
}
