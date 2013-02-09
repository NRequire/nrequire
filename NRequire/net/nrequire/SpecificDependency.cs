using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {

    public class SpecificDependency {

        public string Name { get; internal set; }
        public string Group { get; internal set; }
        public Version Version { get; internal set; }
        public String Ext { get; internal set; }
        public string Arch { get; internal set; }
        public string Runtime { get; internal set; }
        public Uri Url { get; internal set; }
        public String CopyTo { get; internal set; }
        public Scopes Scope { get; internal set; }
        public bool HasRelatedDependencies { get { return Related != null && Related.Count > 0;}}
        public IList<SpecificDependency> Related { get; internal set; }
        
        public bool EmbeddedResource { get; set; }

        public SpecificDependency Clone() {
            return new SpecificDependency {
                Arch = Arch,
                CopyTo = CopyTo,
                Ext = Ext,
                Group = Group,
                Name = Name,
                Related = Related==null?new List<SpecificDependency>():new List<SpecificDependency>(Related),
                Runtime = Runtime,
                Scope = Scope,
                Url = Url,
                Version = Version
            };
        }

        public override string ToString() {
            return String.Format("SpecificDependency@{0}<\n\tGroup:{1},\n\tName:{2},\n\tVersion:{3},\n\tExt:{4},\n\tArch:{5},\n\tRuntime:{6},\n\tScope:{7},\n\tUrl:'{8}',\n\tCopyTo:'{9}',\n\tRelated:[{10}]'\n>",
                base.GetHashCode(),
                Group,
                Name,
                Version,
                Ext,
                Arch,
                Runtime,
                Scope,
                Url,
                CopyTo,
                String.Join(",",Related)
            );
        }
    }
}
