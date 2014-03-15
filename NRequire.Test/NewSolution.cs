using System;
using NRequire.Model;

namespace NRequire {
    public class NewSolution : Solution {

        public static NewSolution With() {
            var soln = new NewSolution();
            soln.SolutionFormat = "1";
            return soln;
        }

        /// <summary>
        /// Parse from  group:name:version:ext:classifiers:scope
        /// </summary>
        public NewSolution Wish(String parseString) {
            Wish(Model.Wish.Parse(parseString));
            return this;
        }

        public NewSolution Wish(Wish wish) {
            base.Wishes.Add(wish.Clone());
            return this;
        }

    }
}
