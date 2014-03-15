using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NRequire.IO.Json;
using NRequire.Logging;
using NRequire.Model;
using NRequire.Resolver;
using NRequire.Util;

namespace NRequire.Cmd {

    internal class ProjectUpdateCommand {

        private Logger Log = Logger.GetLogger(typeof(ProjectUpdateCommand));

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

            //TODO:take into consideration all projects in the solution
            
            var soln = m_jsonReader.ReadSolution(LookupJsonFileForSolution(SolutionFile));
            var proj = m_jsonReader.ReadProject(LookupJsonFileForProject(ProjectFile));

            //all the stuff we need to meet all the requirements
            var projectDeps = ProjectResolver
                    .WithCache(LocalCache)
                    .MergeAndResolveDependencies(soln, proj);
            //merge the resolved deps with the additional wish settings like copyToDir, scope etc
            var wishesBySig = proj
                    .GetAllWishes()
                    .ToDictionary(w=>w.GetKey());

            var holders = new List<ResourceHolder>();
            foreach (var dep in projectDeps) {
                var holder = new ResourceHolder { Dep = dep }; 
                Wish wish;
                if (wishesBySig.TryGetValue(dep.GetKey(), out wish)) {
                    if (wish.Scope == Scopes.Provided) {
                        //skip,expect it to exist
                        continue;
                    }
                    holder.Wish = wish;
                }
                holder.Resources = SolutionCache.GetResourcesFor(dep);
                holders.Add(holder);
            }
            CopyRequired(holders);
            UpdateVSProject(holders);
        }

        public class ResourceHolder {
            public Wish Wish { get; set; }//what we want
            public Dependency Dep { get; set; }//what we actually resolve to
            public IList<Resource> Resources { get; set; }//actual stuff we end up needing
        }

        private static void CheckNotNotNull<T>(T val, String name) where T:class {
            if (val == null) {
                throw new NullReferenceException("Expect " + name + " to be set");
            }
        }

        //and wishes marked with a copyTo property will be copied into the appropriate destination 
        //else just referenced directly from the local cache
        private void CopyRequired(IList<ResourceHolder> holders) {
            foreach (var h in holders) {
                if (h.Wish != null && !String.IsNullOrEmpty(h.Wish.CopyTo)) {
                    Log.DebugFormat("For {0} copying resources to {1}", h.Wish.SafeToSummary(), h.Wish.CopyTo);
                    CopyResources(h.Resources, h.Wish.CopyTo);
                }
            }
        }

        private void CopyResources(IList<Resource>  resources, String path) {
            Log.DebugFormat("copying resources {0} to path {1}", resources.Count(), path);
            var projDir = ProjectFile.Directory;
            //TODO:look if absolute or relative?
            foreach (var r in resources) {
                var targetFile = new FileInfo(Path.Combine(projDir.FullName, path, r.File.Name));
                Log.TraceFormat("target file {0}", targetFile.FullName);
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

        //mysoln.sln.nrequire.json <--in case soln and proj with same name in same dir
        //mysoln.nrequire.json <--in case multiple soln's in same dir
        //solution.nrequire.json
        private FileInfo LookupJsonFileForSolution(FileInfo solnFile) {
            return LookupJsonFileFor(
                solnFile.Directory, 
                solnFile.Name + ".nrequire.json",
                FileNameMinusExtension(solnFile) + ".nrequire.json",
                "solution.nrequire.json"
            );
        }

        //myproj.csproj.nrequire.json
        //myproj.nrequire.json
        //project.nrequire.json
        private FileInfo LookupJsonFileForProject(FileInfo projFile) {
            return LookupJsonFileFor(
                projFile.Directory, 
                projFile.Name + ".nrequire.json",
                FileNameMinusExtension(projFile) + ".nrequire.json",
                "project.nrequire.json"
             );
        }

        private FileInfo LookupJsonFileFor(DirectoryInfo dir, params String[] names) {
            foreach (var name in names) {
                var fullpath = new FileInfo(Path.Combine(dir.FullName, name));
                if (fullpath.Exists) {
                    return fullpath;
                }
            }
            throw new ArgumentException(String.Format("Couldn't find any of [{0}]", String.Join(",",names)));

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
