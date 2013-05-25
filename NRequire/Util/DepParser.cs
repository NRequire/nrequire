using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Util {
    public static class DepParser {

        public static void Parse(String s, params Action<String>[] setters) {
            if (s == null) {
                return;
            }
            var parts = s.Split(new char[] { ':' });
            if (parts.Length > setters.Length) {
                throw new ArgumentException("expected at most " + setters.Length + " parts but got " + parts.Length);
            }
            for (var i = 0; i < parts.Length && i < setters.Length; i++) {
                var val = parts[i].Trim();
                if (val.Length > 0) {
                    setters[i].Invoke(val);
                }
            }
        }
    }
}
