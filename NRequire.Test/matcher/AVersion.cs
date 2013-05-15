using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Matcher {
    public static class AVersion {

        public static IExtendedMatcher<Version> EqualTo(String expect) {
            return EqualTo(Version.Parse(expect));
        }

        public static IExtendedMatcher<Version> EqualTo(Version expect) {
            return AnInstance.EqualTo(expect);
        }

        public static IExtendedMatcher<Version> Matching(String range) {
            return Matching(VersionMatcher.Parse(range));
        }

        public static IExtendedMatcher<Version> Matching(VersionMatcher versionRange) {
            return FunctionMatcher.For<Version>(
                (actual) => versionRange.Match(actual),
                () => "AVersion matching version range " + versionRange);
        }
    }
}
