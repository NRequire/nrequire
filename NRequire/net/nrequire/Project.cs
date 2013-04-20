using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {
    public class Project {
        private static readonly DependencyWish DefaultDependencyValues = new DependencyWish { Arch = "any", Runtime = "any" };

        public String ProjectFormat { get; set; }
        public IList<DependencyWish> Compile { get;set;}
        public IList<DependencyWish> Provided { get; set; }
        public IList<DependencyWish> Transitive { get; set; }
        
        public DependencyWish DependencyDefaults { get; set; }

        public Project() {
            Compile = new List<DependencyWish>();
            Provided = new List<DependencyWish>(); 
            Transitive = new List<DependencyWish>();
            DependencyDefaults = DefaultDependencyValues.Clone();
        }

        public void AfterLoad() {
            if (ProjectFormat != "1") {
                throw new ArgumentException("This solution only supports format version 1. Instead got " + ProjectFormat);
            }
            //Apply defaults
            DependencyDefaults = DependencyDefaults == null ? DefaultDependencyValues.Clone() : DependencyDefaults.FillInBlanksFrom(DefaultDependencyValues);
            Compile = DependencyWish.FillInBlanksFrom(Compile, DependencyDefaults);
            Provided = DependencyWish.FillInBlanksFrom(Provided, DependencyDefaults);
            Transitive = DependencyWish.FillInBlanksFrom(Compile, DependencyDefaults);
        }
    }
}
