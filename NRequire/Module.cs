using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRequire.Util;

namespace NRequire {
    public class Module : AbstractDependency, IRequireLoadNotification, ISource, IResolved {

        public virtual String SourceName { get { return "Module:" + Source; } }

        public Version Version { get; set;  }
        public List<Wish> RuntimeWishes { get; set; }
        //for additional runtime options. Like transitive but clearer
        public List<Wish> OptionalWishes { get; set; }
        public List<Wish> TransitiveWishes { get; set; }

        public Module() {
            RuntimeWishes = new List<Wish>();
            OptionalWishes = new List<Wish>();
            TransitiveWishes = new List<Wish>();
        }

        public Dependency ToDependency() {
            return new Dependency { 
                Group = Group,
                Name = Name,
                Version = Version,
                Url = Url,
                Classifiers = Classifiers.Clone(),
                Ext = Ext,
                Source = new SourceLocations().Add(Source)
            };
        }
        /// <summary>
        /// Parse from  group:name:version:ext:classifiers
        /// </summary>
        /// <param name="fullString">Full string.</param>
        public static Module Parse(String fullString) {
            var module = new Module();
            module.SetAllFromParse(fullString);
            return module;
        }

        public void SetAllFromParse(String fullString) {
            DepParser.Parse(fullString,
            (s) => Group = s,
            (s) => Name = s,
            (s) => Version = Version.Parse(s),
            (s) => Ext = s,
            (s) => Classifiers = Classifiers.Parse(s));
        }

        public Module Clone() {
            return Clone(new Module());
        }

        protected internal new T Clone<T>(T cloneTarget) where T : Module {
            base.Clone(cloneTarget);
            cloneTarget.RuntimeWishes = new List<Wish>(RuntimeWishes);
            cloneTarget.OptionalWishes = new List<Wish>(OptionalWishes);
            cloneTarget.TransitiveWishes = new List<Wish>(TransitiveWishes);
            return cloneTarget;
        }

        public virtual void AfterLoad(){
            RuntimeWishes.ForEach(w=>w.Scope = Scopes.Runtime);
            OptionalWishes.ForEach(w=>w.Scope = Scopes.Transitive);
            TransitiveWishes.ForEach(w=>w.Scope = Scopes.Transitive);

            SourceLocations.AddToSourceLocations(RuntimeWishes, Source);
            SourceLocations.AddToSourceLocations(OptionalWishes, Source);
            SourceLocations.AddToSourceLocations(TransitiveWishes, Source);
        }

        public List<Wish> GetWishes() {
            var wishes = new List<Wish>();
            wishes.AddRange(RuntimeWishes);
            wishes.AddRange(OptionalWishes);
            wishes.AddRange(TransitiveWishes);

            return wishes;
        }

        protected override void ToString(StringBuilder sb) {
            base.ToString(sb);
            sb.Append(",\n\tRuntimeWishes=").Append(String.Join(",\n\t\t", RuntimeWishes));
            sb.Append(",\n\tOptionalWishes=").Append(String.Join(",\n\t\t", OptionalWishes));
            sb.Append(",\n\tTransitiveWishes=").Append(String.Join(",\n\t\t", TransitiveWishes));
        }

        public override String ToSummary() {
            return String.Format(GetType().Name + "<{0}:{1}:{2}>", GetKey(), Version, Ext);
        }
    }
}
