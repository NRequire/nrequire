using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NRequire {

    public interface IDependencyCache {
        bool ContainsDependency(Dependency d);
        Resource GetResourceFor(Dependency d);

        IList<Version> GetVersionsMatching(DependencyWish dep);

        IList<Dependency> FindDependenciesMatching(DependencyWish wish);

        IList<DependencyWish> FindWishesFor(Dependency dep);
    }

    internal class DependencyCache : IDependencyCache {

        public DependencyCache UpstreamCache { get; set; }
        public DirectoryInfo CacheDir { get; set; }
        public String VSProjectBaseSymbol { get; set; }

        private readonly Logger Log = Logger.GetLogger(typeof(DependencyCache));
        private readonly JsonReader m_reader = new JsonReader();

        public IList<Dependency> FindDependenciesMatching(DependencyWish wish){
            //TODO:reverse this so deps are returned and versions extracted?
            return GetVersionsMatching(wish).Select(v => new Dependency {
                Group = wish.Group,
                Name = wish.Name,
                Classifiers = wish.Classifiers.Clone(),
                Version = v
            }).ToList();
        }

        public IList<Version> GetVersionsMatching(DependencyWish wish) {
            return FindAllVersionsFor(wish)
                .Where(v=>wish.Version==null || wish.Version.Match(v))
                .ToList();
        }

        private IList<Version> FindAllVersionsFor(AbstractDependency dep){
            var relPath = String.Format("{0}\\{1}", dep.Group, dep.Name);
            var dir = new DirectoryInfo(Path.Combine(CacheDir.FullName,relPath));
            Log.DebugFormat("Versions dir '{0}'", dir.FullName);
            if(!dir.Exists){
                return new List<Version>();
            }
            //And what about empty?
            var classifierPathPart = ToPathPart(dep.Classifiers);
            if (String.IsNullOrWhiteSpace(classifierPathPart)) {
                classifierPathPart = null;
            }
            //TODO:cache this?
            //.Where((versionDir) => Directory.GetDirectories(Path.Combine(dir.FullName,versionDir)).Contains(classifiers));
            //check each contains the matching classifiers
            if (Log.IsDebugEnabled()) {
                Log.DebugFormat("Versions dir.dirs '{0}'", String.Join(",", dir.EnumerateDirectories()));
            }
            var versionDirs = (classifierPathPart == null ? dir.EnumerateDirectories() : dir.DirectoriesWithSubDirsNamed(classifierPathPart)).ToList();

            if (Log.IsDebugEnabled()) {
                Log.Debug("Version dirs:" + versionDirs);
                Log.DebugFormat("Version dirs found were {0}", String.Join(",", versionDirs));
            }

            var versions = versionDirs
                .Select((versionDir) => Version.Parse(versionDir.Name))
                .ToList();

            versions.Sort();
            versions.Reverse();

            if (Log.IsDebugEnabled()) {
                Log.DebugFormat("Versions found for dep {0} and classifier '{1}' were {2}", dep, dep.Classifiers, String.Join(",", versions));
            }

            return versions;
        }

        public bool ContainsDependency(Dependency d) {
            return GetResourceFor(d).Exists;
        }

        public Resource GetResourceFor(Dependency d) {
            var relPath = GetRelPathFor(d);
            var file = GetFullPathFor(relPath);
            //TODO:if SNAPSHOT,then check localcache timestamp
            if (!file.Exists && UpstreamCache != null) {
                var parentResource = UpstreamCache.GetResourceFor(d);
                if (parentResource.Exists) {
                    parentResource.CopyTo(file);
                }
            }
            return new Resource(d, file, VSProjectBaseSymbol + "\\" + relPath);
        }

        public IList<DependencyWish> FindWishesFor(Dependency dep){
            var relPath = GetRelPathFor(dep);
            var file = GetFullPathFor(relPath + ".nrequire.json");
            if(file.Exists){
                return m_reader.Read<DependencyDto>(file).Wishes;
            }
            var noUpstreamMarkerFile = GetFullPathFor(relPath + ".nrequire.json.none");
            if (!noUpstreamMarkerFile.Exists) {
                //TODO:download from upstream
                Log.Debug("TODO:need to download from upstream. Not implemented");
            }
            return new List<DependencyWish>();
        }

        protected String GetRelPathFor(Dependency d) {
            var parts = new List<String>(3);
            parts.Add(String.Format("{0}\\{1}\\{2}", d.Group, d.Name, d.Version.ToString()));
            parts.Add(ToPathPart(d.Classifiers));
            parts.Add(d.Name + "." + d.Ext);
            return Path.Combine(parts.ToArray());
        }

        private String ToPathPart(Classifiers c) {
            var classifiers = c.ToList();
            classifiers.Sort();
            return String.Join("_", classifiers);
        }

        protected FileInfo GetFullPathFor(Dependency d) {
            return GetFullPathFor(GetRelPathFor(d));
        }

        protected FileInfo GetFullPathFor(String relPath){
            return new FileInfo(Path.Combine(CacheDir.FullName, relPath));
        }

    }
    class DependencyDto : Dependency {
        public List<DependencyWish> Wishes { get; set;}

        public DependencyDto(){
            Wishes = new List<DependencyWish>();
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
