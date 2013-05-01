using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace NRequire {

    //goto to major.minor.revision.build|timestamp|SNAPSHOT|qualifier to simplify parsing (like in gradle)
    public class Version :IComparable<Version> {
        public static String Format = "Major.Minor.Revision?.(SNAPSHOT|<Timestamp>|<Build>|<Qualifier>)?";

        private static readonly String[] DateFormats = new[] { "yyyyMMddHHmmssfff", "yyyyMMddHHmmss", "yyyy:MMdd:HHmm:ss", "yyyy:MMdd:HHmm:ss:fff"};
        private static readonly char[] Dots = new[] { '.' };
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
                sb.Append(".").Append(Qualifier);
            } else {
                sb.Append(".0");
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

                var parts = s.Trim().Split(Dots);
                if (parts.Length < 2 || parts.Length > 4) {
                    throw NewInvalidFormat(s);
                }
                
                v.Major = ParseInt(parts[0]);

                if (parts.Length > 3) {
                    v.Minor = ParseInt(parts[1]);
                    v.Revision = ParseInt(parts[2]);
                    var qualifierPart = parts[3];
                    SetQualifier(v, qualifierPart);
                } else if (parts.Length > 2) {
                    v.Minor = ParseInt(parts[1]);
                    if (IsVersionNum(parts[2])) {
                        v.Revision = ParseInt(parts[2]);
                    } else {
                        var qualifierPart = parts[2];
                        SetQualifier(v, qualifierPart);
                    }
                } else if (parts.Length > 1) {
                    if (IsVersionNum(parts[1])) {
                        v.Minor = ParseInt(parts[1]);
                    } else {
                        var qualifierPart = parts[1];
                        SetQualifier(v, qualifierPart);
                    }
                }
                v.GenerateMatchString();
                return v;
           } catch (Exception e) {
               throw NewInvalidFormat(s, e);
           }
        }

        private static bool IsVersionNum(string s) {
            int i;
            if (int.TryParse(s, out i)) {
                if (i < 0) {
                    throw new ArgumentException("Value must be positive but was " + s);
                }
                return true;
            }
            return false;
        }

        private static int ParseInt(string s) {
            int i;
            if (int.TryParse(s, out i)) {
                if (i < 0 || s[0] == '-') {
                    throw new ArgumentException(String.Format("Value must be positive but was '{0}'",s));
                }
                return i;
            }
            throw new ArgumentException(String.Format("Could not parse '{0}' as an int",s));
        }
        
        private static void SetQualifier(Version v, string s) {
            if (String.IsNullOrEmpty(s)) {
                throw NewInvalidFormat(s);
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
            return new ArgumentException(String.Format("Invalid version string '{0}', expected format is {1}", s, Format));
        }

        private static ArgumentException NewInvalidFormat(String s, Exception e) {
            return new ArgumentException(String.Format("Invalid version string '{0}', expected format is {1}", s, Format), e);
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
                sb.Append(".").Append(Qualifier);
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
