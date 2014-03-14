using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRequire.Test;

namespace NRequire {
    public class NewDependency : Dependency {

        public static NewDependency With() {
            return new NewDependency();
        }

        /// <summary>
        /// Parse from  group:name:version:ext:classifiers
        /// </summary>
        public static new NewDependency Parse(String fullString) {
            var dep = new NewDependency();
            dep.Defaults();
            dep.SetAllFromParse(fullString);
            return dep;
        }

        public NewDependency Defaults() {
            TestDefaults.Apply(this);
            return this;
        }

        public List<Dependency> Versions(params String[] versions) {
            var list = new List<Dependency>();
            foreach (var s in versions) {
                var clone = Clone();
                clone.VersionString = s;
                list.Add(clone);
            }
            return list;
        }

        public new NewDependency Version(String val) {
            base.Version = NRequire.Version.Parse(val);
            return this;
        }

        public new NewDependency Group(String val) {
            base.Group = val;
            return this;
        }

        public new NewDependency Name(String val) {
            base.Name = val;
            return this;
        }

        public new NewDependency Arch(String val) {
            base.Arch = val;
            return this;
        }

        public new NewDependency Runtime(String val) {
            base.Runtime = val;
            return this;
        }

        public new NewDependency Ext(String val) {
            base.Ext = val;
            return this;
        }

        public new NewDependency Classifiers(Classifiers c) {
            base.Classifiers = c;
            return this;
        }

        public new NewDependency Classifiers(String s) {
            base.Classifiers = NRequire.Classifiers.Parse(s);
            return this;
        }

        public new NewDependency Source(ISource location) {
            SourceLocations.AddToSourceLocations(this, location);
            return this;
        }

    }
}
