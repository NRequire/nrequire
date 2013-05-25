using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NRequire.Lang;

namespace NRequire {
    public class NewDependencyCache : IDependencyCache {

        private static readonly Logger Log = Logger.GetLogger(typeof(NewDependencyCache));

        private readonly JsonWriter m_writer = new JsonWriter();

        private DependencyCache m_cache;

        private NewDependencyCache() {
            m_cache = new DependencyCache.Builder { CacheDir = FileUtil.newTmpDir("deps-cache") }.Build();
        }

        public static NewDependencyCache With() {
            return new NewDependencyCache();
        }

        public DirectoryInfo GetCacheDir(){
            return m_cache.CacheDir;
        }

        public IList<Resource> GetResourcesFor(IResolved d) {
            return m_cache.GetResourcesFor(d);
        }

        public IList<Dependency> FindDependenciesMatching(Wish wish) {
            return m_cache.FindDependenciesMatching(wish);
        }

        public IList<Wish> FindWishesFor(IResolved d) {
            return m_cache.FindWishesFor(d);
        }

        public NewDependencyCache A(IBuilder<Module> builder) {
            A(builder.Build());
            return this;
        }

        public NewDependencyCache A(Module module) {
            var resourceFile = m_cache.GetFullPathFor(module);
            WriteFileWithContent(resourceFile, "module.binary.file.for:" + module.ToString());

            //write the wishes to disk
            var jsonFile = m_cache.GetFullPathFor(m_cache.GetRelPathFor(module) + ".nrequire.module.json");
            var json = m_writer.Write(module);
            WriteFileWithContent(jsonFile, json);

            return this;
        }


        public NewDependencyCache Dependencies(IEnumerable<Module> modules) {
            foreach (var mod in modules) {
                A(mod);
            }
            return this;
        }

        private static void WriteFileWithContent(FileInfo file, String content) {
            Log.DebugFormat("Writing file '{0}'", file.FullName);
            if (Log.IsTraceEnabled()) {
                Log.Trace("File content = " + content);
            }
            file.Directory.Create();
            using (var stream = file.Open(FileMode.Create, FileAccess.Write)) {
                var bytes = System.Text.Encoding.UTF8.GetBytes(content);
                stream.Write(bytes, 0, bytes.Length);
            }
        }


    }
}
