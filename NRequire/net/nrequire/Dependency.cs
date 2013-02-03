using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace net.nrequire {
    public class Dependency {

        public string Name { get; set; }
        public string GroupId { get; set; }
        public string ArtifactId { get; set; }
        public string Version { get; set; }
        public String Ext { get; set; }
        public string Arch { get; set; }
        public string Runtime { get; set; }
        public Uri Url { get; set; }
        public String CopyTo { get; set; }
        public Scopes? Scope { get; set; }
        public bool Optional { get; set; }

        public IList<Dependency> Dependencies { get; set; }
        /// <summary>
        /// Dependencies which differ only in the extension which should be considered
        /// related to this dependency. For example dll deps usually have xml and pdb 
        /// files associated with with and these should be considered part of the dll
        /// event though they can be considered resources/deps in their own right
        /// </summary>
        public IList<String> Related { get; set; }

        public Dependency() {
            this.Dependencies = new List<Dependency>();
            Optional = false;
        }

        public bool HasRelatedDependencies() {
            return Related != null && Related.Count > 0;
        }

        public static IList<Dependency> FillInBlanksFrom(IEnumerable<Dependency> deps, Dependency defaultDep) {
            var merged = new List<Dependency>();
            foreach (var dep in deps) {
                merged.Add(dep.FillInBlanksFrom(defaultDep));
            }
            return merged;
        }

        public Dependency FillInBlanksFrom(Dependency parent) {
            var d = Clone();
            d.InternalFillInBlanksFrom(parent);
            return d;
        }

        public Dependency Clone() {
            var d = new Dependency();
            d.InternalFillInBlanksFrom(this);
            return d;
        }

        private void InternalFillInBlanksFrom(Dependency d) {
            if (d == null) {
                return;
            }
            InternalFillInSimpleBlanksFrom(d);
            this.Dependencies = MergeLists(d.Dependencies, this.Dependencies);
        }

        private void InternalFillInSimpleBlanksFrom(Dependency d) {
            if (d == null) {
                return;
            }
            if (String.IsNullOrWhiteSpace(this.Arch)) {
                this.Arch = d.Arch;
            }
            if (String.IsNullOrWhiteSpace(this.ArtifactId)) {
                this.ArtifactId = d.ArtifactId;
            }
            if (String.IsNullOrWhiteSpace(this.Ext)) {
                this.Ext = d.Ext;
            }
            if (String.IsNullOrWhiteSpace(this.Runtime)) {
                this.Runtime = d.Runtime;
            }
            if (String.IsNullOrWhiteSpace(this.GroupId)) {
                this.GroupId = d.GroupId;
            }
            if (String.IsNullOrWhiteSpace(this.Name)) {
                this.Name = d.Name;
            }
            if (this.Ext == null) {
                this.Ext = d.Ext;
            }
            if (this.Url == null) {
                this.Url = d.Url;
            }
            if (String.IsNullOrWhiteSpace(this.CopyTo)) {
                this.CopyTo = d.CopyTo;
            }
            if (this.Related == null || Related.Count == 0) {
                this.Related = d.Related;
            }
            if (String.IsNullOrWhiteSpace(this.Version)) {
                this.Version = d.Version;
            }
            if (this.Scope == null) {
                this.Scope = d.Scope;
            }
        }

        private static IList<Dependency> MergeLists(IList<Dependency> parent, IList<Dependency> child) {
            if (parent == null && child == null) {
                return new List<Dependency>();
            }
            if (parent == null  || parent.Count == 0) {
                return new List<Dependency>(child);
            }
            if (child == null || child.Count == 0) {
                return new List<Dependency>(parent);
            }

            var merged = parent.ToDictionary(d=>d.Signature());
            foreach (var childDep in child) {
                var key = childDep.Signature();
                if (merged.ContainsKey(key)) {
                    var parentDep = merged[key];
                    merged.Remove(key);
                    merged[key] = childDep.Clone().FillInBlanksFrom(parentDep);
                } else {
                    merged[key] = childDep;
                }
            }
            
            return new List<Dependency>(merged.Values);
        }

        public void ValidateRequiredSet() {
            ValidateMergeValuesSet();
            if (String.IsNullOrWhiteSpace(Version)) {
                throw new ArgumentException("Expect Version to be set on " + this);
            }
            if (String.IsNullOrWhiteSpace(Ext)) {
                throw new ArgumentException("Expect Extension to be set on " + this);
            }
            if (Scope==null) {
                throw new ArgumentException("Expect Scope to be set on " + this);
            }
        }

        public void ValidateMergeValuesSet() {
            if (String.IsNullOrWhiteSpace(GroupId)) {
                throw new ArgumentException("Expect GroupId to be set on " + this);
            }
            if (String.IsNullOrWhiteSpace(ArtifactId)) {
                throw new ArgumentException("Expect ArtifactId to be set on " + this);
            }
            if (String.IsNullOrWhiteSpace(Arch)) {
                throw new ArgumentException("Expect Arch to be set on " + this);
            }
            if (String.IsNullOrWhiteSpace(Runtime)) {
                throw new ArgumentException("Expect Runtime to be set on " + this);
            }
        }

        public string Signature() {
            return String.Format("groupId-{0}-artifactId-{1}-arch-{2}-runtime-{3}>",
                GroupId,
                ArtifactId,
                Arch,
                Runtime
            ).ToLower();
        }

        public override string ToString() {
            var depsString = "";
            if (Dependencies != null && Dependencies.Count > 0) {
                depsString = "\n\t" + String.Join("\n\t", Dependencies) + "\n\t";
            }
            return String.Format("Dependency@{0}<\n\tGroupId:{1},\n\tArtifactId:{2},\n\tVersion:{3},\n\tExt:{4},\n\tArch:{5},\n\tRuntime:{6},\n\tScope:{7},\n\tUrl:'{8}',\n\tCopyTo:'{9}',\n\tRelated:[{10}],\n\tDependencies:[{11}]\n>", 
                base.GetHashCode(),
                GroupId,
                ArtifactId,
                Version,
                Ext,
                Arch,
                Runtime,
                Scope,
                Url,
                CopyTo,
                Related==null?null:String.Join(",",Related),
                depsString
            );
        }
    }
}
