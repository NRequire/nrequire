using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using NRequire.Matcher;

namespace NRequire {
	public class VersionMatcher : IMatcher<Version> {

        private string m_versionString;
        private IMatcher<Version> m_match;
        
        private VersionMatcher(String versionString, IMatcher<Version> match) {
            m_match = match;
            m_versionString = versionString;
        }

        public static VersionMatcher Parse(String versionMatch) {
            try {
                return InternalParse(versionMatch);
            } catch (Exception e) {
                throw new ArgumentException(String.Format("Error trying to parse version criteria '{0}'", versionMatch),e);
            }
        }

        private static VersionMatcher InternalParse(String versionMatch){
            //http://docs.codehaus.org/display/MAVEN/Dependency+Mediation+and+Conflict+Resolution
            // [  -> >=
            // ( -> >
            // ] -> <=
            // ) -> <
            var from = 0;
            char lastLimiter = ',';
            var any = new AnyMatcher();

            RangeMatcher range = null;
            var getRangePair = new Func<RangeMatcher>(() => {
                if (range == null) {
                    range = new RangeMatcher();
                    any.Add(range);
                }
                return range;
            });
            var newRangePair = new Action(() => range = null);
            for (var i = 0; i < versionMatch.Length; i++) {
                var c = versionMatch[i];
                try {
                    if (c == '[' || c == '(') {
                        if (lastLimiter == '[' || lastLimiter == '(') {
                            throw new ArgumentException("Did not expect previous range limter to be " + lastLimiter);
                        }
                        lastLimiter = c;
                        from = i + 1;
                    } else if (c == ',') {
                        var part = versionMatch.Substring(from, i - from);
                        switch (lastLimiter) {
                            case '[':
                            case '('://...(1.2.3,....
                                getRangePair().From = ExactMatcher.Parse(lastLimiter, part);
                                break;
                            case ']':
                            case ')'://...),...
                                //end of the last one. Check no intervening chars??
                                if (from != i) {
                                    throw new ArgumentException("Did not expect additional since last " + lastLimiter);
                                }
                                break;
                            case ',': //...,1.2.3,...
                                if (range != null) {
                                    throw new ArgumentException("Do not expect " + c);
                                }
                                any.Add(ExactMatcher.Parse('=', part));
                                newRangePair();
                                break;
                            default:
                                break;
                        }
                        lastLimiter = ',';
                        from = i + 1;
                    } else if (c == ']' || c == ')') { ///...,1.2.3).... or ...(1.2.3)
                        var part = versionMatch.Substring(from, i - from);
                        if (lastLimiter == ',') {
                            if (from < i) { //only if it's not something like (1.2.3,) in which case we just ignore the last matcher
                                getRangePair().To = ExactMatcher.Parse(c, part);
                            }
                        } else if (lastLimiter == '[' || lastLimiter == '(') {
                            if (lastLimiter == '(' && c == ')') {
                                throw new ArgumentException("Nothing can match (x), use [x) or (x] or (x,y) as position " + i);
                            }
                            getRangePair().From = ExactMatcher.Parse(lastLimiter, part);
                            getRangePair().To = ExactMatcher.Parse(c, part);
                        } else {
                            throw new ArgumentException("Did not expect previous range limter to be " + lastLimiter);
                        }
                        lastLimiter = c;
                        from = i + 1;
                        newRangePair();
                    }
                } catch (ArgumentException e) {
                    throw new ArgumentException("Invalid version string at position " + i, e);
                }
            }
            if (from < versionMatch.Length) {
                any.Add(ExactMatcher.Parse('=',versionMatch.Substring(from,versionMatch.Length)));
            }
            return new VersionMatcher(versionMatch,any.Collapse());
        }

        public bool Match(String versionString) {
            return Match(Version.Parse(versionString));
        }

        public bool Match(Version v) {
            return m_match.Match(v);
        }

        public override String ToString() {
            return m_versionString;
        }

        public override int GetHashCode()
        {
          return m_versionString.GetHashCode();
        }

        public override bool Equals(Object other) {
          if( other == null || !(other is VersionMatcher) ){
            return false;
          }
          var o = other as VersionMatcher;
          return o.m_versionString.Equals(m_versionString);
        }
    }
}
