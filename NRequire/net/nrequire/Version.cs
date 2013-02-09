using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace net.nrequire {

    //goto to major.minor.revision.build|timestamp|SNAPSHOT|qualifier to simplify parsing (like in gradle)
    public class Version {

        private static readonly String[] DateFormats = new[] { "yyyyMMddHHmmssfff", "yyyyMMddHHmmss", "yyyy:MMdd:HHmm:ss", "yyyy:MMdd:HHmm:ss:fff"};
        private static readonly char[] Dots = new[] { '.'};
        private static readonly char[] Dashes = new[] { '-' };

        public int Major { get; set; }

        public int Minor { get; set; }

        public int Revision { get; set; }

        public String Qualifier { get; set; }

        public int Build { get; set; }
        public DateTime? Timestamp { get; set; }

        public bool IsTimestamped { get { return m_qual == Qual.Timestamp; } }

        public bool IsSnapshot { get { return m_qual == Qual.Snapshot; } }

        public bool IsBuild { get { return m_qual == Qual.Build; } }

        public bool IsQualified { get { return m_qual != Qual.None; } }

        private Qual m_qual;
        private enum Qual {
            None, Timestamp, Snapshot, Build, Other
        }

        private Version() {
        }

        private static int CheckInRange(int num, String name) {
            if (num < 0 ) {
                throw new ArgumentException(String.Format("Expect '{0}' to be > 0 but was {1}",name,num));
            }
            return num;
        }

        public static Version Parse(String s) {
           try {
                var v = new Version();

                var parts = s.Split(Dashes);
                if (parts.Length == 0 || parts.Length > 2) {
                    throw NewInvalidFormat(s);
                }
                if (parts.Length == 2) {
                    var q = parts[1].Trim();
                    if(String.IsNullOrEmpty(q)){
                        throw NewInvalidFormat(s);
                    }
                    try {
                        SetQualifier(v,q);
                    } catch (ArgumentException e) {
                        throw NewInvalidFormat(s,e);
                    }
                }
                var numParts = parts[0].Split(Dots);
                if (numParts.Length < 2 || numParts.Length > 3) {
                    throw NewInvalidFormat(s);
                }
                v.Major = int.Parse(numParts[0]);
                v.Minor = int.Parse(numParts[1]);
                if (numParts.Length == 3) {
                    v.Revision = int.Parse(numParts[2]);
                }
                return v;
           } catch (Exception e) {
               throw NewInvalidFormat(s, e);
           }
        }

        private static void SetQualifier(Version v, string s) {
            if (String.IsNullOrEmpty(s)) {
                return;
            }
            v.Qualifier = s;
            
            if (s.Equals("SNAPSHOT")) {
                v.m_qual = Qual.Snapshot;
                return;
            }
            long num;
            if (long.TryParse(s, out num)) {
                if (num < 0) {
                    throw new ArgumentException("Invalid build number, needs to be a positive integer");
                }
                if (num == 0) {
                    v.Qualifier = null;
                    v.m_qual = Qual.None;
                    return;
                }
                if (s.Length >= 14) {
                    DateTime ts;
                    if (DateTime.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out ts)) {
                        v.m_qual = Qual.Timestamp;
                        v.Timestamp = ts;
                        return;
                    }
                } 
                if (num < int.MaxValue) {
                    v.m_qual = Qual.Build;
                    v.Build = (int)num;
                    return;
                }
                throw new ArgumentException("Invalid qualifier, is a number but neither a valid build number or a datetime");
            }
            v.m_qual = Qual.Other;
            return;
        }

        private static ArgumentException NewInvalidFormat(String s) {
            return new ArgumentException(String.Format("Invalid version string '{0}', expected format is Major.Minor.Revision?-(SNAPSHOT|<Timestamp>|<Build>|<Qualifier>)?", s));
        }

        private static ArgumentException NewInvalidFormat(String s, Exception e) {
            return new ArgumentException(String.Format("Invalid version string '{0}', expected format is Major.Minor.Revision?-(SNAPSHOT|<Timestamp>|<Build>|<Qualifier>)?", s), e);
        }

        public override String ToString() {
            var sb = new StringBuilder();
            sb.Append(Major).Append('.');
            sb.Append(Minor).Append('.');
            sb.Append(Revision);
            if (IsQualified) {
                sb.Append("-").Append(Qualifier);
            }
            return sb.ToString();
        }

        public String ToMatchString() {
            var sb = new StringBuilder();
            sb.Append(Major).Append('.');
            sb.Append(Minor).Append('.');
            sb.Append(Revision).Append('.');
            if (IsQualified) {
                sb.Append("-").Append(Qualifier);
            } else {
                sb.Append("-0");
            }
            return sb.ToString();
        }
    }
}
