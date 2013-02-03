using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace net.nrequire {
    internal class ProjectUpdater {
        private const String DEP_FILE = "nrequire.json";

        internal DependencyCache LocalCache { get; set; }
        internal DependencyCache SolutionCache { get; set; }

        internal FileInfo SolutionFile { get; set; }
        internal FileInfo ProjectFile { get; set; }
        internal bool FailOnProjectChanged { get; set; }

        private readonly Dependency DefaultDependencyValues = new Dependency { Ext = "dll", Arch = "any", Runtime = "any" };
        private readonly JsonReader m_depsReader = new JsonReader();

        public void UpdateProject() {
            var deps = ReadProjectDependency();
            CopyRequired(deps);
            UpdateProjectReferences(deps);
        }

        //and deps marked with a copyTo property will be copied into the appropriate destination 
        private void CopyRequired(IEnumerable<Dependency> deps) {
            foreach (var d in deps) {
                if (!String.IsNullOrEmpty(d.CopyTo)) {
                    CopyDep(d);
                }
            }
        }

        private void CopyDep(Dependency d) {
            Resource resource = SolutionCache.GetResourceFor(d);
            if (!resource.Exists) {
                throw new InvalidOperationException(String.Format("Could not find dependency '{0}'", resource.File.FullName));
            }
            var projDir = ProjectFile.Directory;
            //TODO:look if absolute or relative?
            var targetFile = new FileInfo(Path.Combine(projDir.FullName, d.CopyTo, resource.File.Name + "." + resource.File.Extension));

            if (!targetFile.Exists || targetFile.LastWriteTime != resource.TimeStamp) {
                resource.CopyTo(targetFile);
            }
        }

        private IList<Dependency> ReadProjectDependency() {
            var soln = m_depsReader.ReadSolution(FindJsonFileFor(SolutionFile));
            var proj = m_depsReader.ReadProject(FindJsonFileFor(ProjectFile));
            var mergedProj = proj.MergeWith(soln);
            return mergedProj.Dependencies;
        }

        public void UpdateProjectReferences(IEnumerable<Dependency> deps) {
            var resources = ToResources(deps);
            EnsureResourcesExist(resources);

            var changed = VSProject
                .FromPath(ProjectFile)
                .UpdateReferences(resources);

            if (changed && FailOnProjectChanged) {
                throw new FailBuildException(String.Format("VS Project  '{0}' needed updating and fail enabled so stopping the build", ProjectFile.FullName));
            }
        }

        private IList<Resource> ToResources(IEnumerable<Dependency> deps) {
            var resources = new List<Resource>();
            //now update the project!
            foreach (var child in deps) {
                var merged = child.MergeWithParent(DefaultDependencyValues);
                merged.ValidateRequiredSet();
                Resource resource = SolutionCache.GetResourceFor(merged);
                resources.Add(resource);
            }
            return resources;
        }

        private void EnsureResourcesExist(IEnumerable<Resource> resources) {
            var notFound = new List<Resource>();
            foreach (var resource in resources) {
                if (!resource.Exists) {
                    notFound.Add(resource);
                }
            }
            if (notFound.Count > 0) {
                throw new InvalidOperationException(String.Format("Could not find dependencies [\n\t{0}\n\t]", String.Join<Resource>(",\n\t", notFound)));
            }
        }

        private FileInfo FindJsonFileFor(FileInfo file) {
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
