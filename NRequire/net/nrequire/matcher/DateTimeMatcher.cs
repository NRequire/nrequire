using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire.matcher {
    internal class DateTimeMatcher : IMatcher<DateTime?> {

        private static readonly IMatcher<DateTime?> AlwaysTrue = new AlwaysTrueMatcher<DateTime?>();
        private readonly Func<DateTime?, bool> m_comparator;
        
        internal static IMatcher<DateTime?> From(char symbol, DateTime? expect) {
            if (expect == null) {
                return AlwaysTrue;
            }
            switch (symbol) {
                case '(': return new DateTimeMatcher((DateTime? actual) => actual > expect.Value);
                case '[': return new DateTimeMatcher((DateTime? actual) => actual >= expect.Value);
                case ')': return new DateTimeMatcher((DateTime? actual) => actual < expect.Value);
                case ']': return new DateTimeMatcher((DateTime? actual) => actual <= expect.Value);
                case '=': return new DateTimeMatcher((DateTime? actual) => actual == expect.Value);
                default:
                    throw new ArgumentException("Don't recognize match symbol:" + symbol);
            }
        }
        
        private DateTimeMatcher(Func<DateTime?, bool> comp) {
            m_comparator = comp;
        }

        public bool Match(DateTime? actual) {
            return m_comparator.Invoke(actual);
        }

        internal static IMatcher<DateTime?> EqualToAny() {
            return AlwaysTrue;
        }
    }
}
