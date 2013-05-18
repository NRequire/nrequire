using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using NRequire.Matcher;

namespace NRequire {
    public class AReflectionMatcher<T> : ExtendedMatcher<T> {

        private IList<IExtendedMatcher<T>> m_matchers = new List<IExtendedMatcher<T>>();

        public override bool Match(T instance, IMatchDiagnostics diag) {
            if (instance == null) {
                return false;
            }
            foreach (var p in m_matchers) {
                if (!p.Match(instance, diag)) {

                    diag.Fail("Above failed on instance " + instance);
                    return false;
                }
            }
            return true;
        }

        protected void AddProperty<TProperty>(String propertyName, IExtendedMatcher<TProperty> matcher) {
            var propMatcher = new PropertyMatcher<T, TProperty>(propertyName, matcher);
            Add(propMatcher);
        }

        protected void AddProperty<TProperty>(String propertyName, Func<T, TProperty> valueExtractor, IExtendedMatcher<TProperty> matcher) {
            var propMatcher = new PropertyMatcher<T, TProperty>(propertyName, valueExtractor, matcher);
            Add(propMatcher);
        }

        protected void Add(IExtendedMatcher<T> matcher) {
            m_matchers.Add(matcher);
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append(typeof(T).Name);
            sb.Append(" with properties [");
            sb.Append(String.Join(",\n\t", m_matchers));
            sb.Append("]");
            return sb.ToString();
        }
    }

    class PropertyMatcher<T, TProperty> : ExtendedMatcher<T> {
        private readonly String m_propName;
        private readonly IExtendedMatcher<TProperty> m_valueMatcher;
        private readonly Func<T, TProperty> m_valueExtractor;

        internal PropertyMatcher(string propName, IExtendedMatcher<TProperty> valueMatcher) {
            var prop = typeof(T).GetProperty(propName);
            if (prop == null) {
                throw new ArgumentException(String.Format("No property named '{0}' on type '{1}' found",
                    propName, typeof(T).FullName));
            }
            if (!typeof(TProperty).IsAssignableFrom(prop.PropertyType)) {
                throw new ArgumentException(String.Format("Property named '{0}' is of type '{1}' but matcher provided is for type '{3}'",
                    propName,prop.PropertyType.FullName,typeof(TProperty).FullName));
            }
            m_propName = propName;
            m_valueMatcher = valueMatcher;
            m_valueExtractor = new Func<T, TProperty>((instance) => (TProperty)prop.GetValue(instance, null));
        }
        
        internal PropertyMatcher(string propName, Func<T, TProperty> valueExtractor, IExtendedMatcher<TProperty> valueMatcher) {
            m_propName = propName;
            m_valueExtractor = valueExtractor;
            m_valueMatcher = valueMatcher;
        }

        public override bool Match(T instance, IMatchDiagnostics diagnostics) {
            var propVal = m_valueExtractor.Invoke(instance);
            return m_valueMatcher.Match((TProperty)propVal,diagnostics);
        }

        public override String ToString() {
            return m_propName + " is " + m_valueMatcher.ToString();
        }
    }
}
