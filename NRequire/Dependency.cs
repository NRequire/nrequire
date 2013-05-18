using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NRequire.Json;

namespace NRequire {

	/// <summary>
	/// The result of a wish being resolved. A single, concrete dependencies. This could be split into mutiple resources (dll,xml,pbe...)
	/// </summary>
    public class Dependency : AbstractDependency {

        [JsonIgnore]
        internal String VersionString {
            get { return Version == null ? null : Version.ToString(); }
            set { Version = value == null ? null : Version.Parse(value); }
        }

        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; internal set; }

        public bool EmbeddedResource { get; set; }


        /// <summary>
        /// Parse from  group:name:version:ext:classifiers
        /// </summary>
        /// <param name="fullString">Full string.</param>
        public static Dependency Parse(String fullString) {
            var dep = new Dependency();

            var parts = fullString.Split(new char[] { ':' });
            if (parts.Length > 0) {
                dep.Group = parts[0];
            }
            if (parts.Length > 1) {
                dep.Name = parts[1];
            }
            if (parts.Length > 2) {
                dep.Version = Version.Parse(parts[2]);
            }
            if (parts.Length > 3) {
                dep.Ext = parts[3];
            }
            if (parts.Length > 4) {
                dep.Classifiers = Classifiers.Parse(parts[4]);
            }

            return dep;
        }

        public Dependency Clone() {
            return Clone(new Dependency());    
        }

        protected internal new T Clone<T>(T cloneTarget) where T : Dependency {
            base.Clone(cloneTarget);
            cloneTarget.Scope = Scope;
            cloneTarget.Version = Version;

            SourceLocations.AddSourceLocations(cloneTarget, Source);
            return cloneTarget;
        }

        public override string ToString() {
            var sb = new StringBuilder("Dependency@").Append(GetHashCode()).Append("<");
            ToString(sb);
            sb.Append(">");
            return sb.ToString();
        }

        protected virtual void ToString(StringBuilder sb) {
            sb.Append("\n\tGroup=").Append(Group);
            sb.Append(",\n\tName=").Append(Name);
            sb.Append(",\n\tVersion=").Append(Version);
            sb.Append(",\n\tExt=").Append(Ext);
            sb.Append(",\n\tArch=").Append(Arch);
            sb.Append(",\n\tRuntime=").Append(Runtime);
            sb.Append(",\n\tClassifiers=").Append(Classifiers);
            sb.Append(",\n\tUrl=").Append(Url);
            sb.Append(",\n\tCopyTo=").Append(CopyTo);
            sb.Append(",\n\tSource=").Append(Source);
            sb.Append(",\n\tScope=").Append(Scope);
            //sb.Append(",\n\tRelated=").Append(Related == null ? null : String.Join(",", Related));
        }
    }
}
