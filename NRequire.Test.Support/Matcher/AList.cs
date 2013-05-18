using System;
using System.Collections.Generic;

namespace NRequire.Matcher
{
    public static class AList
    {
        public static IExtendedMatcher<IEnumerable<T>> Empty<T>() {
            return new InOrderListWithOnly<T>();
        }

        public static IExtendedMatcher<IEnumerable<T>> WithOnly<T>(IExtendedMatcher<T> matcher) {
            return InOrder().With(matcher);
        }

        public static InOrderList InOrder() {
            return new InOrderList();
        }


        public class InOrderList {

            internal InOrderList(){}

            public InOrderListWithOnly<T> With<T>(IExtendedMatcher<T> matcher) {
                return new InOrderListWithOnly<T>().And(matcher);
            }

            public IExtendedMatcher<IEnumerable<T>> WithOnly<T>(params IExtendedMatcher<T>[] matchers) {
                return new AListMatcher<T>(matchers);
            }

            public IExtendedMatcher<IEnumerable<T>> WithOnly<T>(IEnumerable<IExtendedMatcher<T>> matchers) {
                return new AListMatcher<T>(matchers);
            }
        }
        
        public class InOrderListWithOnly<T> : ExtendedMatcher<IEnumerable<T>>
        {
            private readonly List<IExtendedMatcher<T>> m_matchers = new List<IExtendedMatcher<T>>();
            private IExtendedMatcher<IEnumerable<T>> m_cachedMatcher;

            internal InOrderListWithOnly(){}

            public InOrderListWithOnly<T> And(IExtendedMatcher<T> matcher) {
                m_matchers.Add(matcher);
                m_cachedMatcher = null;
                return this;
            }

            public override bool Match(IEnumerable<T> actual, IMatchDiagnostics diag) {
                if (m_cachedMatcher == null) {
                    m_cachedMatcher = AList.InOrder().WithOnly<T>(m_matchers);
                }
                return m_cachedMatcher.Match(actual, diag);
            }
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
                    return false;
                }
               
                for (int i = 0; i < m_expect.Count; i++) {
                    var expectItemMatcher = m_expect[i];
                    var actualItem = actualList[i];
                    if (!expectItemMatcher.Match(actualItem, diag)) {
                        diag.Fail("AList was : ");
                        if (diag.Enabled) {
                            diag.Print("Expected:");
                            PrintAll(m_expect, diag);
                            diag.Print("Actual:");
                            PrintAll(actualList, diag);
                        }
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
                    diag.Print("[" + index + "] " + item);
                    index ++;
                }
            }
        }
    }
}

