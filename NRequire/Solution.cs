using System;
using System.Collections.Generic;

namespace NRequire {
    public class Solution : ITakeSourceLocation, IRequireLoadNotification, ISource {

        public const String SupportedVersion = "1";

        private static readonly Wish DefaultWishValues = new Wish { 
            Arch = AbstractDependency.DefaultArch,
            Runtime = AbstractDependency.DefaultRuntime,
            Source = new SourceLocations("Solution.Default.Wish"),
            Scope = Scopes.Transitive
        };

        public String SourceName { get { return "Solution:" + Source; } }

        public SourceLocations Source { get; set; }

        //TODO:add Repos:[] {name:central,url:path/to/some/repo,layout:maven|nget|...}
        public List<Wish> Wishes { get; set; }
 
        public Wish WishDefaults { get; set; }
        public String SolutionFormat { get;set; }

        public Solution() {
            Wishes = new List<Wish>();
            WishDefaults = DefaultWishValues.Clone();
        }

        public void AfterLoad() {
            if (SolutionFormat != SupportedVersion) {
                throw new ArgumentException("This solution only supports format version " + SupportedVersion + ". Instead got " + SolutionFormat);
            }
            //apply defaults
            WishDefaults = WishDefaults == null ? DefaultWishValues.Clone() : WishDefaults.CloneAndFillInBlanksFrom(DefaultWishValues);
            WishDefaults.Scope = Scopes.Transitive;
            Wishes = Wish.CloneAndFillInBlanksFrom(Wishes, WishDefaults);
            Wishes.ForEach(w => w.Scope = Scopes.Transitive);

            SourceLocations.AddToSourceLocations(Wishes, Source);

            ValidateReadyForMerge(Wishes);
        }

        public List<Wish> GetAllWishes() {
            return Wishes;
        }

        private void ValidateReadyForMerge(List<Wish> wishes) {
            foreach (var wish in wishes) {
                wish.ValidateMergeValuesSet();
            }
        }

        public override string ToString() {
            return base.ToString() + "<Source=" + Source + ">";
        }
    }
}
