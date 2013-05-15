using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire {
    internal class NewDependencyWish : DependencyWish {

        internal static NewDependencyWish With() {
            return new NewDependencyWish();
        }

        internal NewDependencyWish Defaults() {
            Group = "Group";
            Name = "Name";
            Ext = "Ext";
            Arch = "Any";
            Runtime = "Any";
            return this;
        }

        new internal NewDependencyWish Version(String s) {
            base.Version = VersionMatcher.Parse(s);
            return this;
        }

        new internal NewDependencyWish Classifiers(String s) {
            base.ClassifiersString = s;
            return this;
        }
    }
}
