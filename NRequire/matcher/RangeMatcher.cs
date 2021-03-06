﻿using Version = NRequire.Model.Version;

namespace NRequire.Matcher {
    internal class RangeMatcher : IMatcher<Version> {
        private static readonly IMatcher<Version> AlwaysTrue = new AlwaysTrueMatcher<Version>();

        internal IMatcher<Version> From { get; set; }
        internal IMatcher<Version> To { get; set; }

        internal RangeMatcher() {
            From = AlwaysTrue;
            To = AlwaysTrue;
        }

        public bool Match(Version v) {
            if (!From.Match(v)) {
                return false;
            }
            if (!To.Match(v)) {
                return false;
            }
            return true;
        }

    }
}
