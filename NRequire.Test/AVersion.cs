using System;
using TestFirst.Net;
using TestFirst.Net.Matcher;
using Version = NRequire.Model.Version;

namespace NRequire {
    public partial class AVersion {

        public static IMatcher<Version> EqualTo(Version expect)
        {
            if (expect == null)
            {
                return AnInstance.Null<Version>();
            }
            return EqualTo(expect.ToString());
        }
        
        public static IMatcher<Version> EqualTo(String expect)
        {
            return Matchers.Function<Version>(actual=>expect.Equals(actual.ToString()),expect);
        }

        public static IMatcher<Version> VersionRange(String range)
        {
            return VersionRange(VersionMatcher.Parse(range));
        }

        public static IMatcher<Version> VersionRange(VersionMatcher expect)
        {
            return Matchers.Function<Version>(
                (actual) => expect.Match(actual),
                () => "AVersion matching version range " + expect);
        }
    }
}
