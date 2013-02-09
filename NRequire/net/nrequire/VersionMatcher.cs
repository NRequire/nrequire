using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {
    public class VersionMatcher {

        private VersionMatcher() {

        }

        public static VersionMatcher Parse(String versionMatch) {
            //http://docs.codehaus.org/display/MAVEN/Dependency+Mediation+and+Conflict+Resolution
            return new VersionMatcher();
        }

        public bool Match(String versionString) {
            return Match(Version.Parse(versionString));
        }

        public bool Match(Version v) {
            return false;
        }
    }
}
