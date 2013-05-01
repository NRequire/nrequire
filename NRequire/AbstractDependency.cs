using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire {
    
    public abstract class AbstractDependency {

        public string Group { get; set; }
        public string Name { get; set; }
        public Uri Url { get; set; }

        public String Ext { get; set; }

        //to be moved into classifiers
        public string Arch { get; set; }

        public string Runtime { get; set; }

        public String CopyTo { get; set; }

    }
}
