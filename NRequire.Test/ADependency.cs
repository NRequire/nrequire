using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRequire.Test;
using NRequire.Util;
using TestFirst.Net.Matcher;

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
            DepParser.Parse(fullString,
            (s) => Group(s),
            (s) => Name(s),
            (s) => Version(s),
            (s) => Ext(s),
            (s) => Classifiers(s));
        }
    }
}
