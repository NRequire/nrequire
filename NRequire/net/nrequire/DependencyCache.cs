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
            var paths = new List<string>();

            var basePath = String.Format(
                "{0}\\{1}\\{2}.{3}.{4}", d.GroupId, d.ArtifactId, d.Version.Major, d.Version.Minor, d.Version.Revision
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
