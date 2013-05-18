using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRequire.Test;

namespace NRequire.Matcher {
    public class ADependency : AnAbstractDependency<ADependency,Dependency> {
        public static ADependency With() {
            return new ADependency();
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
    }
}
