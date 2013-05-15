using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NRequire.Json;

namespace NRequire {
    
    public abstract class AbstractDependency {

        public const String KeyArch = "arch";
        public const String KeyRuntime = "runtime";

        public string Group { get; set; }
        public string Name { get; set; }
        public Uri Url { get; set; }

        public String Ext { get; set; }

        //to be moved into classifiers
        public string Arch { get { return Classifiers[KeyArch]; } set { Classifiers[KeyArch] = value; } }

        public string Runtime { get { return Classifiers[KeyRuntime]; } set { Classifiers[KeyRuntime] = value; } }

        public String CopyTo { get; set; }

        
        [JsonConverter(typeof(ClassifierConverter))]
        public Classifiers Classifiers { get; set; }


        public AbstractDependency() {
            Classifiers = new Classifiers();
        }
    }
}
