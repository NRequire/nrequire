using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NRequire.Json;

namespace NRequire {

    public abstract class AbstractDependency : ITakeSourceLocation  {

        public const String DefaultArch = "any";
        public const String DefaultRuntime = "any";
        public const String DefaultExt = "dll";

        public const String KeyArch = "arch";
        public const String KeyRuntime = "runtime";

        //where the dep was created/read from. Useful for debugging!
        [JsonIgnore]
        public SourceLocations Source { get; set; }

        public string Group { get; set; }
        public string Name { get; set; }

        //TODO:move into wish?
        [JsonIgnore]
        public Scopes Scope { get; internal set; }

        public Uri Url { get; set; }
        public String Ext { get; set; }

        public string Arch { get { return Classifiers[KeyArch]; } set { Classifiers[KeyArch] = value; } }
        public string Runtime { get { return Classifiers[KeyRuntime]; } set { Classifiers[KeyRuntime] = value; } }

        private Classifiers m_classifiers = new Classifiers();

        [JsonConverter(typeof(ClassifierConverter))]
        public Classifiers Classifiers { 
            get { return m_classifiers; }
            set { m_classifiers = value != null ? value:new Classifiers(); }
        }

        public AbstractDependency() {
        }

        protected internal T Clone<T>(T clone) where T:AbstractDependency {
            clone.Classifiers = Classifiers.Clone();
            clone.Ext = Ext;
            clone.Group = Group;
            clone.Name = Name;
            clone.Url = Url;
            clone.Scope = Scope;
            return clone;
        }

        public void SetAllFrom(AbstractDependency d) {
            Group = d.Group;
            Name = d.Name;
            Ext = d.Ext;
            Classifiers = d.Classifiers.Clone();
        }


        public void SetAllFrom(IResolved r) {
            Group = r.Group;
            Name = r.Name;
            Ext = r.Ext;
            Classifiers = r.Classifiers.Clone();
        }

        public string GetKey() {
            return String.Format("{0}:{1}:{2}"
                , Group
                , Name
                , Classifiers.ToString()
            ).ToLower();
        }

        public virtual String ToSummary() {
            return String.Format(GetType().Name + "<{0}>", GetKey());
        }

        public override string ToString() {
            var sb = new StringBuilder( GetType().Name + "@").Append(GetHashCode()).Append("<");
            ToString(sb);
            sb.Append(">");
            return sb.ToString();
        }

        protected virtual void ToString(StringBuilder sb) {
            sb.Append("\n\tGroup=").Append(Group);
            sb.Append(",\n\tName=").Append(Name);
            sb.Append(",\n\tExt=").Append(Ext);
            sb.Append(",\n\tArch=").Append(Arch);
            sb.Append(",\n\tRuntime=").Append(Runtime);
            sb.Append(",\n\tClassifiers=").Append(Classifiers);
            sb.Append(",\n\tUrl=").Append(Url);
            sb.Append(",\n\tSource=").Append(Source);
            sb.Append(",\n\tScope=").Append(Scope);
        }

    }
}
