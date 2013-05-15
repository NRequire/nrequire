using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire {

	/// <summary>
	/// A concrete fixed dependency
	/// </summary>
    public class Dependency : AbstractDependency {

        internal String VersionString {
            get { return Version == null ? null : Version.ToString(); }
            set { Version = value == null ? null : Version.Parse(value); }
        }
        public Version Version { get; internal set; }
        //public string Classifiers { get; internal set; }
        public Scopes Scope { get; internal set; }
        public bool HasRelatedDependencies { get { return Related != null && Related.Count > 0;}}
        public IList<Dependency> Related { get; internal set; }
        
        public bool EmbeddedResource { get; set; }

        public Dependency Clone() {
            return new Dependency {
                Arch = Arch,
                CopyTo = CopyTo,
                Classifiers = Classifiers,
                Ext = Ext,
                Group = Group,
                Name = Name,
                Related = Related==null?new List<Dependency>():new List<Dependency>(Related),
                Runtime = Runtime,
                Scope = Scope,
                Url = Url,
                Version = Version
            };
        }

        public override string ToString() {
            return String.Format("Dependency@{0}<\n\tGroup:{1},\n\tName:{2},\n\tVersion:{3},\n\tExt:{4},\n\tArch:{5},\n\tRuntime:{6},\n\tClassifiers:{7},\n\tScope:{8},\n\tUrl:{9},\n\tCopyTo:{10},\n\tRelated:[{11}]'\n>",
                base.GetHashCode(),
                Group,
                Name,
                Version,
                Ext,
                Arch,
                Runtime,
                Classifiers,
                Scope,
                Url,
                CopyTo,
                Related==null?null:String.Join(",",Related)
            );
        }
    }
}
