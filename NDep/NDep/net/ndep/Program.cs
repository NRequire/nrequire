using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace net.ndep {
    class Program {

        private DependencyCache LocalCache { get; set; }
        private DirectoryInfo SolutionRootDir { get; set; }
        private FileInfo ProjectFile { get; set; }

        private readonly Dependency DefaultDependencyValues = new Dependency { Ext = "dll",Arch="any", Runtime="any"};
        private readonly JsonReader m_depsReader = new JsonReader();

        static void Main(string[] args) {
            try {
                Console.WriteLine("NDep !!!");
                new Program().InvokeWithArgs(args);
                Environment.ExitCode = 0;
            } catch (Exception e) {
                Console.WriteLine("NDep error! :" + e.Message);
                Console.WriteLine(e.StackTrace);
                Environment.ExitCode = -1;
            }
        }

        public void InvokeWithArgs(string[] args) {
            if (args.Length < 2) {
                throw new ArgumentException("No project file to update");
            } else {
                var solutionDir = new DirectoryInfo(args[0]);
                if (!solutionDir.Exists) {
                    throw new ArgumentException(String.Format("Solution file '{0}' does not exist", solutionDir.FullName));
                }

                var projectFile = new FileInfo(args[1]);
                if (!projectFile.Exists) {
                    throw new ArgumentException(String.Format("Project file '{0}' does not exist", projectFile.FullName));
                }

                var userHomeDir = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
                if (!Directory.Exists(userHomeDir)) {
                    throw new ArgumentException(String.Format("Could not find user home '{0}'", userHomeDir));
                }

                var localCacheDir = new DirectoryInfo(Path.Combine(userHomeDir, ".ndep/cache"));
                if (!localCacheDir.Exists) {
                    throw new ArgumentException(String.Format("Dependency cache dir '{0}' does not exist", localCacheDir.FullName));
                }

                LocalCache = new DependencyCache() {
                    VSProjectBaseSymbol = "%HOMEDRIVE%%HOMEPATH%",
                    CacheDir = localCacheDir
                };
                SolutionRootDir = solutionDir;
                ProjectFile = projectFile;

                UpdateProject();
            }
        }

        public void UpdateProject() {
            var d = ReadProjectDependency();
            UpdateReferences(d);
        }

        private Dependency ReadProjectDependency() {
            var solnDep = ReadDepFromJsonInDir(SolutionRootDir);
            Console.WriteLine("soln dep:" + solnDep);
            var projDep = ReadDepFromJsonInDir(Directory.GetParent(ProjectFile.FullName));
            Console.WriteLine("proj dep:" + projDep);
            var merged = projDep.MergeWithParent(solnDep);
            Console.WriteLine("merged soln and proj dep:" + merged);
            return merged;
        }

        public void UpdateReferences(Dependency projectDep) {
            var resources = ToResources(projectDep);
            EnsureResourceExist(resources);
            new VSProject(ProjectFile).WriteReferences(resources);
        }

        private IList<Resource> ToResources(Dependency dependency) {
            var resources = new List<Resource>();
            //now update the project!
            foreach (var child in dependency.Dependencies) {
                var merged = child.MergeWithParent(DefaultDependencyValues);
                Console.WriteLine("validating:" + merged);
                merged.ValidateRequiredSet();
                Resource resource = LocalCache.GetResourceFor(merged);
                Console.WriteLine("dependency:" + resource.VSProjectPath);
                resources.Add(resource);
            }
            return resources;
        }

        private void EnsureResourceExist(IEnumerable<Resource> resources) {
            var notFound = new List<Resource>();
            foreach (var resource in resources) {
                if (!resource.Exists) {
                    notFound.Add(resource);
                }
            }
            if (notFound.Count > 0) {
                throw new InvalidOperationException(String.Format("Could not find dependencies [\n\t{0}\n\t]", String.Join<Resource>(",\n\t",notFound)));
            }
        }

        private Dependency ReadDepFromJsonInDir(DirectoryInfo dir) {
            var jsonFile = new FileInfo(Path.Combine(dir.FullName, "ndep.json"));
            return m_depsReader.ReadDependency(jsonFile);
            
            //return new Dependency();
        }
    }
}
