using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using NRequire.Json;
using NRequire.Util;

namespace NRequire {

    /// <summary>
    /// A wish for a dependency which matches the given criteria
    /// </summary>
    public class Wish : AbstractDependency,IRequireLoadNotification, IResolvable {

        [JsonConverter(typeof(VersionMatcherConverter))]
        public VersionMatcher Version { get; set; }

        [JsonIgnore]
        internal String VersionString {
            get { return Version == null ? null : Version.ToString(); }
            set { Version = value == null? null : VersionMatcher.Parse(value); } 
        } 

        [JsonIgnore]
        internal String ClassifiersString {
            get { return Classifiers == null ? null : Classifiers.ToString(); }
            set { Classifiers = value == null ? null : Classifiers.Parse(value); }
        }
        /// <summary>
        /// If set then these constraints are included when resolving dependencies. Allows for excluding deps which
        /// cause issues. Instead of adding to a project as a transitive, add it to the wish to make it clear
        /// where the issue is coming from
        /// </summary>
        public List<Wish> TransitiveWishes { get; set;  }

        public String CopyTo { get; set; }

        /// <summary>
        /// Create a new wish set to require the exact given version
        /// </summary>
        public Wish(Dependency dep) :this() {
            ((AbstractDependency)dep).Clone(this);
            Version = VersionMatcher.Parse(dep.Version.ToString());
        }

        public Wish() {
            TransitiveWishes = new List<Wish>();
        }

        /// <summary>
        /// Parse from  group:name:version:ext:classifiers:scope
        /// </summary>
        /// <param name="fullString">Full string.</param>
        public static Wish Parse(String fullString) {
            var wish = new Wish();
            wish.SetAllFromParse(fullString);
            return wish;
        }

        protected void SetAllFromParse(String fullString) {

            DepParser.Parse(fullString,
                (s) => Group = s,
                (s) => Name = s,
                (s) => Version = VersionMatcher.Parse(s),
                (s) => Ext = s,
                (s) => Classifiers = Classifiers.Parse(s),
                (s) => Scope = (Scopes)Enum.Parse(typeof(Scopes),s,true))
                ;
        }

        private static String CleanOrValue(String v, String defaultVal) {
            if( v == null){
                return defaultVal;
            }
            v = v.Trim();
            if (v.Length == 0) {
                return defaultVal;
            }
            return v;
        }

        public void AfterLoad() {
            //any post load validation or setup
            TransitiveWishes.ForEach(w=>w.Scope = Scopes.Transitive);
        }

        public bool HasClassifiers() {
            return Classifiers != null && Classifiers.Count > 0;
        }

        public Wish Clone() {
            var clone = new Wish();
            Clone(clone);
            clone.Version = Version;
            clone.Scope = Scope;
            clone.CopyTo = CopyTo;
            clone.TransitiveWishes = TransitiveWishes==null?new List<Wish>():new List<Wish>(TransitiveWishes);
            return clone;
        }

        public static List<Wish> CloneAndFillInBlanksFrom(IEnumerable<Wish> deps, Wish defaultDep) {
            var merged = new List<Wish>();
            foreach (var dep in deps) {
                merged.Add(dep.CloneAndFillInBlanksFrom(defaultDep));
            }
            return merged;
        }

        public Wish CloneAndFillInBlanksFrom(Wish parent) {
            var clone = Clone();
            clone.InternalFillInBlanksFrom(parent);
            return clone;
        }

        private void InternalFillInBlanksFrom(Wish wish) {
            if (wish == null) {
                return;
            }
            InternalFillInSimpleBlanksFrom(wish);
        }

        private void InternalFillInSimpleBlanksFrom(Wish fromWish) {
            if (fromWish == null) {
                return;
            }
            var modified = false;
            if (String.IsNullOrWhiteSpace(this.Group) && !String.IsNullOrWhiteSpace(fromWish.Group)) {
                this.Group = fromWish.Group;
                modified = true;
            }
            if (String.IsNullOrWhiteSpace(this.Name) && !String.IsNullOrWhiteSpace(fromWish.Name)) {
                this.Name = fromWish.Name;
                modified = true;
            }
            if (String.IsNullOrWhiteSpace(this.Ext) && !String.IsNullOrWhiteSpace(fromWish.Ext)) {
                this.Ext = fromWish.Ext;
                modified = true;
            }
            if (this.Url == null && fromWish.Url != null) {
                this.Url = fromWish.Url;
                modified = true;
            }
            if (String.IsNullOrWhiteSpace(this.CopyTo) && !String.IsNullOrWhiteSpace(fromWish.CopyTo)) {
                this.CopyTo = fromWish.CopyTo;
                modified = true;
            }
            if (this.Version == null && fromWish.Version != null) {
                this.Version = fromWish.Version;
                modified = true;
            }
            if (this.Classifiers.Count == 0 && fromWish.Classifiers.Count > 0 ) {
                this.Classifiers = fromWish.Classifiers.Clone();
                modified = true;
            }
            if (String.IsNullOrWhiteSpace(this.Arch) && !String.IsNullOrWhiteSpace(fromWish.Arch)) {
                this.Arch = fromWish.Arch;
                modified = true;
            }
            if (String.IsNullOrWhiteSpace(this.Runtime) && !String.IsNullOrWhiteSpace(fromWish.Runtime)) {
                this.Runtime = fromWish.Runtime;
                modified = true;
            }

            if (fromWish.Scope > Scope) {
                this.Scope = fromWish.Scope;
                modified = true;
            }

            //TODO:also set sources on the merged lists
            this.TransitiveWishes = MergeLists(fromWish.TransitiveWishes, this.TransitiveWishes);

            if (modified) {
                SourceLocations.AddToSourceLocations(this, fromWish.Source);
            }
        }

        private static List<Wish> MergeLists(IList<Wish> parentWishes, IList<Wish> childWishes) {
            if (parentWishes == null && childWishes == null) {
                return new List<Wish>();
            }
            if (parentWishes == null  || parentWishes.Count == 0) {
                return new List<Wish>(childWishes);
            }
            if (childWishes == null || childWishes.Count == 0) {
                return new List<Wish>(parentWishes);
            }

            var merged = parentWishes.ToDictionary(d=>d.GetKey());
            foreach (var wish in childWishes) {
                var sig = wish.GetKey();
                if (merged.ContainsKey(sig)) {
                    var parentDep = merged[sig];
                    //merged.Remove(key);
                    merged[sig] = wish.Clone().CloneAndFillInBlanksFrom(parentDep);
                } else {
                    merged[sig] = wish;
                }
            }
            
            return new List<Wish>(merged.Values);
        }

        public void ValidateRequiredSet() {
            ValidateMergeValuesSet();
            //if (Version==null) {
            //    throw new ArgumentException("Expect Version to be set on " + this);
            //}           // if (String.IsNullOrWhiteSpace(Ext)) {
           //     throw new ArgumentException("Expect Extension to be set on " + this);
           // }
        }

        public void ValidateMergeValuesSet() {
            if (String.IsNullOrWhiteSpace(Group)) {
                throw new ArgumentException("Expect Group to be set on " + this);
            }
            if (String.IsNullOrWhiteSpace(Name)) {
                throw new ArgumentException("Expect Name to be set on " + this);
            }
            if (String.IsNullOrWhiteSpace(Arch)) {
                throw new ArgumentException("Expect Arch to be set on " + this);
            }
            if (String.IsNullOrWhiteSpace(Runtime)) {
                throw new ArgumentException("Expect Runtime to be set on " + this);
            }
        }

        protected override void ToString(StringBuilder sb) {
            base.ToString(sb);
            sb.Append(",\n\tVersion=").Append(Version);
            sb.Append(",\n\tCopyTo=").Append(CopyTo);
            sb.Append(",\n\tTransitiveWishes=").Append(String.Join(",\n\t\t", TransitiveWishes));
        }

        public override String ToSummary() {
            return String.Format(GetType().Name + "<{0}:{1}:{2}:{3}>", GetKey(), Version, Ext, Scope);
        }
    }
}
