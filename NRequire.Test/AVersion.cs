using NRequire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestFirst.Net;
using TestFirst.Net.Matcher;

namespace NRequire {
    public partial class AVersion {

        public static IMatcher<NRequire.Version> EqualTo(NRequire.Version expect)
        {
            if (expect == null)
            {
                return AnInstance.Null<Version>();
            }
            return EqualTo(expect.ToString());
        }
        
        public static IMatcher<NRequire.Version> EqualTo(String expect)
        {
            return Matchers.Function<Version>(actual=>expect.Equals(actual.ToString()),expect);
        }

        public static IMatcher<NRequire.Version> VersionRange(String range)
        {
            return VersionRange(VersionMatcher.Parse(range));
        }

        public static IMatcher<NRequire.Version> VersionRange(VersionMatcher expect)
        {
            return Matchers.Function<NRequire.Version>(
                (actual) => expect.Match(actual),
                () => "AVersion matching version range " + expect);
        }
    }
}
