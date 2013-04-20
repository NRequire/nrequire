using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace net.nrequire.matcher {
    internal class ExactMatcher : IMatcher<Version> {
        public static String Format = "Major.Minor?.Revision?.(SNAPSHOT|<Timestamp>|<Build>|<Qualifier>)?";

        private static readonly String[] DateFormats = new[] { "yyyyMMddHHmmssfff", "yyyyMMddHHmmss", "yyyy:MMdd:HHmm:ss", "yyyy:MMdd:HHmm:ss:fff" };

        private static readonly char[] Dots = new[] { '.' };

        int? Major { get; set; }
        int? Minor { get; set; }
        int? Revision { get; set; }
        int? Build { get; set; }
        bool Snapshot { get; set; }

        IMatcher<DateTime?> TimeStamp { get; set; }
        String OtherQualifier { get; set; }

        private readonly char m_operator;

        private ExactMatcher(char symbol) {
            m_operator = symbol;
        }

        public bool Match(Version v) {
            if (v == null) {
                return false;
            }
            if (v.IsSnapshot && !Snapshot) {
                return false;
            }

            //TODO: perform a comparision instead of per Major/minor/..
            //want 1.2.4 > 1.2.3 so match when using (1.2.3
            //so compare each field until we get a non zero comparison
            //then decide which symbol we are using. Bring the int macther back in?
            //make it a null capable int comparator?
            var compare = Compare(Major,v.Major);
            if (compare == 0) {
                compare = Compare(Minor, v.Minor);
            }
            if (compare == 0) {
                compare = Compare(Revision, v.Revision);
            }
            if (compare == 0) {
                compare = Compare(Build, v.Build);
            }
            if (!IsCompareInRange(compare)) {
                return false;
            }

            if (Snapshot && !(v.IsSnapshot || v.IsTimestamped)) {
                return false;
            }
            if (!TimeStamp.Match(v.Timestamp)) {
                return false;
            }

            if (OtherQualifier != null && OtherQualifier != v.Qualifier) {
                return false;
            }
            return true;
        }

        private int Compare(int? expect, int actual) {
            if (expect == null) {
                return 0;
            }
            return actual.CompareTo(expect.Value);
        }

        private bool IsCompareInRange(int compare) {
            switch (m_operator) {
                case '=': return compare == 0;
                case '[': return compare >= 0;
                case '(': return compare > 0;
                case ']': return compare <= 0;
                case ')': return compare < 0;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="symbol">what type of matching to perform. one of =()[]. '[' for example means to
        /// match anything greater than or equal to, '(' anything greater etc.</param>
        /// <param name="s"></param>
        /// <returns></returns>
        internal static ExactMatcher Parse(char symbol, String s) {
            try {
                var matcher = new ExactMatcher(symbol);
                var parts = s.Split(Dots);
                matcher.Major = ParseInt(parts[0]);
                if (parts.Length > 3) {
                    matcher.Minor = ParseInt(parts[1]);
                    matcher.Revision = ParseInt(parts[2]);
                    var qualifierPart = parts[3];
                     SetQualifier(matcher, symbol, qualifierPart);
                } else if (parts.Length > 2) {
                    matcher.Minor = ParseInt(parts[1]);
                    if (IsVersionNum(parts[2])) {
                        matcher.Revision = ParseInt(parts[2]);
                    } else {
                        var qualifierPart = parts[2];
                        SetQualifier(matcher, symbol, qualifierPart);
                    }
                } else if (parts.Length > 1) {
                    if (IsVersionNum(parts[1])) {
                        matcher.Minor = ParseInt(parts[1]);
                    } else {
                        var qualifierPart = parts[1];
                        SetQualifier(matcher, symbol, qualifierPart);
                    }
                }
                if (matcher.TimeStamp == null) {
                    matcher.TimeStamp = DateTimeMatcher.EqualToAny();
                }
                return matcher;
            } catch (Exception e) {
                throw NewInvalidFormat(s, e);
            }
        }

        private static bool IsVersionNum(string s) {
            int i;
            if (int.TryParse(s, out i)) {
                if (i < 0) {
                    throw new ArgumentException(String.Format("Value must be positive but was '{0}'",s));
                }
                return true;
            }
            return false;
        }

        private static int? ParseInt(String s) {
            if (s == null || s.Length == 0) {
                return null;
            }
            int intVal;
            if (int.TryParse(s, out intVal)) {
                if (intVal < 0 || s[0] == '-') {
                    throw new ArgumentException(String.Format("Value must be positive but was '{0}'",s));
                }
                return intVal;
            }
            throw new ArgumentException(String.Format("Could not parse '{0}' as an int", s));
        }

        private static void SetQualifier(ExactMatcher matcher, char symbol, string s) {
            if (String.IsNullOrEmpty(s)) {
                return;
            }
            if (s.Equals("SNAPSHOT")) {
                matcher.Snapshot = true;
                return;
            }
            long num;
            if (long.TryParse(s, out num)) {
                if (num < 0) {
                    throw new ArgumentException("Invalid build number, needs to be a positive integer");
                }
                if (num == 0) {
                    //TODO:what here?
                    //v.Qualifier = null;
                    //v.m_qual = Qual.None;
                    return;
                }
                if (s.Length >= 14) {
                    DateTime ts;
                    if (DateTime.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out ts)) {
                        matcher.TimeStamp = DateTimeMatcher.From(symbol, ts);
                        return;
                    }
                }
                if (num < int.MaxValue) {
                    matcher.Build = (int)num;
                    return;
                }
                return;
            }
            matcher.OtherQualifier = s;
            return;
        }

        private static ArgumentException NewInvalidFormat(String s) {
            return new ArgumentException(String.Format("Invalid version match string '{0}' expected format is {1}", s, Format));
        }

        private static ArgumentException NewInvalidFormat(String s, Exception e) {
            return new ArgumentException(String.Format("Invalid version match string '{0}' expected format is {1}", s, Format), e);
        }

        public override String ToString() {
            return String.Format("Exact<{0}.{1}.{2}.{3}>",Major,Minor,Revision,Build);
        }
    }
}
