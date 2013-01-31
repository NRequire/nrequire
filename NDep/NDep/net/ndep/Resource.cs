using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace net.ndep {
    public class Resource {

        public String VSProjectPath { get; private set; }
        public FileInfo File { get; private set; }
        public bool Exists { get { return File.Exists; } }
        public Dependency Dep { get;set; }

        public Resource(Dependency dep, FileInfo fullPath, String vsProjectPath) {
            Dep = dep;
            File = fullPath;
            VSProjectPath = vsProjectPath;
        }

        public override string ToString() {
            return String.Format("Resource@{0}<FullPath:{1},VSProjectPath:{2},Dependency:{3}>",
                base.GetHashCode(),
                File.FullName,
                VSProjectPath,
                Dep
            );
        }
    }
}
