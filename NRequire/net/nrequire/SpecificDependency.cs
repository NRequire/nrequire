using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {

    public class SpecificDependency {

        public string Name { get; internal set; }
        public string GroupId { get; internal set; }
        public string ArtifactId { get; internal set; }
        public Version Version { get; internal set; }
        public String Ext { get; internal set; }
        public string Arch { get; internal set; }
        public string Runtime { get; internal set; }
        public Uri Url { get; internal set; }
        public String CopyTo { get; internal set; }
        public IList<SpecificDependency> Related { get; internal set; }

        public IList<SpecificDependency> GetClonesWithExtensions(IList<String> extensions) {
            var clones = new List<SpecificDependency>();
            if (extensions != null && extensions.Count > 0) {
                foreach (var ext in extensions) {
                    var d = Clone();
                    d.Ext = ext;
                    clones.Add(d);
                }
            }
            return clones;
        }

        private SpecificDependency Clone() {
            return new SpecificDependency {
                Arch = Arch,
                ArtifactId = ArtifactId,
                CopyTo = CopyTo,
                Ext = Ext,
                GroupId = GroupId,
                Name = Name,
                Related = Related==null?new List<SpecificDependency>():new List<SpecificDependency>(Related),
                Runtime = Runtime,
                Url = Url,
                Version = Version
            };
        }

        public bool HasRelatedDependencies() {
            return Related != null && Related.Count > 0;
        }

        public override string ToString() {
            return String.Format("SpecificDependency@{0}<GroupId:{1},ArtifactId:{2},Version:{3},Ext:{4},Arch:{5},Runtime:{6},Url:'{7}',CopyTo:'{8}',Related:[{9}]'>",
                base.GetHashCode(),
                GroupId,
                ArtifactId,
                Version,
                Ext,
                Arch,
                Runtime,
                Url,
                CopyTo,
                String.Join(",",Related)
            );
        }
    }
}
