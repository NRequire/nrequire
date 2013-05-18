using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire {
    public class Module : Dependency, IRequireLoadNotification, ISource {

        public virtual String SourceName { get { return "Module:" + Source; } }

        public List<Wish> RuntimeWishes { get; set; }
        //for additional runtime options. Like transitive but clearer
        public List<Wish> OptionalWishes { get; set; }
        public List<Wish> TransitiveWishes { get; set; }

        public Module() {
            RuntimeWishes = new List<Wish>();
            OptionalWishes = new List<Wish>();
            TransitiveWishes = new List<Wish>();
        }

        public new Module Clone() {
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

            SourceLocations.AddSourceLocations(RuntimeWishes, Source);
            SourceLocations.AddSourceLocations(OptionalWishes, Source);
            SourceLocations.AddSourceLocations(TransitiveWishes, Source);
        }


        public override string ToString() {
            var sb = new StringBuilder("Module@").Append(GetHashCode()).Append("<");
            ToString(sb);
            sb.Append(">");
            return sb.ToString();
        }

        protected override void ToString(StringBuilder sb) {
            base.ToString(sb);
            sb.Append(",\n\tRuntimeWishes=").Append(String.Join(",\n\t\t", RuntimeWishes));
            sb.Append(",\n\tOptionalWishes=").Append(String.Join(",\n\t\t", OptionalWishes));
            sb.Append(",\n\tTransitiveWishes=").Append(String.Join(",\n\t\t", TransitiveWishes));
        }
    }
}
