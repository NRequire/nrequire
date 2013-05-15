using System;
using System.Collections.Generic;

namespace NRequire.Matcher
{
    public static class AList
    {
        public static AListMatcherBuilder<T> With<T>(IExtendedMatcher<T> matcher) {
            return new AListMatcherBuilder<T>().And(matcher);
        }

        public class AListMatcherBuilder<T> : ExtendedMatcher<IEnumerable<T>>
        {
            private readonly List<IExtendedMatcher<T>> m_matchers = new List<IExtendedMatcher<T>>();
            private IExtendedMatcher<IEnumerable<T>> m_cachedMatcher;

            public AListMatcherBuilder<T> And(IExtendedMatcher<T> matcher) {
                m_matchers.Add(matcher);
                m_cachedMatcher = null;
                return this;
            }

            public override bool Match(IEnumerable<T> actual, IMatchDiagnostics diag) {
                if (m_cachedMatcher == null) {
                    m_cachedMatcher = AList.WithOnly<T>(m_matchers);
                }
                return m_cachedMatcher.Match(actual, diag);
            }
        }

        public static IExtendedMatcher<IEnumerable<T>> WithOnly<T>(IEnumerable<IExtendedMatcher<T>>  matchers){
            return new AListMatcher<T>(matchers);
        }

        class AListMatcher<T> : ExtendedMatcher<IEnumerable<T>>{
            private readonly IList<IExtendedMatcher<T>> m_expect;

            internal AListMatcher(IEnumerable<IExtendedMatcher<T>> matchers){
                m_expect = new List<IExtendedMatcher<T>>(matchers);
            }

            public AListMatcher<T> And(IExtendedMatcher<T> matcher) {
                m_expect.Add(matcher);
                return this;
            }

           public override bool Match(IEnumerable<T> actual,IMatchDiagnostics diag) {
                var actualList = new List<T>(actual);

                if (m_expect.Count != actualList.Count) {
                    diag.Fail("list counts don't match, expected " + m_expect.Count + " but got " + actualList.Count);
                    if (diag.Enabled) {
                        diag.Print("Expected:");
                        PrintAll(m_expect,diag);
                        diag.Print("Actual:");
                        PrintAll(actualList,diag);
                    }
                }

                for (int i = 0; i < m_expect.Count; i++) {
                    if (!m_expect[i].Match(actualList[i],diag)){
                        return false;
                    }
                    diag.Pass("[" + i + "]");
                }
                return true;
            }

            private void PrintAll<T>(IEnumerable<T> items, IMatchDiagnostics diag){
                if (!diag.Enabled) {
                    return;
                }
                int index = 0;
                foreach (var item in items) {
                    diag.Print("[" + index + "]" + item);
                    index ++;
                }
            }
        }
    }
}

