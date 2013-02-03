using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {

    public class Version {

        private static readonly char[] Dots = new[] { '.' };
        private static readonly char[] Dashes = new[] { '-' };

        public int Major { get; set; }

        public int Minor { get; set; }

        public int Revision { get; set; }

        public String Qualifier { get; set; }

        public int Build { get; set; }

        public bool IsTimestamped { get { return IsQualified && !IsSnapshot; } }

        public bool IsSnapshot { get { return "SNAPSHOT".Equals(Qualifier); } }

        public bool IsQualified { get { return Qualifier != null & Qualifier != "0"; } }

        private Version() {
        }

        public static Version Parse(String s) {
            var v = new Version();
            var parts = s.Split(Dots, StringSplitOptions.RemoveEmptyEntries);

            v.Major = PartAsIntOr(parts, 0, 1);
            v.Minor = PartAsIntOr(parts, 1, 0);
            var revParts = PartOr(parts, 2, "0").Split(Dashes, StringSplitOptions.RemoveEmptyEntries);
            v.Revision = PartAsIntOr(revParts, 0, 0);
            v.Qualifier = PartOr(revParts, 1, "0");
            v.Build = PartAsIntOr(revParts, 2, 0);

            return v;
        }

        private static int PartAsIntOr(String[] parts, int idx, int defVal) {
            return int.Parse(PartOr(parts, idx, defVal.ToString()));
        }

        private static String PartOr(String[] parts, int idx, String defVal) {
            if (parts.Length > idx) {
                return parts[idx];
            }
            return defVal;
        }
    }
}
