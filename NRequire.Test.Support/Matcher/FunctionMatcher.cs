using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Matcher {

    public static class FunctionMatcher {
        public static FunctionMatcher<T> For<T>(Func<T, bool> matchFunction, Func<String> misMatchMessageFactory) {
            return new FunctionMatcher<T>(matchFunction, misMatchMessageFactory, false);
        }

        public static FunctionMatcher<T> ForAllowNull<T>(Func<T, bool> matchFunction, Func<String> misMatchMessageFactory) {
            return new FunctionMatcher<T>(matchFunction, misMatchMessageFactory, true);
        }
    }
    public class FunctionMatcher<T> : ExtendedMatcher<T> {
        private readonly Func<T, bool> m_matchFunction;
        private readonly Func<String> m_misMatchMessageFactory;
        private readonly bool m_allowNullValue;

        internal FunctionMatcher(Func<T, bool> matchFunction, Func<String> misMatchMessageFactory, bool allowNullValue) {
            m_matchFunction = matchFunction;
            m_misMatchMessageFactory = misMatchMessageFactory;
            m_allowNullValue = allowNullValue;
        }

        public override bool Match(T actual, IMatchDiagnostics diagnostics) {
            if (!m_allowNullValue && actual == null) {
                diagnostics.Fail("was null but expected " + m_misMatchMessageFactory.Invoke());
                return false;
            }
            if (m_matchFunction.Invoke(actual)) {
                return true;
            }
            diagnostics.Fail( "got '" + actual + " but expected " + m_misMatchMessageFactory.Invoke());
            return false;
        }

        public override String ToString(){
            return m_misMatchMessageFactory.Invoke();
        }
    }
}
