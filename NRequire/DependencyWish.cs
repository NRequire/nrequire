using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using NRequire.Json;
namespace NRequire {

    public class DependencyWish : AbstractDependency {

        [JsonConverter(typeof(VersionConverter))]
        public VersionMatcher Version { get; set; }

        [JsonIgnore]
        internal String VersionString {
            get { return Version == null ? null : Version.ToString(); }
            set { Version = value == null? null : VersionMatcher.Parse(value); } 
        } 
      
        //scratch and only put in resolved? deps should come under projects dep lilsts of compile:[],provided:[],files:[],transitive;[]
        internal Scopes? Scope { get; set; }
        //remove? see scopes above
        public bool? Optional { get; set; }//this dep is optional
        public bool? Force { get; set; }//force this dep
        public bool? ResolveTransitive { get; set; }//whether to resolve transitive deps for this or not

        internal int Depth { get; set; }//0 is top level, 1 is child, 2 is child of child....

        [JsonConverter(typeof(ClassifierConverter))]
        public Classifiers Classifiers { get; set; }

        [JsonIgnore]
        internal String ClassifiersString {
            get { return Classifiers == null ? null : Classifiers.ToString(); }
            set { Classifiers = value == null ? null : Classifiers.Parse(value); }
        } 

        public IList<DependencyWish> Transitive { get; set; }
      
        /// <summary>
        /// Dependencies which differ only in the extension which should be considered
        /// related to this dependency. For example dll deps usually have xml and pdb 
        /// files associated with with and these should be considered part of the dll
        /// event though they can be considered resources/deps in their own right
        /// </summary>
        public IList<String> Related { get; set; }

        public DependencyWish() {
            this.Transitive = new List<DependencyWish>();
            this.Classifiers = new Classifiers();
            Optional = false;
        }

        public void AfterLoad() {
            //any post load validation or setup
        }

        public bool HasClassifiers() {
            return Classifiers != null && Classifiers.Count > 0;
        }

        public bool HasRelatedDependencies() {
            return Related != null && Related.Count > 0;
        }

        public static IList<DependencyWish> FillInBlanksFrom(IEnumerable<DependencyWish> deps, DependencyWish defaultDep) {
            var merged = new List<DependencyWish>();
            foreach (var dep in deps) {
                merged.Add(dep.FillInBlanksFrom(defaultDep));
            }
            return merged;
        }

        public DependencyWish FillInBlanksFrom(DependencyWish parent) {
            var d = Clone();
            d.InternalFillInBlanksFrom(parent);
            return d;
        }

        public DependencyWish Clone() {
            var d = new DependencyWish();
            d.InternalFillInBlanksFrom(this);
            return d;
        }

        private void InternalFillInBlanksFrom(DependencyWish d) {
            if (d == null) {
                return;
            }
            InternalFillInSimpleBlanksFrom(d);
            this.Transitive = MergeLists(d.Transitive, this.Transitive);
        }

        private void InternalFillInSimpleBlanksFrom(DependencyWish d) {
            if (d == null) {
                return;
            }
            if (String.IsNullOrWhiteSpace(this.Group)) {
                this.Group = d.Group;
            }
            if (String.IsNullOrWhiteSpace(this.Name)) {
                this.Name = d.Name;
            } 
            if (String.IsNullOrWhiteSpace(this.Arch)) {
                this.Arch = d.Arch;
            }
            if (String.IsNullOrWhiteSpace(this.Ext)) {
                this.Ext = d.Ext;
            }
            if (String.IsNullOrWhiteSpace(this.Runtime)) {
                this.Runtime = d.Runtime;
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
            if (this.Version == null) {
                this.Version = d.Version;
            }
            if (this.Scope == null) {
                this.Scope = d.Scope;
            }
        }

        private static IList<DependencyWish> MergeLists(IList<DependencyWish> parent, IList<DependencyWish> child) {
            if (parent == null && child == null) {
                return new List<DependencyWish>();
            }
            if (parent == null  || parent.Count == 0) {
                return new List<DependencyWish>(child);
            }
            if (child == null || child.Count == 0) {
                return new List<DependencyWish>(parent);
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
            
            return new List<DependencyWish>(merged.Values);
        }

        public void ValidateRequiredSet() {
            ValidateMergeValuesSet();
            if (Version==null) {
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
            if (String.IsNullOrWhiteSpace(Group)) {
                throw new ArgumentException("Expect Group to be set on " + this);
            }
            if (String.IsNullOrWhiteSpace(Name)) {
                throw new ArgumentException("Expect Name to be set on " + this);
            }
            //if (String.IsNullOrWhiteSpace(Arch)) {
            //    throw new ArgumentException("Expect Arch to be set on " + this);
            //}
            //if (String.IsNullOrWhiteSpace(Runtime)) {
            //    throw new ArgumentException("Expect Runtime to be set on " + this);
            //}
        }

        public string Signature() {
            return String.Format("group-{0}-name-{1}>"
                , Group
                , Name
                //, Arch
                //, Runtime
            ).ToLower();
        }

        public override string ToString() {
            var depsString = "";
            if (Transitive != null && Transitive.Count > 0) {
                depsString = "\n\t" + String.Join("\n\t", Transitive) + "\n\t";
            }
            return String.Format("Dependency@{0}<\n\tGroup:{1},\n\tName:{2},\n\tVersion:{3},\n\tExt:{4},\n\tArch:{5},\n\tRuntime:{6},\n\tClassifiers:{7},\n\tScope:{8},\n\tUrl:{9},\n\tCopyTo:{10},\n\tRelated:[{11}],\n\tDependencies:[{12}]\n>", 
                base.GetHashCode(),
                Group,
                Name,
                VersionString,
                Ext,
                Arch,
                Runtime,
                Classifiers,
                Scope,
                Url,
                CopyTo,
                Related==null?null:String.Join(",",Related),
                depsString
            );
        }
    }
}
