﻿using System;
using NRequire.Util;

namespace NRequire
{
    public partial class ADependency {
        
        /// <summary>
        /// Parse from  group:name:version:ext:classifiers
        /// </summary>
        /// <param name="fullString">Full string.</param>
        public static ADependency From(String fullString) {
            var d = new ADependency();
            d.SetAllFromParse(fullString);
            return d;
        }

        protected void SetAllFromParse(String fullString) {
            PartsParser.ParseParts(fullString,
            (s) => Group(s),
            (s) => Name(s),
            (s) => Version(s),
            (s) => Ext(s),
            (s) => Classifiers(s));
        }
    }
}
