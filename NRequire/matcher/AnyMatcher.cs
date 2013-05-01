using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Matcher {
    internal class AnyMatcher : IMatcher<Version> {
        private List<IMatcher<Version>> m_matchers = new List<IMatcher<Version>>();

        internal IMatcher<Version> Collapse() {
            if (m_matchers.Count == 1) {
                return m_matchers[0];
            }
            return this;
        }
        internal void Add(IMatcher<Version> matcher) {
            m_matchers.Add(matcher);
        }

        public bool Match(Version v) {
            foreach (var m in m_matchers) {
                if (m.Match(v)) {
                    return true;
                }
            }
            return false;
        }

        public override String ToString() {
            return "Any<" + String.Join(",", m_matchers) + ">";
        }
    }
}
