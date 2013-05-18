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

        //TODO:move into wish?
        [JsonIgnore]
        public String CopyTo { get; set; }

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

        protected internal T Clone<T>(T cloneTarget) where T:AbstractDependency {
            cloneTarget.CopyTo = CopyTo;
            cloneTarget.Classifiers = Classifiers.Clone();
            cloneTarget.Ext = Ext;
            cloneTarget.Group = Group;
            cloneTarget.Name = Name;
            cloneTarget.Url = Url;
            cloneTarget.Scope = Scope;
            return cloneTarget;
        }

        public string Signature() {
            return String.Format("{0}:{1}:{2}"
                , Group
                , Name
                , Classifiers.ToString()
            ).ToLower();
        }

    }
}
