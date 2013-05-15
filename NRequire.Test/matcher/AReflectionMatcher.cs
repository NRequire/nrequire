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
            var propMatcher = new PropertyFunctionMatcher<T, TProperty>(propertyName, valueExtractor, matcher);
            Add(propMatcher);
        }

        protected void Add(IExtendedMatcher<T> matcher) {
            m_matchers.Add(matcher);
        }


    }

    class PropertyMatcher<T, TProperty> : ExtendedMatcher<T> {
        private readonly PropertyInfo m_prop;
        private readonly IExtendedMatcher<TProperty> m_propMatcher;

        internal PropertyMatcher(string propName, IExtendedMatcher<TProperty> propMatcher) {
            m_prop = typeof(T).GetProperty(propName);
            if (m_prop == null) {
                throw new ArgumentException(String.Format("No property named '{0}' on type '{1}' found",
                    propName, typeof(T).FullName));
            }
            if (!typeof(TProperty).IsAssignableFrom(m_prop.PropertyType)) {
                throw new ArgumentException(String.Format("Property named '{0}' is of type '{1}' but matcher provided is for type '{3}'",
                    propName,m_prop.PropertyType.FullName,typeof(TProperty).FullName));
            }
            m_propMatcher = propMatcher;
        }

        public override bool Match(T instance,IMatchDiagnostics diagnostics) {
            var propVal = m_prop.GetValue(instance,null);
            return m_propMatcher.Match((TProperty)propVal);
        }
    }

    class PropertyFunctionMatcher<T, TProperty> : ExtendedMatcher<T> {
        private readonly PropertyInfo m_prop;
        private readonly IExtendedMatcher<TProperty> m_propMatcher;
        private readonly Func<T, TProperty> m_valueExtractor;

        internal PropertyFunctionMatcher(string propName, Func<T, TProperty> valueExtractor, IExtendedMatcher<TProperty> propMatcher) {
            m_valueExtractor = valueExtractor;
            m_propMatcher = propMatcher;
        }

        public override bool Match(T instance, IMatchDiagnostics diagnostics) {
            var propVal = m_valueExtractor.Invoke(instance);
            return m_propMatcher.Match((TProperty)propVal,diagnostics);
        }
    }
}
