using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NRequire {
    internal class NewDependencyCache : DependencyCache {

        private readonly JsonWriter m_writer = new JsonWriter();

        internal static NewDependencyCache With() {
            var cache = new NewDependencyCache();
            cache.CacheDir = FileUtil.newTmpDir();
            return cache;
        }

        internal NewDependencyCache A(DependencyNode node) {
            return Dependency(node);
        }

        internal NewDependencyCache Dependency(DependencyNode node) {
            Dependency((Dependency)node);
            var file = GetFullPathFor(GetRelPathFor(node) + ".nrequire.json");
            var wishes = new List<DependencyWish>(node.Wishes);
            m_writer.WriteTo(node, file);
            return this;
        }


        internal NewDependencyCache Dependencies(IEnumerable<Dependency> deps) {
            foreach (var d in deps) {
                Dependency(d);
            }
            return this;
        }

        internal NewDependencyCache Dependency(Dependency d) {
            var file = GetFullPathFor(d);
            WriteFileWithContent(file, "dep:" + d.ToString());
            return this;
        }

        private static void WriteFileWithContent(FileInfo file, String content) {
            file.Directory.Create();
            using (var stream = file.Open(FileMode.Create, FileAccess.Write)) {
                var bytes = System.Text.Encoding.UTF8.GetBytes(content);
                stream.Write(bytes, 0, bytes.Length);
            }
        }


    }
}
