using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace net.nrequire {
    class Program {

        private const String DEFAULT_CACHE_DIR_NAME = ".nrequire\\cache";

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

            DependencyCache localCache;
            if (result.HasOptionValue("--cache")) {
                var cacheDir = new DirectoryInfo(result.GetOptionValue("--cache"));
                localCache = new DependencyCache() {
                    VSProjectBaseSymbol = cacheDir.FullName,
                    CacheDir = cacheDir
                };
            } else {
                localCache = new DependencyCache() {
                    VSProjectBaseSymbol = "%HOMEDRIVE%%HOMEPATH%" + "\\" + DEFAULT_CACHE_DIR_NAME,
                    CacheDir = new DirectoryInfo(Path.Combine(userHomeDir,DEFAULT_CACHE_DIR_NAME))
                };
            }

            if (!localCache.CacheDir.Exists) {
                throw new ArgumentException(String.Format("Dependency cache dir '{0}' does not exist", localCache.CacheDir.FullName));
            }

            var solnDir = solutionFile.Directory;
            var solutionCache = new DependencyCache() {
                UpstreamCache = localCache,
                VSProjectBaseSymbol = "$(SolutionDir)\\.cache",
                CacheDir = new DirectoryInfo(Path.Combine(solnDir.FullName, ".cache"))
            };

            if (!solutionCache.CacheDir.Exists) {
                solutionCache.CacheDir.Create();
            }

            var resolver = new ProjectUpdater {
                FailOnProjectChanged = result.GetOptionValue("--fail", true) == "true",
                LocalCache = localCache,
                SolutionCache = solutionCache,
                ProjectFile = projectFile,
                SolutionFile = solutionFile
            };
            resolver.UpdateProject();
        }

        private class FailBuildException : Exception {
            internal FailBuildException(string msg)
                : base(msg) {
            }
        }
    }
}
