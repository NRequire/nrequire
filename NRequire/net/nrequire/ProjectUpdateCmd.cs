using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace net.nrequire {

    internal class ProjectUpdateCommand {
        private const String DEP_FILE = "nrequire.json";

        private readonly JsonReader m_jsonReader = new JsonReader();

        internal DependencyCache LocalCache { get; set; }
        internal DependencyCache SolutionCache { get; set; }
        internal FileInfo SolutionFile { get; set; }
        internal FileInfo ProjectFile { get; set; }
        internal bool FailOnProjectChanged { get; set; }
       
        public void Invoke() {
            CheckNotNotNull(SolutionFile, "SolutionFile");
            CheckNotNotNull(ProjectFile, "ProjectFile");
            CheckNotNotNull(LocalCache, "LocalCache");
            CheckNotNotNull(SolutionCache, "SolutionCache");

            var deps = ResolveDependencies();
            var resources = ResolveResources(deps);
            var related = ResolveRelatedResources(deps);

            CopyRequired(resources);
            CopyRequired(related);

            UpdateVSProject(resources);
        }

        private static void CheckNotNotNull<T>(T val, String name) where T:class {
            if (val == null) {
                throw new NullReferenceException("Expect " + name + " to be set");
            }
        }

        //and deps marked with a copyTo property will be copied into the appropriate destination 
        private void CopyRequired(IEnumerable<Resource> resources) {
            foreach (var r in resources) {
                if (!String.IsNullOrEmpty(r.Dep.CopyTo)) {
                    CopyResource(r);
                }
            }
        }

        private void CopyResource(Resource resource) {
            var projDir = ProjectFile.Directory;
            //TODO:look if absolute or relative?
            var targetFile = new FileInfo(Path.Combine(projDir.FullName, resource.Dep.CopyTo, resource.File.Name));

            if (!targetFile.Exists || targetFile.LastWriteTime != resource.TimeStamp) {
                resource.CopyTo(targetFile);
            }
        }

        private IList<Dependency> ResolveDependencies() {
            var soln = m_jsonReader.ReadSolution(LookupJsonFileFor(SolutionFile));
            var proj = m_jsonReader.ReadProject(LookupJsonFileFor(ProjectFile));

            var deps = Resolver.WithCache(LocalCache).ResolveDependencies(soln, proj);
            return deps;
        }

        public void UpdateVSProject(IEnumerable<Resource> resources) {
            //some deps may be copied or available elsewhere and not needed for compilation
            var applyResources = resources.Where((r) => r.Dep.Scope != Scopes.Provided);

            var changed = VSProject
                .FromPath(ProjectFile)
                .UpdateReferences(applyResources);

            if (changed && FailOnProjectChanged) {
                throw new FailBuildException(String.Format("VS Project  '{0}' needed updating and fail enabled so stopping the build", ProjectFile.FullName));
            }
        }

        private IList<Resource> ResolveResources(IEnumerable<Dependency> deps) {
            var notFound = new List<Resource>();
            var resources = new List<Resource>();
            //now update the project!
            foreach (var d in deps) {
                var resource = SolutionCache.GetResourceFor(d);
                if (!resource.Exists) {
                    notFound.Add(resource);
                }
                resources.Add(resource);
            }
            if (notFound.Count > 0) {
                throw new InvalidOperationException(String.Format("Could not find dependencies [\n\t{0}\n\t]", String.Join<Resource>(",\n\t", notFound)));
            }
            return resources;
        }

        private IList<Resource> ResolveRelatedResources(IEnumerable<Dependency> deps) {
            var resources = new List<Resource>();
            foreach (var dep in deps) {
                if(dep.HasRelatedDependencies){
                    AddRelatedResources(dep, resources);
                }
            }
            return resources;
        }

        private void AddRelatedResources(Dependency dep, IList<Resource> addTo) {
            foreach (var related in dep.Related) {
                var resource = SolutionCache.GetResourceFor(related);
                if (resource.Exists) {
                    addTo.Add(resource);
                }
            }
        }

        private FileInfo LookupJsonFileFor(FileInfo file) {
            var jsonFileByName = new FileInfo(Path.Combine(file.DirectoryName, FileNameMinusExtension(file) + "." + DEP_FILE));
            if (jsonFileByName.Exists) {
                return jsonFileByName;
            }
            var jsonFile = new FileInfo(Path.Combine(file.DirectoryName, DEP_FILE));
            if (!jsonFile.Exists) {
                throw new ArgumentException(String.Format("Neither json file '{0}' or '{1}' exists", jsonFileByName.FullName, jsonFile.FullName));
            }
            return jsonFile;
        }

        private static String FileNameMinusExtension(FileInfo file) {
            var name = file.Name;
            var lastDot = name.IndexOf('.');
            if (lastDot > 0) {
                return name.Substring(0, lastDot);
            }
            return name;
        }

        private class FailBuildException : Exception {
            internal FailBuildException(string msg)
                : base(msg) {
            }
        }
    }
}
