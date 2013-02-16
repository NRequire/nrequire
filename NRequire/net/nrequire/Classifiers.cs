using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {
    public class Classifiers {
        private readonly IDictionary<String, String> m_classifiers;

        public int Count { get { return m_classifiers.Count; } }

        public static Classifiers FromDict(IDictionary<String, String> classifiers) {
            return new Classifiers(classifiers);
        }

        public Classifiers():this(new Dictionary<string,string>()){

        }
        private Classifiers(IDictionary<String, String> classifiers) {
            m_classifiers = classifiers;
        }

        public Classifiers Clone() {
            var dictClone = new Dictionary<String,String>(m_classifiers);
            return new Classifiers(dictClone);
        }

        public IDictionary<String, String> ToDict() {
            return new Dictionary<String, String>(m_classifiers);
        }

        public Classifiers SetAll(IDictionary<String, String> dict) {
            foreach (var key in dict.Keys) {
                Set(key, dict[key]);
            }
            return this;
        }
        
        public Classifiers Set(String key, String val) {
            if (m_classifiers.ContainsKey(key)) {
                m_classifiers.Remove(key);
            }
            m_classifiers.Add(key, val);
            return this;
        }

        public String Get(String key) {
            if (m_classifiers.ContainsKey(key)) {
                return m_classifiers[key];
            }
            return null;
        }
        
        public bool ContainsKey(String key) {
            return m_classifiers.ContainsKey(key);
        }

        public static bool TryParse(String s, out Classifiers c) {
            try {
                c = Parse(s);
            } catch (Exception) {
            }
            c = null;
            return false;
        }

        public static Classifiers Parse(Classifiers existing,String s) {
            var dict = ParseIntoDict(s);
            existing.SetAll(dict);
            return existing;
        }

        public static Classifiers Parse(String s) {
            return new Classifiers(ParseIntoDict(s));
        }

        private static IDictionary<String, String> ParseIntoDict(String s) {
            var opts = new Dictionary<String, String>();
            var parts = s.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts) {
                var pair = part.Split(new char[] { '-' });
                if (pair.Length == 1) {
                    opts[pair[0]] = "true";
                } else if (pair.Length == 2) {
                    opts[pair[0]] = pair[1];
                } else {
                    throw new ArgumentException(String.Format("Error parsing part '{0}' in options string'{1}' expected name-value pair", part, s));
                }
            }
            return opts;
        }

        public override string ToString() {
            return DictToString(m_classifiers);
        }

        private static String DictToString(IDictionary<String, String> classifiers) {
            if (classifiers != null && classifiers.Count > 0) {
                var keys = new List<String>(classifiers.Keys);
                keys.Sort();
                var sb = new StringBuilder();
                foreach (var key in keys) {
                    var val = classifiers[key];
                    if (val == "true") {
                        if (sb.Length > 0) {
                            sb.Append("_");
                        }
                        sb.Append(key);
                    } else if (val == "false") {
                        //bool option and it doesn'texist, don't include modifier
                    } else {
                        if (sb.Length > 0) {
                            sb.Append("_");
                        }
                        sb.Append(key).Append("-").Append(val);
                    }
                }
                return sb.ToString();
            }
            return null;
        }
    }
}
