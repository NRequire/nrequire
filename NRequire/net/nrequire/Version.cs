using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace net.nrequire {

    //goto to major.minor.revision.build|timestamp|SNAPSHOT|qualifier to simplify parsing (like in gradle)
    public class Version :IComparable<Version> {

        private static readonly String[] DateFormats = new[] { "yyyyMMddHHmmssfff", "yyyyMMddHHmmss", "yyyy:MMdd:HHmm:ss", "yyyy:MMdd:HHmm:ss:fff"};
        private static readonly char[] Dots = new[] { '.'};
        private static readonly char[] Dashes = new[] { '-' };

        public int Major { get; private set; }

        public int Minor { get; private set; }

        public int Revision { get; private set; }

        public String Qualifier { get; private set; }

        public int Build { get; private set; }
        public DateTime? Timestamp { get; private set; }

        public bool IsTimestamped { get { return m_qual == Qual.Timestamp; } }

        public bool IsSnapshot { get { return m_qual == Qual.Snapshot; } }

        public bool IsBuild { get { return m_qual == Qual.Build; } }

        public bool IsQualified { get { return m_qual != Qual.None; } }

        private Qual m_qual;
        private enum Qual {
            None, Timestamp, Snapshot, Build, Other
        }

        public String MatchString { get; private set; }

        private Version() {
        }

        private void GenerateMatchString() {
            var sb = new StringBuilder();
            sb.Append(Major).Append('.');
            sb.Append(Minor).Append('.');
            sb.Append(Revision);
            if (IsQualified) {
                sb.Append("-").Append(Qualifier);
            } else {
                sb.Append("-0");
            }
            MatchString = sb.ToString();
        }

        private static int CheckInRange(int num, String name) {
            if (num < 0 ) {
                throw new ArgumentException(String.Format("Expect '{0}' to be > 0 but was {1}",name,num));
            }
            return num;
        }

        public static bool TryParse(String s, out Version version) {
            try {
                version = Parse(s);
                return true;
            } catch (Exception) {
            }
            version = null;
            return false;
        }

        public static Version Parse(String s) {
           try {
                var v = new Version();

                var versionQualifierParts = s.Split(Dashes);
                if (versionQualifierParts.Length == 0 || versionQualifierParts.Length > 2) {
                    throw NewInvalidFormat(s);
                }
                if (versionQualifierParts.Length == 2) {
                    var qualifierPart = versionQualifierParts[1].Trim();
                    if(String.IsNullOrEmpty(qualifierPart)){
                        throw NewInvalidFormat(s);
                    }
                    try {
                        SetQualifier(v,qualifierPart);
                    } catch (ArgumentException e) {
                        throw NewInvalidFormat(s,e);
                    }
                }
                var versionParts = versionQualifierParts[0].Split(Dots);
                if (versionParts.Length < 2 || versionParts.Length > 3) {
                    throw NewInvalidFormat(s);
                }
                v.Major = int.Parse(versionParts[0]);
                v.Minor = int.Parse(versionParts[1]);
                if (versionParts.Length == 3) {
                    v.Revision = int.Parse(versionParts[2]);
                }
                v.GenerateMatchString();
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

        public int CompareTo(Version other) {
            if (other == null) {
                return 1;
            }
            return MatchString.CompareTo(other.MatchString);
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

        public override bool Equals(Object other) {
            if (other == null || !(other is Version)) {
                return false;
            }
            return MatchString.Equals((other as Version).MatchString);
        }

        public override int GetHashCode() {
            return MatchString.GetHashCode();
        }
    }
}
