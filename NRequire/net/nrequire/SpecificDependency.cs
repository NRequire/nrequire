using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {

    public class SpecificDependency {
        public string Name { get; set; }
        public string GroupId { get; set; }
        public string ArtifactId { get; set; }
        public string Version { get; set; }
        public String Ext { get; set; }
        public string Arch { get; set; }
        public string Runtime { get; set; }
        public Uri Url { get; set; }
        public String CopyTo { get; set; }
        public IList<String> Related { get; set; }

        public void ValidateRequiredSet() {
            if (String.IsNullOrWhiteSpace(GroupId)) {
                throw new ArgumentException("Expect GroupId to be set on " + this);
            }
            if (String.IsNullOrWhiteSpace(ArtifactId)) {
                throw new ArgumentException("Expect ArtifactId to be set on " + this);
            }
            if (String.IsNullOrWhiteSpace(Version)) {
                throw new ArgumentException("Expect Version to be set on " + this);
            }
            if (String.IsNullOrWhiteSpace(Ext)) {
                throw new ArgumentException("Expect Extension to be set on " + this);
            }
            if (String.IsNullOrWhiteSpace(Arch)) {
                throw new ArgumentException("Expect Arch to be set on " + this);
            }
            if (String.IsNullOrWhiteSpace(Runtime)) {
                throw new ArgumentException("Expect Runtime to be set on " + this);
            }
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
