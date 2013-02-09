using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace net.nrequire {

    internal class DependencyCache {

        public DependencyCache UpstreamCache { get; set; }
        public DirectoryInfo CacheDir { get; set; }
        public String VSProjectBaseSymbol { get; set; }

        public bool ContainsResource(SpecificDependency d) {
            return GetResourceFor(d).Exists;
        }

        public Resource GetResourceFor(SpecificDependency d) {
            var relPath = GetRelPathFor(d);
            var file = new FileInfo(Path.Combine(CacheDir.FullName,relPath));
            //TODO:if SNAPSHOT,then check localcache timestamp
            if (!file.Exists && UpstreamCache != null) {
                var parentResource = UpstreamCache.GetResourceFor(d);
                if (parentResource.Exists) {
                    parentResource.CopyTo(file);
                }
            }
            return new Resource(d, file, VSProjectBaseSymbol + "\\" + relPath);
        }

        private String GetRelPathFor(SpecificDependency d) {
            var parts = new List<String>(3);
            parts.Add(String.Format("{0}\\{1}\\{2}", d.Group, d.Name, d.Version.ToString()));
            var classifiers = new List<String>(3);

            if (!String.IsNullOrEmpty(d.Arch)) {
                classifiers.Add("arch-" + d.Arch);
            } 
            if (!String.IsNullOrEmpty(d.Runtime)) {
                classifiers.Add("runtime-" + d.Runtime);
            }
            if(classifiers.Count > 0){
                parts.Add(String.Join("_",classifiers));
            }
            parts.Add(d.Name + "." + d.Ext);
            return Path.Combine(parts.ToArray());
        }
    }
}
