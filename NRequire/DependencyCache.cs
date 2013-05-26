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
        IList<Resource> GetResourcesFor(IResolved d);

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
        IList<Wish> FindWishesFor(IResolved dep);
    }

    public class DependencyCache : IDependencyCache {

        private const String DefaultExt = "dll";

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
            Log.DebugFormat("find deps matching {0}", wish.SafeToSummary());
            //TODO:reverse this so deps are returned and versions extracted?
            var deps= FindAllVersionsFor(wish)
                .Where(v=>wish.Version==null||wish.Version.Match(v))
                .Select(v => ReadModuleForOrCreate(wish, v).ToDependency())
                .ToList();
            Log.Debug("found " + deps.Count);
            return deps;
        }

        private IList<Version> FindAllVersionsFor(IResolvable resolvable){
            Log.DebugFormat("finding versions for {0}", resolvable.SafeToSummary());

            var relPath = String.Format("{0}\\{1}", resolvable.Group, resolvable.Name);
            var topVersionsDir = new DirectoryInfo(Path.Combine(CacheDir.FullName,relPath));
            Log.TraceFormat("topVersionsDir '{0}'", topVersionsDir.FullName);
            if(!topVersionsDir.Exists){
                return new List<Version>();
            }
            //And what about empty?
            var classifierPathPart = ToPathPart(resolvable.Classifiers);
            //TODO:cache this?
            //.Where((versionDir) => Directory.GetDirectories(Path.Combine(dir.FullName,versionDir)).Contains(classifiers));
            //check each contains the matching classifiers
            if (Log.IsTraceEnabled()) {
                Log.TraceFormat("version dirs '{0}'", String.Join(",", topVersionsDir.EnumerateDirectories()));
                Log.TraceFormat("classifier parts '{0}'", classifierPathPart);
            }
            var versionDirs = (String.IsNullOrWhiteSpace(classifierPathPart) ? topVersionsDir.EnumerateDirectories() : topVersionsDir.DirectoriesWithSubDirsNamed(classifierPathPart)).ToList();

            if (Log.IsTraceEnabled()) {
                Log.TraceFormat("version dirs found (with classifier dirs in them) were [{0}]", String.Join(",", versionDirs.Select(d=>d.Name)));
            }

            var versions = versionDirs
                .Select((versionDir) => Version.Parse(versionDir.Name))
                .ToList();

            versions.Sort();
            versions.Reverse();

            Log.Debug("found " + versions.Count);
            if (Log.IsTraceEnabled()) {
                Log.TraceFormat("Versions found for {0} and classifier '{1}' were {2}", resolvable.SafeToSummary(), resolvable.Classifiers, String.Join(",", versions));
            }
            return versions;
        }

        public IList<Resource> GetResourcesFor(IResolved resolved) {
            Log.DebugFormat("Finding resources for {0}", resolved.SafeToSummary());
            var resources = new List<Resource>();
            var relPathMinusExt = GetRelPathFor(resolved);
            var depPath = GetFullPathFor(relPathMinusExt + "." + ExtOrDefault(resolved.Ext));
            Log.TraceFormat("checking dep is locally copied, file={0}", depPath);
            //download from upstream
            if (!depPath.Exists) {
                if (UpstreamCache != null) {
                    var upstreamResources = UpstreamCache.GetResourcesFor(resolved);
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
                    throw new Exception(String.Format("couldn't find resources for {0} looked in {1}", resolved.SafeToSummary(), relPathMinusExt));
                }
            }
            //TODO:ad more error checking around here to fail if expected resources don't exist (e.g. the dll)
           
            var extensions = FindExtensionsFor(resolved);
            if( Log.IsTraceEnabled()){
                Log.TraceFormat("Found extensions [{0}]", String.Join(",",extensions));
            }
            foreach (var ext in extensions) {
                var resourceRelPath = relPathMinusExt + "." + ext;
                var resourcePath = GetFullPathFor(resourceRelPath);
                //TODO:if SNAPSHOT,then check localcache timestamp
                if (resourcePath.Exists) {
                    resources.Add(new Resource(resolved, resourcePath, VSProjectBaseSymbol + "\\" + resourceRelPath));
                }
            }
            if( Log.IsTraceEnabled()){
                Log.TraceFormat("Found resources: {0}", String.Join("\n",resources));
            }

            Log.DebugFormat("Found {0} resources", resources.Count);
            return resources;
        }

        private List<String> FindExtensionsFor(IResolved resolved) {
            var relatedExtensions = new List<String>();
            var ext = ExtOrDefault(resolved.Ext);

            if (DefaultRelatedByExt.ContainsKey(ext)) {
                relatedExtensions.AddRange(DefaultRelatedByExt[ext]);
            }
            if (!relatedExtensions.Contains(ext)) {
                relatedExtensions.Add(ext);
            }
            //TODO:need to grab this from the modules file at some point
/*            if (dep.Related.Count > 0) {

            }*/
            return relatedExtensions;
        }

        private static String ExtOrDefault(String ext) {
            return String.IsNullOrWhiteSpace(ext) ? DefaultExt : ext; 
        }

        public IList<Wish> FindWishesFor(IResolved resolved) {
            Log.Debug("Finding wishes for " + resolved.SafeToSummary());
            var module = ReadModuleForOrCreate(resolved);
            var wishes = module.GetWishes();
            Log.Debug("found " + wishes.Count);
            return wishes;
        }
        
        private Module ReadModuleForOrCreate(IResolved resolved) {
            return ReadModuleForOrCreate(resolved, resolved.Version);
        }

        private Module ReadModuleForOrCreate(IResolvable resolvable, Version v) {
            Module module;
            var file = GetFullPathFor(GetRelPathFor(resolvable, v) + ".nrequire.module.json");
            Log.TraceFormat("looking for module file {0}", file.FullName);
            if (file.Exists) {
                module = m_reader.Read<Module>(file);
                module.Version = v;
                Log.TraceFormat("Found module {0}", module);
            } else {
                module = new Module {
                    Group = resolvable.Group,
                    Name = resolvable.Name,
                    Classifiers = resolvable.Classifiers.Clone(),
                    Version = v
                };
            }
            return module;
        }

        internal String GetRelPathWithExtFor(IResolved resolved) {
            return GetRelPathFor(resolved) + "." + ExtOrDefault(resolved.Ext);
        }

        internal String GetRelPathFor(IResolved resolved) {
            return GetRelPathFor(resolved, resolved.Version);
        }

        internal String GetRelPathFor(IResolvable resolvable, Version v) {
            PreConditions.NotBlank(resolvable.Group, "Group", ()=>"dep=" + resolvable);
            PreConditions.NotBlank(resolvable.Name, "Name", ()=>"dep=" + resolvable);
            var parts = new List<String>(3);
            parts.Add(String.Format("{0}\\{1}\\{2}", resolvable.Group, resolvable.Name, v.ToString()));
            parts.Add(ToPathPart(resolvable.Classifiers));
            parts.Add(resolvable.Name);
            return Path.Combine(parts.ToArray());
        }

        private String ToPathPart(Classifiers c) {
            var classifiers = c.ToList();
            classifiers.Sort();
            return String.Join("_", classifiers);
        }

        internal FileInfo GetFullPathFor(IResolved d) {
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
