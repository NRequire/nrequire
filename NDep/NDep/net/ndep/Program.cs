using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace net.ndep {
    class Program {

        internal DependencyCache LocalCache { get; set; }
        internal FileInfo SolutionFile { get; set; }
        internal FileInfo ProjectFile { get; set; }

        private readonly Dependency DefaultDependencyValues = new Dependency { Ext = "dll",Arch="any", Runtime="any"};
        private readonly JsonReader m_depsReader = new JsonReader();

        static void Main(string[] args) {
            try {
                new Program().InvokeWithArgs(args);
                Environment.ExitCode = 0;
            } catch (Exception e) {
                Console.WriteLine("NDep error! :" + e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(GetParser().PrintHelp());
                Environment.ExitCode = -1;
            }
        }

        private static CommandLineParser GetParser() {
            return new CommandLineParser()
                .AddCommand("update-proj", "Update the project file with the latest resolved dependencies")
                .AddOptionWithValue("update-proj", "-soln", "(Required) Path to the solution file")
                .AddOptionWithValue("update-proj", "-proj", "(Required) Path to the project file")
                .AddOptionWithValue("update-proj", "-cache", "(Optional) Path to the local cache directory (Default is %HOMEDRIVE%%HOMEPATH%/.ndep/cache)")
            ;
        }

        public void InvokeWithArgs(string[] args) {
            var result = GetParser().Parse(args);

            var solutionFile = new FileInfo(result.GetOptionValue("-soln"));
            if (!solutionFile.Exists) {
                throw new ArgumentException(String.Format("Solution file '{0}' does not exist", solutionFile.FullName));
            }

            var projectFile = new FileInfo(result.GetOptionValue("-proj"));
            if (!projectFile.Exists) {
                throw new ArgumentException(String.Format("Project file '{0}' does not exist", projectFile.FullName));
            }
                
            var userHomeDir = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            if (!Directory.Exists(userHomeDir)) {
                throw new ArgumentException(String.Format("Could not find user home '{0}'", userHomeDir));
            }

            if (result.HasOptionValue("-cache")) {
                var cacheDir = new DirectoryInfo(result.GetOptionValue("-cache"));
                LocalCache = new DependencyCache() {
                    VSProjectBaseSymbol = cacheDir.FullName,
                    CacheDir = cacheDir
                };
            } else {
                LocalCache = new DependencyCache() {
                    VSProjectBaseSymbol = "%HOMEDRIVE%%HOMEPATH%",
                    CacheDir = new DirectoryInfo(Path.Combine(userHomeDir, ".ndep/cache"))
                };
            }

            if (!LocalCache.CacheDir.Exists) {
                throw new ArgumentException(String.Format("Dependency cache dir '{0}' does not exist", LocalCache.CacheDir.FullName));
            }
            SolutionFile = solutionFile;
            ProjectFile = projectFile;
            UpdateProject();
            
        }

        public void UpdateProject() {
            var d = ReadProjectDependency();
            UpdateReferences(d);
        }

        private Dependency ReadProjectDependency() {
            var solnDep = ReadDepFromJsonForFile(SolutionFile);
            var projDep = ReadDepFromJsonForFile(ProjectFile);
            var merged = projDep.MergeWithParent(solnDep);
            return merged;
        }

        public void UpdateReferences(Dependency projectDep) {
            var resources = ToResources(projectDep);
            EnsureResourceExist(resources);
            VSProject.FromPath(ProjectFile).WriteReferences(resources);
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

        private Dependency ReadDepFromJsonForFile(FileInfo file) {
            var jsonFile = new FileInfo(Path.Combine(file.DirectoryName,file.Name + ".ndep.json"));
            if (!jsonFile.Exists) {
                jsonFile = new FileInfo(Path.Combine(file.DirectoryName, "ndep.json"));
            }
            return m_depsReader.ReadDependency(jsonFile);
        }
    }
}
