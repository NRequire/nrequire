using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace net.nrequire {

    internal class DependencyCache {
        
        public DirectoryInfo CacheDir { get; set; }
        public String VSProjectBaseSymbol { get; set; }

        public bool ContainsResource(Dependency d) {
            return GetResourceFor(d).Exists;
        }

        public Resource GetResourceFor(Dependency d) {
            var relPath = GetRelPathFor(d);
            var fullPath = new FileInfo(Path.Combine(CacheDir.FullName,relPath));
            return new Resource(d, fullPath, VSProjectBaseSymbol + "\\" + relPath);
        }

        private String GetRelPathFor(Dependency d) {
            var paths = new List<string>();

            var basePath = String.Format(
                "{0}\\{1}",d.GroupId, d.Version
            );
            paths.Add(basePath);
            if (!String.IsNullOrEmpty(d.Runtime)) {
                paths.Add("runtime-" + d.Runtime);
            }
            if (!String.IsNullOrEmpty(d.Arch)) {
                paths.Add("arch-" + d.Arch);
            }
            paths.Add(d.ArtifactId + "." + d.Ext);

            return Path.Combine(paths.ToArray());
        }
    }
}
