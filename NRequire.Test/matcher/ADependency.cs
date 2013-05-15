using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Matcher {
    public class ADependency : AReflectionMatcher<Dependency> {
        public static ADependency With() {
            return new ADependency();
        }

        public ADependency Name(String expect) {
            return Name(AString.EqualTo(expect));
        }

        public ADependency Name(IExtendedMatcher<String> matcher) {
            AddProperty<String>("Name", (actual) => actual.Name, matcher);
            return this;
        }

        public ADependency Group(String expect) {
            return Group(AString.EqualTo(expect));
        }

        public ADependency Group(IExtendedMatcher<String> matcher) {
            AddProperty<String>("Group", (actual) => actual.Group, matcher);
            return this;
        }

        public ADependency VersionMatching(String expect) {
            return Version(AVersion.Matching(expect));
        }

        public ADependency Version(String expect) {
            return Version(AVersion.EqualTo(expect));
        }

        public ADependency Version(IExtendedMatcher<Version> matcher) {
            AddProperty<Version>("Version", (actual) => actual.Version, matcher);
            return this;
        }
    }
}
