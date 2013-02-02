using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace net.nrequire {
    class Program {
        private const String DEP_FILE = "nrequire.json"; 
        private const String DEFAULT_CACHE_DIR_NAME = ".nrequire\\cache";

        internal DependencyCache LocalCache { get; set; }
        internal FileInfo SolutionFile { get; set; }
        internal FileInfo ProjectFile { get; set; }
        internal bool FailOnProjectChanged { get; set; }

        private readonly Dependency DefaultDependencyValues = new Dependency { Ext = "dll",Arch="any", Runtime="any"};
        private readonly JsonReader m_depsReader = new JsonReader();

        static void Main(string[] args) {
            try {
                new Program().InvokeWithArgs(args);
                Environment.ExitCode = 0;
            } catch(FailBuildException e){
                Console.WriteLine(e.Message);
                Environment.ExitCode = -1;
            } catch (CommandParseException e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(GetParser().PrintHelp());
                Environment.ExitCode = -1;
            } catch (Exception e) {
                Console.WriteLine("Unhandled error :" + e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(GetParser().PrintHelp());
                Environment.ExitCode = -1;
            }
        }

        private static CommandLineParser GetParser() {
            return new CommandLineParser()
                .ProgramName("nrequire")
                .AddCommand("update-proj", "Update the project file with the latest resolved dependencies")
                    .AddOption("update-proj", Opt.Named("--soln").Required(true).Arg("filePath").Help("Path to the solution file"))
                    .AddOption("update-proj", Opt.Named("--proj").Required(true).Arg("filePath").Help("Path to the project file"))
                    .AddOption("update-proj", Opt
                        .Named("--cache")
                        .Required(false)
                        .Arg("dirPath")
                        .Help("Path to the local cache directory")
                        .Default("%HOMEDRIVE%%HOMEPATH%/" + DEFAULT_CACHE_DIR_NAME))
                    .AddOption("update-proj",Opt
                        .Named("--fail")
                        .Required(false)
                        .Arg("val")
                        .Default("true")
                        .Help("If true then fail the build after updating the project if the project dependencies changed"))
                    .AddExample("update-proj", "--soln ${SolutionPath} --proj ${ProjectPath}")
                    .AddExample("update-proj", "--soln ${SolutionPath} --proj ${ProjectPath} --cache C:/opt/cache")
                    .AddExample("update-proj", "--soln ${SolutionPath} --proj ${ProjectPath} --fail false")

                    .AddCommand("--help", "Print this help")
            ;
        }

        public void InvokeWithArgs(string[] args) {
            var result = GetParser().Parse(args);
            if (result.IsCommand("--help")) {
                throw new CommandParseException("");
            } else if( result.IsCommand("update-proj")) {
                UpdateProject(result);
            } else {
                throw new CommandParseException("Dont't recognize command:" + result.Command);
            }
        }

        private void UpdateProject(CommandLineParser.ParseResult result) {
            var solutionFile = new FileInfo(result.GetOptionValue("--soln"));
            if (!solutionFile.Exists) {
                throw new ArgumentException(String.Format("Solution file '{0}' does not exist", solutionFile.FullName));
            }

            var projectFile = new FileInfo(result.GetOptionValue("--proj"));
            if (!projectFile.Exists) {
                throw new ArgumentException(String.Format("Project file '{0}' does not exist", projectFile.FullName));
            }

            var userHomeDir = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            if (!Directory.Exists(userHomeDir)) {
                throw new ArgumentException(String.Format("Could not find user home '{0}'", userHomeDir));
            }

            if (result.HasOptionValue("--cache")) {
                var cacheDir = new DirectoryInfo(result.GetOptionValue("--cache"));
                LocalCache = new DependencyCache() {
                    VSProjectBaseSymbol = cacheDir.FullName,
                    CacheDir = cacheDir
                };
            } else {
                LocalCache = new DependencyCache() {
                    VSProjectBaseSymbol = "%HOMEDRIVE%%HOMEPATH%\\" + DEFAULT_CACHE_DIR_NAME,
                    CacheDir = new DirectoryInfo(Path.Combine(userHomeDir,DEFAULT_CACHE_DIR_NAME))
                };
            }

            if (!LocalCache.CacheDir.Exists) {
                throw new ArgumentException(String.Format("Dependency cache dir '{0}' does not exist", LocalCache.CacheDir.FullName));
            }

            FailOnProjectChanged = result.GetOptionValue("--fail", true) == "true";
            SolutionFile = solutionFile;
            ProjectFile = projectFile;
            UpdateProject();
        }

        public void UpdateProject() {
            var d = ReadProjectDependency();
            CopyRequired(d);
            UpdateReferences(d);
        }

        //and deps marked with a copyTo property will be copied into the appropriate destination 
        private void CopyRequired(Dependency dep) {
            foreach (var d in dep.Dependencies) {
                if(!String.IsNullOrEmpty(d.CopyTo)){
                    CopyDep(d);
                }
            }
        }

        private void CopyDep(Dependency d) {
            Resource resource = LocalCache.GetResourceFor(d);
            if (!resource.Exists) {
                throw new InvalidOperationException(String.Format("Could not find dependency '{0}'", resource.File.FullName));
            }
            var projDir = ProjectFile.Directory;
            var targetFile = new FileInfo(Path.Combine(projDir.FullName, d.CopyTo, resource.File.Name + "." + resource.File.Extension));
            
            if (!targetFile.Exists || targetFile.LastWriteTime != resource.File.LastWriteTime) {
                CopyFile(resource.File, targetFile);
            }
        }

        private static void CopyFile(FileInfo from, FileInfo to) {
            to.Directory.Create();

            using (var streamFrom = from.Open(FileMode.Open, FileAccess.Read))
            using (var streamTo = to.Open(FileMode.CreateNew, FileAccess.Write)) {
                CopyStream(streamFrom, streamTo);
            }

            to.CreationTime= from.CreationTime;
            to.LastWriteTime = from.LastWriteTime;
        }

        private static void CopyStream(FileStream streamFrom, FileStream streamTo) {
            var buf = new byte[2048];
            int numBytesRead;
            while ((numBytesRead = streamFrom.Read(buf, 0, buf.Length)) > 0) {
                streamTo.Write(buf, 0, numBytesRead);
            };
        }

        private Dependency ReadProjectDependency() {
            var solnDep = ReadDepFromJsonForFile(SolutionFile);
            var projDep = ReadDepFromJsonForFile(ProjectFile);
            var merged = projDep.MergeWithParent(solnDep);
            return merged;
        }

        public void UpdateReferences(Dependency projectDep) {
            var resources = ToResources(projectDep);
            EnsureResourcesExist(resources);

            var changed = VSProject
                .FromPath(ProjectFile)
                .UpdateReferences(resources);

            if (changed && FailOnProjectChanged) {
                throw new FailBuildException(String.Format("VS Project  '{0}' needed updating and fail enabled so stopping the build", ProjectFile.FullName));
            }
        }

        private IList<Resource> ToResources(Dependency dependency) {
            var resources = new List<Resource>();
            //now update the project!
            foreach (var child in dependency.Dependencies) {
                var merged = child.MergeWithParent(DefaultDependencyValues);
                merged.ValidateRequiredSet();
                Resource resource = LocalCache.GetResourceFor(merged);
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
                throw new InvalidOperationException(String.Format("Could not find dependencies [\n\t{0}\n\t]", String.Join<Resource>(",\n\t",notFound)));
            }
        }

        private Dependency ReadDepFromJsonForFile(FileInfo file) {
            var jsonFile = new FileInfo(Path.Combine(file.DirectoryName, file.Name + "." + DEP_FILE));
            if (!jsonFile.Exists) {
                jsonFile = new FileInfo(Path.Combine(file.DirectoryName, DEP_FILE));
            }
            return m_depsReader.ReadDependency(jsonFile);
        }

        private class FailBuildException : Exception {
            internal FailBuildException(string msg)
                : base(msg) {
            }
        }
    }
}
