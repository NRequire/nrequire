using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {
    internal class ADependency : Dependency {

        internal static ADependency With() {
            return new ADependency();
        }

        internal ADependency Defaults() {
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

        new internal ADependency Version(String s) {
            base.Version = net.nrequire.Version.Parse(s);
            return this;
        }

        new internal ADependency Classifiers(String s) {
            base.Classifiers = s;
            return this;
        }
    }
}
