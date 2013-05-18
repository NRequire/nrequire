using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NRequire.Lang;

namespace NRequire {

    public interface IDependencyCache {

        /// <summary>
        /// Return all the resources for teh given dependency. This can be dll's, pdb, exe other etc. This does _not_ resolve
        /// required deps only the immediate one
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        IList<Resource> GetResourcesFor(Dependency d);

        /// <summary>
        /// Find all the dependencies which match the given wish.
        /// </summary>
        /// <param name="wish"></param>
        /// <returns>a modifiable list of deps which are free to be modified (changes will not be reflected in the cache)</returns>
        IList<Dependency> FindDependenciesMatching(Wish wish);

        /// <summary>
        /// Return all the wishes for the given dependency including transitive, runtime, provided, optional. Up to callers to pick the ones they need
        /// </summary>
        /// <param name="dep"></param>
        /// <returns>a modifiable list of wishes which are free to be modified (changes will not be reflected in the cache)</returns>
        IList<Wish> FindWishesFor(Dependency dep);
    }

    public class DependencyCache : IDependencyCache {

        private static readonly IDictionary<String, IList<String>> DefaultRelatedByExt = new Dictionary<String, IList<String>>{
            { "dll", new List<String>{ "xml", "pdb" }},
            { "exe", new List<String>{ "xml", "pdb" }}
        };

        private ISource Source { get; set; }
        public DependencyCache UpstreamCache { get; private set; }
        public DirectoryInfo CacheDir { get; private set; }
        public String VSProjectBaseSymbol { get; private set; }

        private readonly Logger Log = Logger.GetLogger(typeof(DependencyCache));
        private readonly JsonReader m_reader = new JsonReader();

        public static Builder With(){
            return new Builder();
        }

        private DependencyCache(DirectoryInfo cacheDir, String vsProjectBaseSymbol, DependencyCache upstreamCache) {
            CacheDir = cacheDir;
            VSProjectBaseSymbol = vsProjectBaseSymbol;
            UpstreamCache = upstreamCache;
            Source = SourceLocations.FromName(CacheDir.FullName);
        }

        public IList<Dependency> FindDependenciesMatching(Wish wish){
            Log.DebugFormat("find deps matching '{0}'", wish);
            //TODO:reverse this so deps are returned and versions extracted?
            var deps= FindAllVersionsFor(wish)
                .Where(v=>wish.Version==null||wish.Version.Match(v))
                .Select(v => new Dependency {
                    Group = wish.Group,
                    Name = wish.Name,
                    Ext = wish.Ext,
                    Classifiers = wish.Classifiers.Clone(),
                    Version = v,
                    Source = new SourceLocations(Source).Add(wish.Source),
                    CopyTo = wish.CopyTo,
                    //Related = wish.Related,
                    Url = wish.Url
                    
            }).ToList();
            Log.Debug("found " + deps.Count);
            return deps;
        }

        private IList<Version> FindAllVersionsFor(AbstractDependency dep){
            Log.DebugFormat("finding versions for '{0}'", dep);

            var relPath = String.Format("{0}\\{1}", dep.Group, dep.Name);
            var topVersionsDir = new DirectoryInfo(Path.Combine(CacheDir.FullName,relPath));
            Log.TraceFormat("topVersionsDir '{0}'", topVersionsDir.FullName);
            if(!topVersionsDir.Exists){
                return new List<Version>();
            }
            //And what about empty?
            var classifierPathPart = ToPathPart(dep.Classifiers);
            //TODO:cache this?
            //.Where((versionDir) => Directory.GetDirectories(Path.Combine(dir.FullName,versionDir)).Contains(classifiers));
            //check each contains the matching classifiers
            if (Log.IsTraceEnabled()) {
                Log.TraceFormat("version dirs '{0}'", String.Join(",", topVersionsDir.EnumerateDirectories()));
                Log.TraceFormat("classifier parts '{0}'", classifierPathPart);
            }
            var versionDirs = (String.IsNullOrWhiteSpace(classifierPathPart) ? topVersionsDir.EnumerateDirectories() : topVersionsDir.DirectoriesWithSubDirsNamed(classifierPathPart)).ToList();

            if (Log.IsTraceEnabled()) {
                Log.TraceFormat("version dirs found were [{0}]", String.Join(",", versionDirs.Select(d=>d.Name)));
            }

            var versions = versionDirs
                .Select((versionDir) => Version.Parse(versionDir.Name))
                .ToList();

            versions.Sort();
            versions.Reverse();

            Log.Debug("found " + versions.Count);
            if (Log.IsTraceEnabled()) {
                Log.TraceFormat("Versions found for dep {0} and classifier '{1}' were {2}", dep, dep.Classifiers, String.Join(",", versions));
            }
            return versions;
        }

        public IList<Resource> GetResourcesFor(Dependency dep) {
            Log.DebugFormat("Finding resources for dep {0}", dep);
            var resources = new List<Resource>();
            var relPathMinusExt = GetRelPathFor(dep);
            var depPath = GetFullPathFor(relPathMinusExt + "." + dep.Ext);
            Log.TraceFormat("checking dep is locally copied, file={0}", depPath);
            //download from upstream
            if (!depPath.Exists) {
                if (UpstreamCache != null) {
                    var upstreamResources = UpstreamCache.GetResourcesFor(dep);
                    foreach (var r in upstreamResources) {
                        var localResourcePath = GetFullPathFor(relPathMinusExt + "." + r.Type);
                        Log.TraceFormat("localResourcePath '{0}'", localResourcePath);
                        if(localResourcePath.Exists){
                            //TODO:only if a dynamic version (aka a SNAPSHOT)
                            localResourcePath.Delete();//might be newer?
                        }
                        r.CopyTo(localResourcePath);
                    }
                } else {
                    throw new Exception("couldn't find resources for dep : " + dep);
                }
            }
            //TODO:ad more error checking around here to fail if expected resources don't exist (e.g. the dll)
           
            var extensions = FindExtensionsFor(dep);
            if( Log.IsTraceEnabled()){
                Log.TraceFormat("Found extensions [{0}]", String.Join(",",extensions));
            }
            foreach (var ext in extensions) {
                var resourceRelPath = relPathMinusExt + "." + ext;
                var resourcePath = GetFullPathFor(resourceRelPath);
                //TODO:if SNAPSHOT,then check localcache timestamp
                if (resourcePath.Exists) {
                    resources.Add(new Resource(dep, resourcePath, VSProjectBaseSymbol + "\\" + resourceRelPath));
                }
            }
            if( Log.IsTraceEnabled()){
                Log.TraceFormat("Found resources: {0}", String.Join("\n",resources));
            }

            Log.DebugFormat("Found {0} resources", resources.Count);
            return resources;
        }

        private List<String> FindExtensionsFor(Dependency dep){
            var relatedExtensions = new List<String>();
            if (DefaultRelatedByExt.ContainsKey(dep.Ext)) {
                relatedExtensions.AddRange(DefaultRelatedByExt[dep.Ext]);
            }
            if (!relatedExtensions.Contains(dep.Ext)) {
                relatedExtensions.Add(dep.Ext);
            }
            //TODO:need to grab this from the modules file at some point
/*            if (dep.Related.Count > 0) {

            }*/
            return relatedExtensions;
        }

        public IList<Wish> FindWishesFor(Dependency dep){
            Log.Debug("Finding wishes for " + dep);
            var file = GetFullPathFor(GetRelPathFor(dep) + ".nrequire.module.json");
            Log.TraceFormat("looking for module file {0}", file.FullName);
            var wishes = new List<Wish>();
            if(file.Exists){
                var module = m_reader.Read<Module>(file);
                wishes.AddRange(module.RuntimeWishes);
                wishes.AddRange(module.OptionalWishes);
                wishes.AddRange(module.TransitiveWishes);

                Log.TraceFormat("Found module {0}", module);
            }
            Log.Debug("found " + wishes.Count);
            return wishes;
        }

        internal String GetRelPathWithExtFor(Dependency d) {
            return GetRelPathWithExtFor(d,d.Version);
        }

        internal String GetRelPathWithExtFor(AbstractDependency d,Version v) {
            PreConditions.NotBlank(d.Ext, "Extention", ()=>"dep=" + d);
            return GetRelPathFor(d, v) + "." + d.Ext;
        }

        internal String GetRelPathFor(Dependency d) {
            return GetRelPathFor(d,d.Version);
        }

        internal String GetRelPathFor(AbstractDependency d, Version v) {
            PreConditions.NotBlank(d.Group, "Group", ()=>"dep=" + d);
            PreConditions.NotBlank(d.Name, "Name", ()=>"dep=" + d);
            var parts = new List<String>(3);
            parts.Add(String.Format("{0}\\{1}\\{2}", d.Group, d.Name, v.ToString()));
            parts.Add(ToPathPart(d.Classifiers));
            parts.Add(d.Name);
            return Path.Combine(parts.ToArray());
        }

        private String ToPathPart(Classifiers c) {
            var classifiers = c.ToList();
            classifiers.Sort();
            return String.Join("_", classifiers);
        }

        internal FileInfo GetFullPathFor(Dependency d) {
            return GetFullPathFor(GetRelPathWithExtFor(d));
        }

        internal FileInfo GetFullPathFor(String relPath) {
            return new FileInfo(Path.Combine(CacheDir.FullName, relPath));
        }

        public class Builder : IBuilder<DependencyCache> {
            public DependencyCache UpstreamCache { get; set; }
            public DirectoryInfo CacheDir { get; set; }
            public String VSProjectBaseSymbol { get; set; }

            public DependencyCache Build() {
                return new DependencyCache(CacheDir, VSProjectBaseSymbol, UpstreamCache);
            }
        }

    }

    static class DirectoryInfoExtenions {

        public static IEnumerable<DirectoryInfo> DirectoriesWithSubDirsNamed(this DirectoryInfo dir, String subDirName) {
            var lowerSubDirName = subDirName.ToLowerInvariant();
            return dir.EnumerateDirectories()
                .Where((child) => child.ContainsDirNamed(lowerSubDirName));
        }
        
        
        public static bool ContainsDirNamed(this DirectoryInfo dir, String subDirName) {
            return dir.EnumerateDirectories()
                    .Where((child) => child.Name.ToLowerInvariant().Equals(subDirName))
                    .Count() > 0;
        }


    }
}
