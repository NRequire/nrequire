using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NRequire {

    internal class ProjectUpdateCommand {

        private readonly JsonReader m_jsonReader = new JsonReader();

        internal IDependencyCache LocalCache { get; set; }
        internal IDependencyCache SolutionCache { get; set; }
        internal FileInfo SolutionFile { get; set; }
        internal FileInfo ProjectFile { get; set; }
        internal bool FailOnProjectChanged { get; set; }
       
        public void Invoke() {
            CheckNotNotNull(SolutionFile, "SolutionFile");
            CheckNotNotNull(ProjectFile, "ProjectFile");
            CheckNotNotNull(LocalCache, "LocalCache");
            CheckNotNotNull(SolutionCache, "SolutionCache");

            //TODO:merge the resolved deps with the wishes again, as we need to pull
            //additional info from the wishes like where the user wants to copy things across too
            //etc
            var soln = m_jsonReader.ReadSolution(LookupJsonFileForSolution(SolutionFile));
            var proj = m_jsonReader.ReadProject(LookupJsonFileForProject(ProjectFile));

            //all the stuff we need to meet all the requirements
            var deps = ProjectResolver
                    .WithCache(LocalCache)
                    .MergeAndResolveDependencies(soln, proj);
            //merge the reolved deps with the additional wish settings like copyToDir, scope etc
            var wishesBySig = proj
                    .GetAllWishes()
                    .ToDictionary(w=>w.GetKey());

            var holders = new List<ResourceHolder>();
            foreach (var d in deps) {
                var holder = new ResourceHolder {
                    Dep = d
                }; 
                Wish wish;
                if (wishesBySig.TryGetValue(d.GetKey(), out wish)) {
                    if (wish.Scope == Scopes.Provided) {
                        //skip,expect it to exist
                        continue;
                    }
                    holder.Wish = wish;
                }
                holder.Resources = SolutionCache.GetResourcesFor(d);
                holders.Add(holder);
            }
            CopyRequired(holders);
            UpdateVSProject(holders);
        }

        public class ResourceHolder {
            public Wish Wish { get; set; }
            public Dependency Dep { get; set; }
            public IList<Resource> Resources { get; set; }
        }

        private static void CheckNotNotNull<T>(T val, String name) where T:class {
            if (val == null) {
                throw new NullReferenceException("Expect " + name + " to be set");
            }
        }

        //and wishes marked with a copyTo property will be copied into the appropriate destination 
        //else just referenced directly from the local cache
        private void CopyRequired(IEnumerable<ResourceHolder> holders) {
            foreach (var h in holders) {
                if (h.Wish != null && !String.IsNullOrEmpty(h.Wish.CopyTo)) {
                    CopyResources(h.Resources, h.Wish.CopyTo);
                }
            }
        }

        private void CopyResources(IEnumerable<Resource>  resources, String path) {
            var projDir = ProjectFile.Directory;
            //TODO:look if absolute or relative?
            foreach (var r in resources) {
                var targetFile = new FileInfo(Path.Combine(projDir.FullName, path, r.File.Name));
                if (!targetFile.Exists || targetFile.LastWriteTime != r.TimeStamp) {
                    r.CopyTo(targetFile);
                }
            }
        }

        private void UpdateVSProject(IEnumerable<ResourceHolder> holders) {
            var refs = new List<VSProject.Reference>();
            foreach (var h in holders) {
                foreach (var r in h.Resources) {
                    if( !r.IsType("pdb") && !r.IsType("xml")){
                        var reference = new VSProject.Reference {
                            Include = h.Dep.Name,
                            HintPath = r.VSProjectPath,
                            EmbeddedResource = h.Wish!=null &&h.Wish.CopyTo !=null
                        };
                        refs.Add(reference);
                    }
                }
            }

            var changed = VSProject
                .FromPath(ProjectFile)
                .UpdateReferences(refs);

            if (changed && FailOnProjectChanged) {
                throw new FailBuildException(String.Format("VS Project  '{0}' needed updating and fail enabled so stopping the build", ProjectFile.FullName));
            }
        }

        private FileInfo LookupJsonFileForSolution(FileInfo file) {
            return LookupJsonFileFor(file, "solution");
        }
        private FileInfo LookupJsonFileForProject(FileInfo file) {
            return LookupJsonFileFor(file, "project");
        }

        private FileInfo LookupJsonFileFor(FileInfo file, String typeName) {
            var jsonFileByName = new FileInfo(Path.Combine(file.DirectoryName, FileNameMinusExtension(file) + ".nrequire." + typeName + ".json"));
            if (jsonFileByName.Exists) {
                return jsonFileByName;
            }
            var jsonFile = new FileInfo(Path.Combine(file.DirectoryName, "nrequire." + typeName + ".json"));
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
