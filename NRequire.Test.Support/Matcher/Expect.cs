using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NRequire.Matcher {

    public static class Expect {

        public static Expect<T> That<T>(T actual) {
            return new Expect<T>(actual);
        }
    }

    public class Expect<T> {
        private readonly T m_actual;

        public Expect(T actual) {
            m_actual = actual;
        }

        public void Is(IExtendedMatcher<T> matcher) {
            AssertMatches(matcher);
        }

        public void And(IExtendedMatcher<T> matcher) {
            AssertMatches(matcher);
        }

        private void AssertMatches(IExtendedMatcher<T> matcher) {
            var diag = new MatchDiagnostics();
            if (!matcher.Match(m_actual, diag)) {
                Assert.Fail(diag.ToString());
            }
        }
    }

}
