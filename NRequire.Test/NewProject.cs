using System;
using NRequire.Model;

namespace NRequire {
    public class NewProject : Project {

        public static NewProject With() {
            var p = new NewProject();
            p.ProjectFormat = "1";
            return p;
        }

        public NewProject RuntimeWish(Wish d) {
            base.RuntimeWishes.Add(d.Clone());
            return this;
        }

        /// <summary>
        /// Parse from  group:name:version:ext:classifiers:scope
        /// </summary>
        public NewProject RuntimeWish(String parseString) {
            base.RuntimeWishes.Add(Wish.Parse(parseString));
            return this;
        }

        public NewProject TransitiveWish(Wish d) {
            base.TransitiveWishes.Add(d.Clone());
            return this;
        }

        /// <summary>
        /// Parse from  group:name:version:ext:classifiers:scope
        /// </summary>
        public NewProject TransitiveWish(String parseString) {
            base.TransitiveWishes.Add(Wish.Parse(parseString));
            return this;
        }

    }
}
