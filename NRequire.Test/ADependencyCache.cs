using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NRequire {
    internal class ADependencyCache : DependencyCache {

        internal static ADependencyCache With() {
            var cache = new ADependencyCache();
            cache.CacheDir = FileUtil.newTmpDir();
            return cache;
        }

        internal ADependencyCache Dependencies(IEnumerable<Dependency> deps) {
            foreach (var d in deps) {
                Dependency(d);
            }
            return this;
        }

        internal ADependencyCache Dependency(Dependency d) {
            var relPath = GetRelPathFor(d);
            var file = new FileInfo(Path.Combine(CacheDir.FullName, relPath));
            file.Directory.Create();
            using (var stream = file.Open(FileMode.Create, FileAccess.Write)) {
                var bytes = System.Text.Encoding.UTF8.GetBytes("dep:" + d.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
            return this;
        }

    }
}
