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

            var deps = ReadProjectDependencies();
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

        private IList<Dependency> ReadProjectDependencies() {
            var soln = m_jsonReader.ReadSolution(LookupJsonFileFor(SolutionFile));
            soln.ApplyDefaults();
            var proj = m_jsonReader.ReadProject(LookupJsonFileFor(ProjectFile));
            proj.ApplySolution(soln);
            proj.ApplyDefaults(); 
            proj.ValidateDependenciesSet();

            return proj.GetResolvedDependencies();
        }

        public void UpdateVSProject(IEnumerable<Resource> resources) {
            var changed = VSProject
                .FromPath(ProjectFile)
                .UpdateReferences(resources);

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
                if( dep.HasRelatedDependencies()){
                    resources.AddRange(ResolveRelatedResources(dep));
                }
            }
            return resources;
        }

        private IList<Resource> ResolveRelatedResources(Dependency dep) {
            var resources = new List<Resource>();
            var related = dep.GetRelatedDependencies();
            foreach (var d in related) {
                var resource = SolutionCache.GetResourceFor(d);
                if (resource.Exists) {
                    resources.Add(resource);
                }
            }
            return resources;
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
