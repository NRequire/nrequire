using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {
    public class Project {
        private static readonly Dependency DefaultDependencyValues = new Dependency { Arch = "any", Runtime = "any" };

        public String ProjectFormat { get; set; }
        public IList<Dependency> Compile { get;set;}
        public IList<Dependency> Provided { get; set; }
        public IList<Dependency> Transitive { get; set; }
        
        public Dependency DependencyDefaults { get; set; }

        public Project() {
            Compile = new List<Dependency>();
            Provided = new List<Dependency>(); 
            Transitive = new List<Dependency>();
            DependencyDefaults = DefaultDependencyValues.Clone();
        }

        public void AfterLoad() {
            if (ProjectFormat != "1") {
                throw new ArgumentException("This solution only supports format version 1. Instead got " + ProjectFormat);
            }
            //Apply defaults
            DependencyDefaults = DependencyDefaults == null ? DefaultDependencyValues.Clone() : DependencyDefaults.FillInBlanksFrom(DefaultDependencyValues);
            Compile = Dependency.FillInBlanksFrom(Compile, DependencyDefaults);
            Provided = Dependency.FillInBlanksFrom(Provided, DependencyDefaults);
            Transitive = Dependency.FillInBlanksFrom(Compile, DependencyDefaults);
        }
    }
}
