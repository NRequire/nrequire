using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRequire;
using NRequire.Json;
using Newtonsoft.Json;

namespace NRequire {
    public class Project : Module, IRequireLoadNotification, ISource {

        public const String SupportedVersion = "1";

        public override String SourceName { get { return "Project:" + Source; } }

        private static readonly Wish DefaultWishValues = new Wish { 
            Arch = AbstractDependency.DefaultArch, 
            Runtime = AbstractDependency.DefaultRuntime, 
            Source = new SourceLocations("Project.Default.Dep"),
            Scope = Scopes.Transitive
        };
        public String ProjectFormat { get; set; }

        public List<Wish> ProvidedWishes { get; set; }
        
        public Wish WishDefaults { get; set; }

        public Project() {
            //CompileWishes = new List<Wish>();
            ProvidedWishes = new List<Wish>();
            
            WishDefaults = DefaultWishValues.Clone();
        }

        public override void AfterLoad() {
            if (ProjectFormat != SupportedVersion) {
                throw new ArgumentException("This solution only supports format version " + SupportedVersion + ". Instead got " + ProjectFormat);
            }
            base.AfterLoad();

            //Apply defaults
            WishDefaults = WishDefaults == null ? DefaultWishValues.Clone() : WishDefaults.CloneAndFillInBlanksFrom(DefaultWishValues);
            WishDefaults.Scope = Scopes.Transitive;

            //CompileWishes = Wish.CloneAndFillInBlanksFrom(CompileWishes, WishDefaults);
            RuntimeWishes = PostProcess(RuntimeWishes, WishDefaults, Scopes.Runtime);
            ProvidedWishes = PostProcess(ProvidedWishes, WishDefaults, Scopes.Provided);
            OptionalWishes = PostProcess(OptionalWishes, WishDefaults, Scopes.Transitive);
            TransitiveWishes = PostProcess(TransitiveWishes, WishDefaults, Scopes.Transitive);

            ValidateReadyForMerge(RuntimeWishes);
            ValidateReadyForMerge(ProvidedWishes);
            ValidateReadyForMerge(OptionalWishes);
            ValidateReadyForMerge(TransitiveWishes);

        }

        private void ValidateReadyForMerge(List<Wish> wishes) {
            foreach (var wish in wishes) {
                wish.ValidateMergeValuesSet();
            }
        }

        public List<Wish> GetAllWishes() {

            var wishes = new WishList();
            wishes.AddOrFailIfExists(RuntimeWishes, 0);
            wishes.AddOrFailIfExists(ProvidedWishes, 0);
            wishes.AddOrFailIfExists(OptionalWishes, 0);
            wishes.AddOrFailIfExists(TransitiveWishes, 0);

            IncludeAllTransitivesOf(wishes, 0, wishes.ToList());

            return wishes.ToList();
        }

        private void IncludeAllTransitivesOf(WishList addTo, int depth,List<Wish> wishes) {
            foreach (var wish in wishes) {
                IncludeTransitivesOf(addTo, depth + 1,wish);
            }
        }

        private void IncludeTransitivesOf(WishList addTo, int depth, Wish wish) 
        {
            foreach (var child in wish.TransitiveWishes) {
                addTo.AddOrFailIfExists(child);
                IncludeTransitivesOf(addTo, depth + 1, child);
            }
        }

        private List<Wish> PostProcess(List<Wish> wishes, Wish defaults, Scopes scope) {
            wishes = Wish.CloneAndFillInBlanksFrom(wishes, defaults);
            wishes.ForEach(w => {
                w.Scope = scope;
                if( w.Version==null ){
                    w.Version = VersionMatcher.AnyMatcher;
                }
                //TODO:recursive?
                SetChildTransitivies(w);
            });

            SourceLocations.AddToSourceLocations(wishes, Source);
            return wishes;
        }

        private static void SetChildTransitivies(Wish wish){
            foreach (var child in wish.TransitiveWishes) {
                child.Scope = Scopes.Transitive;
                SetChildTransitivies(child);
            }
        }

        protected override void ToString(StringBuilder sb) {
            base.ToString(sb);
            //sb.Append(",\n\tCompileWishes=").Append(String.Join(",\n\t\t", CompileWishes));
            sb.Append(",\n\tProvidedWishes=").Append(String.Join(",\n\t\t", ProvidedWishes));
        }


    }
}
