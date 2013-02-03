using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace net.nrequire {
    public class Dependency {

        private static readonly Dependency DefaultDependencyValues = new Dependency { Ext = "dll", Arch = "any", Runtime = "any" };
        
        private static readonly IDictionary<String,IList<String>> DefaultRelatedByExt = new Dictionary<String,IList<String>>{
            { "dll", new[]{ "xml", "pdb" }},
            { "exe", new[]{ "xml", "pdb" }}
        };

        public string Name { get; set; }
        public string GroupId { get; set; }
        public string ArtifactId { get; set; }
        public string Version { get; set; }
        public String Ext { get; set; }
        public string Arch { get; set; }
        public string Runtime { get; set; }
        public Uri Url { get; set; }
        public String CopyTo { get; set; }

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
        }

        public static Dependency DefaultDependency() {
            return DefaultDependencyValues.Clone();
        }

        public static IList<Dependency> MergeWithDefault(IEnumerable<Dependency> deps,Dependency defaultDep) {
            var merged = new List<Dependency>();
            foreach (var dep in deps) {
                merged.Add(dep.MergeWithDefault(defaultDep));
            }
            return merged;
        }

        public Dependency MergeWithDefault(Dependency defaultDep) {
            var d = MergeWithParent(defaultDep);
            if (!d.HasRelatedDependencies()) {
                IList<String> related;
                if(DefaultRelatedByExt.TryGetValue(d.Ext,out related)){
                    d.Related = new List<String>(related);
                }
            }
            return d;
        }

        public IList<Dependency> GetRelatedDependencies() {
            ValidateRequiredSet();
            var relatedDeps = new List<Dependency>();
            if (HasRelatedDependencies()) {
                foreach (var ext in Related) {
                    var d = CloneSimpleProps();
                    d.Ext = ext;
                    relatedDeps.Add(d);
                }
            }
            return relatedDeps;
        }

        public bool HasRelatedDependencies() {
            return Related != null && Related.Count > 0;
        }

        public Dependency MergeWithParent(Dependency parent) {
            var d = Clone();
            d.FillInBlanksFrom(parent);
            return d;
        }

        public Dependency Clone() {
            var d = new Dependency();
            d.FillInBlanksFrom(this);
            return d;
        }

        private Dependency CloneSimpleProps() {
            var d = new Dependency();
            d.FillInSimpleBlanksFrom(this);
            return d;
        }

        private void FillInBlanksFrom(Dependency d) {
            if (d == null) {
                return;
            }
            FillInSimpleBlanksFrom(d);
            this.Dependencies = MergeDependencies(d.Dependencies, this.Dependencies);
        }

        private void FillInSimpleBlanksFrom(Dependency d) {
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
        }

        private static IList<Dependency> MergeDependencies(IList<Dependency> parent, IList<Dependency> child) {
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
                    merged[key] = childDep.Clone().MergeWithParent(parentDep);
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
            return String.Format("Dependency@{0}<GroupId:{1},ArtifactId:{2},Version:{3},Ext:{4},Arch:{5},Runtime:{6},Url:'{7}',CopyTo:'{8}',Related:[{9}],Dependencies:[{10}]>", 
                base.GetHashCode(),
                GroupId,
                ArtifactId,
                Version,
                Ext,
                Arch,
                Runtime,
                Url,
                CopyTo,
                String.Join(",",Related),
                depsString
            );
        }
    }
}
