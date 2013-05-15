using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire {
    internal class NewDependency : Dependency {

        internal static NewDependency With() {
            return new NewDependency();
        }

        internal NewDependency Defaults() {
            Group = "Group";
            Name = "Name";
            Ext = "Ext";
            Arch = "Any";
            Runtime = "Any";
            return this;
        }

        internal List<Dependency> Versions(params String[] versions) {
            var list = new List<Dependency>();
            foreach (var s in versions) {
                var clone = Clone();
                clone.VersionString = s;
                list.Add(clone);
            }
            return list;
        }

        new internal NewDependency Version(String s) {
            base.Version = NRequire.Version.Parse(s);
            return this;
        }

        new internal NewDependency Classifiers(Classifiers c) {
            base.Classifiers = c;
            return this;
        }

        new internal NewDependency Classifiers(String s) {
            base.Classifiers = NRequire.Classifiers.Parse(s);
            return this;
        }
    }
}
