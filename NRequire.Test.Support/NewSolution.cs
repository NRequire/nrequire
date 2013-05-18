using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire {
    public class NewSolution : Solution {

        public static NewSolution With() {
            return new NewSolution();
        }

        public NewSolution Dependency(Wish d) {
            base.Wishes.Add(d.Clone());
            return this;
        }

    }
}
