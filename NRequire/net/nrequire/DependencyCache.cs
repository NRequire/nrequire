using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace net.nrequire {

    public interface IDependencyCache {
        bool ContainsResource(SpecificDependency d);
        Resource GetResourceFor(SpecificDependency d);

        IList<Version> GetVersionsMatching(Dependency dep);
    }

    internal class DependencyCache : IDependencyCache {

        public DependencyCache UpstreamCache { get; set; }
        public DirectoryInfo CacheDir { get; set; }
        public String VSProjectBaseSymbol { get; set; }

        private readonly Logger Log = Logger.GetLogger(typeof(DependencyCache));

        public IList<Version> GetVersionsMatching(Dependency dep) {
            var relPath = String.Format("{0}\\{1}", dep.Group, dep.Name);
            var dir = new DirectoryInfo(Path.Combine(CacheDir.FullName,relPath));
            Log.DebugFormat("Versions dir '{0}'", dir.FullName);
            if(!dir.Exists){
                return new List<Version>();
            }
            //And what about empty?
            var c = dep.Classifiers.Clone();
            if (!String.IsNullOrWhiteSpace(dep.Arch)) {
                c.Set("arch", dep.Arch);
            }
            if (!String.IsNullOrWhiteSpace(dep.Runtime)) {
                c.Set("runtime", dep.Runtime);
            }

            var classifiers = c.ToString();
            if (String.IsNullOrWhiteSpace(classifiers)) {
                classifiers = null;
            }
            //TODO:cache this?
                    //.Where((versionDir) => Directory.GetDirectories(Path.Combine(dir.FullName,versionDir)).Contains(classifiers));
            //check each contais the matching classifiers
            if (Log.IsDebugEnabled()) {
                Log.DebugFormat("Versions dir.dirs '{0}'", String.Join(",", dir.EnumerateDirectories()));
            }
            var versionDirs = classifiers == null ? dir.EnumerateDirectories() : dir.DirectoriesWithSubDirsNamed(classifiers);

            var versions = versionDirs
                .Select((versionDir) => Version.Parse(versionDir.Name))
                .ToList();
            
            versions.Sort();
            versions.Reverse();

            if (Log.IsDebugEnabled()) {
                Log.DebugFormat("Versions found for dep {0} an classifier '{1}' were {2}", dep, classifiers, String.Join(",", versions));
            }

            return versions;
        }

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

    static class DirectoryInfoExtenions {

        public static IEnumerable<DirectoryInfo> DirectoriesWithSubDirsNamed(this DirectoryInfo dir, String subDirName) {
            return dir.EnumerateDirectories()
                    .Where((child) => child.ContainsDirNamed(subDirName));
        }
        
        
        public static bool ContainsDirNamed(this DirectoryInfo dir, String subDirName) {
            return dir.EnumerateDirectories()
                    .Where((child) => child.Name.Equals(subDirName))
                    .Count() > 0;
        }


    }
}
