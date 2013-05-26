using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRequire.Test;
using NRequire.Lang;

namespace NRequire {

    public class NewWish : Wish, IBuilder<Wish> {

        public static NewWish With() {
            return new NewWish();
        }

        /// <summary>
        /// Parse from group:name:version:ext:classifiers:scope
        /// </summary>
        /// <param name="fullString">Full string.</param>
        public static new NewWish Parse(String fullString) {
            var wish = new NewWish();
            wish.Defaults();
            wish.SetAllFromParse(fullString);
            return wish;
        }

        public Wish Build(){
            return this.Clone();
        }

        public NewWish Defaults() {
            TestDefaults.Apply(this);
            return this;
        }

        public new NewWish Group(String val) {
            base.Group = val;
            return this;
        }

        public new NewWish Name(String val) {
            base.Name = val;
            return this;
        }

        public new NewWish Arch(String val) {
            base.Arch = val;
            return this;
        }

        public new NewWish Runtime(String val) {
            base.Runtime = val;
            return this;
        }

        public new NewWish Ext(String val) {
            base.Ext = val;
            return this;
        }

        public new NewWish Version(String val) {
            base.Version = VersionMatcher.Parse(val);
            return this;
        }

        public new NewWish Classifiers(String s) {
            base.ClassifiersString = s;
            return this;
        }

        public new NewWish Source(ISource location) {
            SourceLocations.AddSourceLocations(this, location);
            return this;
        }

    }
}
