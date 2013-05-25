using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRequire.Test;
using NRequire.Util;

namespace NRequire.Matcher {
    public class ADependency : AnAbstractDependency<ADependency,Dependency> {
        public static ADependency With() {
            return new ADependency();
        }

        /// <summary>
        /// Parse from  group:name:version:ext:classifiers
        /// </summary>
        /// <param name="fullString">Full string.</param>
        public static ADependency From(String fullString) {
            var d = new ADependency();
            d.SetAllFromParse(fullString);
            return d;
        }

        protected void SetAllFromParse(String fullString) {
            DepParser.Parse(fullString,
            (s) => Group(s),
            (s) => Name(s),
            (s) => Version(s),
            (s) => Ext(s),
            (s) => Classifiers(s));
        }

        public ADependency VersionMatching(String expect) {
            Version(AVersion.Matching(expect));
            return this;
        }

        public ADependency Version(String expect) {
            Version(AVersion.EqualTo(expect));
            return this;
        }

        public ADependency Version(IExtendedMatcher<Version> matcher) {
            AddProperty<Version>("Version", (actual) => actual.Version, matcher);
            return this;
        }

        public ADependency Scope(Scopes expect) {
            Scope(AnInstance.EqualTo(expect));
            return this;
        }

        public ADependency Scope(IExtendedMatcher<Scopes> matcher) {
            AddProperty<Scopes>("Scope", (actual) => actual.Scope, matcher);
            return this;
        }
    }
}
