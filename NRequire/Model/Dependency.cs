using System;
using System.Text;
using Newtonsoft.Json;
using NRequire.IO.Json;
using NRequire.Util;

namespace NRequire.Model {

    /// <summary>
    /// The result of a wish being resolved. A single, concrete dependencies. This could be split into mutiple resources (dll,xml,pbe...)
    /// </summary>
    public class Dependency : AbstractDependency, IResolved {

        [JsonIgnore]
        internal String VersionString {
            get { return Version == null ? null : Version.ToString(); }
            set { Version = value == null ? null : Version.Parse(value); }
        }

        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; internal set; }

        /// <summary>
        /// Parse from  group:name:version:ext:classifiers
        /// </summary>
        /// <param name="fullString">Full string.</param>
        public static Dependency Parse(String fullString) {
            var d = new Dependency();
            d.SetAllFromParse(fullString);
            return d;
        }

        protected void SetAllFromParse(String fullString) {
            PartsParser.ParseParts(fullString,
            (s) => Group = s,
            (s) => Name = s,
            (s) => Version = Version.Parse(s),
            (s) => Ext = s,
            (s) => Classifiers = Classifiers.Parse(s));
        }

        public Dependency Clone() {
            return Clone(new Dependency());    
        }

        protected internal new T Clone<T>(T cloneTarget) where T : Dependency {
            base.Clone(cloneTarget);
            cloneTarget.Scope = Scope;
            cloneTarget.Version = Version;

            SourceLocations.AddToSourceLocations(cloneTarget, Source);
            return cloneTarget;
        }

        protected override void ToString(StringBuilder sb) {
            base.ToString(sb);
            sb.Append(",\n\tVersion=").Append(Version);
        }

        public override String ToSummary() {
            return String.Format(GetType().Name + "<{0}:{1}:{2}:{3}>", GetKey(), Version, Ext, Scope);
        }

    }
}
