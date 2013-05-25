using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRequire.Test;
using NRequire.Util;

namespace NRequire {
    public class NewModule : Module {

        public static NewModule With() {
            return new NewModule();
        }

        /// <summary>
        /// Parse from  group:name:version:ext:classifiers
        /// </summary>
        /// <param name="fullString">Full string.</param>
        public static new NewModule Parse(String fullString) {
            var module = new NewModule();
            module.SetAllFromParse(fullString);
            return module;
        }


        public NewModule Defaults() {
            TestDefaults.Apply(this);
            return this;
        }

        public List<Module> Versions(params String[] versions) {
            var list = new List<Module>();
            foreach (var v in versions) {
                var clone = Clone();
                clone.Version = NRequire.Version.Parse(v);
                list.Add(clone);
            }
            return list;
        }
        public new NewModule Version(String val) {
            base.Version = NRequire.Version.Parse(val);
            return this;
        }

        public new NewModule Group(String val) {
            base.Group = val;
            return this;
        }

        public new NewModule Name(String val) {
            base.Name = val;
            return this;
        }

        public new NewModule Arch(String val) {
            base.Arch = val;
            return this;
        }

        public new NewModule Runtime(String val) {
            base.Runtime = val;
            return this;
        }

        public new NewModule Ext(String val) {
            base.Ext = val;
            return this;
        }

        public new NewModule Classifiers(Classifiers c) {
            base.Classifiers = c;
            return this;
        }

        public new NewModule Classifiers(String s) {
            base.Classifiers = NRequire.Classifiers.Parse(s);
            return this;
        }

        public new NewModule Source(ISource location) {
            SourceLocations.AddSourceLocations(this, location);
            return this;
        }


        /// <summary>
        /// group:name:version:ext:classifiers:scope
        /// </summary>
        /// <param name="parseString"></param>
        /// <returns></returns>
        public NewModule RuntimeWishFrom(String parseString) {
            RuntimeWishWith(NewWish.Parse(parseString));
            return this;
        }

        public NewModule RuntimeWishWith(String name, String version) {
            RuntimeWishWith(NewWish.With().Defaults().Name(name).Version(version));
            return this;
        }

        public NewModule RuntimeWishWith(Wish wish) {
            base.RuntimeWishes.Add(wish);
            return this;
        }

        public NewModule TransitiveWishWith(Wish wish) {
            base.TransitiveWishes.Add(wish);
            return this;
        }

        public NewModule TransitiveWishFrom(String parseString) {
            base.TransitiveWishes.Add(NewWish.Parse(parseString));
            return this;
        }

        public NewModule OptionalWishWith(Wish wish) {
            base.OptionalWishes.Add(wish);
            return this;
        }

        public NewModule OptionalWishFrom(String parseString) {
            base.OptionalWishes.Add(NewWish.Parse(parseString));
            return this;
        }

    }
}
