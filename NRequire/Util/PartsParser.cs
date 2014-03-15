using System;

namespace NRequire.Util {
    public static class PartsParser {

        /// <summary>
        /// Parse a string of the form a:b:c:d, invoking the callback at th same index as the value
        /// </summary>
        /// <param name="s"></param>
        /// <param name="setters"></param>
        public static void ParseParts(String s, params Action<String>[] setters) {
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
