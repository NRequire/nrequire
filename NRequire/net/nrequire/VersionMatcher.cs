using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace net.nrequire {
    public class VersionMatcher {

        private ExactMatch m_match;
        private VersionMatcher(ExactMatch match) {
            m_match = match;
        }

        public static VersionMatcher Parse(String versionMatch) {
            //http://docs.codehaus.org/display/MAVEN/Dependency+Mediation+and+Conflict+Resolution
            return new VersionMatcher(ExactMatch.Parse(versionMatch));
        }

        public bool Match(String versionString) {
            return Match(Version.Parse(versionString));
        }

        public bool Match(Version v) {
            return m_match.Match(v);
        }

        private class ExactMatch {

            private static readonly String[] DateFormats = new[] { "yyyyMMddHHmmssfff", "yyyyMMddHHmmss", "yyyy:MMdd:HHmm:ss", "yyyy:MMdd:HHmm:ss:fff" };

            private static readonly char[] Dots = new[] { '.' };
            private static readonly char[] Dashes = new[] { '-' };

            int? Major { get;set;}
            int? Minor { get; set; }
            int? Revision { get; set; }
            int? Build { get; set; }
            bool Snapshot { get; set; }
            
            DateTime? TimeStamp { get;  set; }
            String OtherQualifier { get; set; }
            
            private ExactMatch() {
            }

            internal bool Match(Version v) {
                if (v == null) {
                    return false;
                }
                if (v.IsSnapshot && !Snapshot) {
                    return false;
                }
                
                if (Major != null && Major != v.Major) {
                    return false;
                }
                if (Minor != null && Minor != v.Minor) {
                    return false;
                }
                if (Revision != null && Revision != v.Revision) {
                    return false;
                }
                if (Build != null && Build != v.Build) {
                    return false;
                }
                if (Snapshot && !(v.IsSnapshot || v.IsTimestamped)) {
                    return false;
                }
                if (TimeStamp !=null && !v.IsTimestamped && !TimeStamp.Equals(v.Timestamp)) {
                    return false;
                }

                if (OtherQualifier != null && OtherQualifier != v.Qualifier) {
                    return false;
                }
                return true;
            }

            internal static ExactMatch Parse(String s) {
                try {
                    var matcher = new ExactMatch();

                    var parts = s.Split(Dashes);
                    if (parts.Length > 2) {
                        throw NewInvalidFormat(s);
                    } 
                    if (parts.Length == 2) {
                        var q = parts[1].Trim();
                        if (String.IsNullOrEmpty(q)) {
                            throw NewInvalidFormat(s);
                        }
                        try {
                            SetQualifier(matcher, q);
                        } catch (ArgumentException e) {
                            throw NewInvalidFormat(s, e);
                        }
                    }
                    var numParts = parts[0].Split(Dots);
                    if (numParts.Length == 0 || numParts.Length > 3) {
                        throw NewInvalidFormat(s);
                    }
                    matcher.Major = int.Parse(numParts[0]);
                    if (numParts.Length > 1) {
                        matcher.Minor = int.Parse(numParts[1]);
                        if (numParts.Length > 2) {
                            matcher.Revision = int.Parse(numParts[2]);
                        }
                    }
                    return matcher;
                } catch (Exception e) {
                    throw NewInvalidFormat(s, e);
                }
            }

            private static void SetQualifier(ExactMatch matcher, string s) {
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
                            matcher.TimeStamp = ts;
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
                return new ArgumentException(String.Format("Invalid version match string '{0}', expected format is Major.Minor?.Revision?-(SNAPSHOT|<Timestamp>|<Build>|<Qualifier>)?", s));
            }

            private static ArgumentException NewInvalidFormat(String s, Exception e) {
                return new ArgumentException(String.Format("Invalid version match string '{0}', expected format is Major.Minor?.Revision?-(SNAPSHOT|<Timestamp>|<Build>|<Qualifier>)?", s), e);
            }
        }
    }
}
