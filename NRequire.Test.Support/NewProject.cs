using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire {
    public class NewProject : Project {
        public static NewProject With() {
            return new NewProject();
        }

        public NewProject RuntimeWish(Wish d) {
            base.RuntimeWishes.Add(d.Clone());
            return this;
        }

        public NewProject TransitiveWish(Wish d) {
            base.TransitiveWishes.Add(d.Clone());
            return this;
        }
    }
}
