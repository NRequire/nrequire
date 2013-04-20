using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {
    internal class ADependencyWish : DependencyWish {

        internal static ADependencyWish With() {
            return new ADependencyWish();
        }

        internal ADependencyWish Defaults() {
            Group = "Group";
            Name = "Name";
            Ext = "Ext";
            Arch = "Any";
            Runtime = "Any";
            return this;
        }

        new internal ADependencyWish Version(String s) {
            base.Version = VersionMatcher.Parse(s);
            return this;
        }

        new internal ADependencyWish Classifiers(String s) {
            base.ClassifiersString = s;
            return this;
        }
    }
}
