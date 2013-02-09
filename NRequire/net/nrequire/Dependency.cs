using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace net.nrequire {
    public class Dependency {
        //groupId->group
        //Artifactid->Name
        public string Group { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public String Ext { get; set; }
        
        //to be moved into classifiers
        public string Arch { get; set; }
        public string Runtime { get; set; }

        public Uri Url { get; set; }
        public String CopyTo { get; set; }
        //scratch and only put in resolved? deps should come under projects dep lilsts of compile:[],provided:[],files:[],transitive;[]
        internal Scopes? Scope { get; set; }
        //remove? see scopes above
        public bool? Optional { get; set; }//this dep is optional
        public bool? Force { get; set; }//force this dep
        public bool? Transitive { get; set; }//whether to resolve transitive deps for this or not

        internal int Depth { get; set; }//0 is top level, 1 is child, 2 is child of child....

        public String Classifier { 
            get{ return ClassifiersAsString(); } 
            set{ Classifiers = ParseClassifierString(value);} 
        }
        public IDictionary<String, String> Classifiers { get; set; }

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
            this.Classifiers = new Dictionary<String,String>();
            Optional = false;
        }

        public bool HasOptions() {
            return Classifiers != null && Classifiers.Count > 0;
        }

        private IDictionary<String, String> ParseClassifierString(String s) {
            var opts = new Dictionary<String, String>();
            var parts = s.Split(new char[]{'_'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts) {
                var pair = part.Split(new char[] { '-' });
                if (pair.Length == 1) {
                    opts[pair[0]] = "true";
                } else if (pair.Length == 2) {
                    opts[pair[0]] = pair[1];
                } else {
                    throw new ArgumentException(String.Format("Error parsing part '{0}' in options string'{1}' expected name-value pair",part,s));
                }
            }
            return opts;
        }

        public String ClassifiersAsString() {
            if (HasOptions()) {
                var keys = new List<String>(Classifiers.Keys);
                keys.Sort();
                var sb = new StringBuilder();
                foreach (var key in keys) {
                    var val = Classifiers[key];
                    if (val == "true") {
                        if (sb.Length > 0) {
                            sb.Append("_");
                        }
                        sb.Append(key);
                    } else if (val == "false") {
                        //bool option and it doesn'texist, don't include modifier
                    } else {
                        if (sb.Length > 0) {
                            sb.Append("_");
                        } 
                        sb.Append(key).Append("-").Append(val);
                    }
                }
                return sb.ToString();
            }
            return null;
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
            if (Dependencies != null && Dependencies.Count > 0) {
                depsString = "\n\t" + String.Join("\n\t", Dependencies) + "\n\t";
            }
            return String.Format("Dependency@{0}<\n\tGroup:{1},\n\tName:{2},\n\tVersion:{3},\n\tExt:{4},\n\tArch:{5},\n\tRuntime:{6},\n\tScope:{7},\n\tUrl:'{8}',\n\tCopyTo:'{9}',\n\tRelated:[{10}],\n\tDependencies:[{11}]\n>", 
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
                Related==null?null:String.Join(",",Related),
                depsString
            );
        }
    }
}
