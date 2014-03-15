using System;
using System.IO;
using NRequire.Cmd;
using NRequire.IO;
using NRequire.IO.Json;
using NRequire.Logging;
using NRequire.Model;
using NRequire.Util;

namespace NRequire {
    class Program {

        private const String DEFAULT_CACHE_DIR_NAME = ".nrequire\\cache";

        static Program() {
            LoadEmbeddedDlls.Load();
        }

        static void Main(string[] args) {
            try {
                new Program().InvokeWithArgs(args);
                Environment.ExitCode = 0;
            } catch(FailBuildException e){
                PrintError(e);
                Environment.ExitCode = -1;
            } catch (CommandLineParseException e) {
                PrintError(e);
                Console.WriteLine(GetParser().PrintHelp(args));
                Environment.ExitCode = -1;
            } catch (Exception e) {
                PrintError(e, stacktrace:true);
                Console.WriteLine(GetParser().PrintHelp(args));
                Environment.ExitCode = -1;
            }
        }

        private static void PrintError(Exception e, bool stacktrace= false)
        {
            var root = e;
            while( e != null)
            {
                Console.Error.WriteLine(e.Message);
                e = e.InnerException;
            }
            if (stacktrace)
            {
                Console.Error.WriteLine(root.StackTrace);
            }
        }
        private static CommandLineParser GetParser() {
            return new CommandLineParser()
                .ProgramName("nrequire")
                .AddCommand("--help", "Print this help")
                .AddCommand("update-vsproj", "Update the Visual Studio project file with the latest resolved dependencies")
                    .AddOption("update-vsproj", Opt.Named("--soln").Required(true).Arg("filePath").Help("Path to the solution file"))
                    .AddOption("update-vsproj", Opt.Named("--proj").Required(true).Arg("filePath").Help("Path to the project file"))
                    .AddOption("update-vsproj", Opt
                        .Named("--cache")
                        .Required(false)
                        .Arg("dirPath")
                        .Help("Path to the local cache directory.")
                        .Help("This is the central location where all the dependencies are looked up from")
                        .Default("%HOMEDRIVE%%HOMEPATH%\\{0} (currently {1}\\{2})", DEFAULT_CACHE_DIR_NAME, GetUserHomeDir(), DEFAULT_CACHE_DIR_NAME ))
                    .AddOption("update-vsproj", Opt
                        .Named("--soln-cache")
                        .Required(false)
                        .Arg("dirPath")
                        .Help("Path to the solution cache cache directory.")
                        .Help("This is where all the dependencies are copied to and referenced from within the VS project")
                        .Default("$(solutionDir)\\.cache"))
                    .AddOption("update-vsproj", Opt
                        .Named("--fail")
                        .Required(false)
                        .Arg("val")
                        .Default("true")
                        .Help("If true then fail the build after6 updating the project if the project dependencies changed"))
                     .AddOption("update-vsproj", Opt
                        .Named("--log")
                        .Required(false)
                        .Arg("val")
                        .Default("warn")
                        .Help("Sets the logging level. One of trace,debug,info,warn,error,off. Case insensitive"))
                    .AddExample("update-vsproj", "--soln ${SolutionPath} --proj ${ProjectPath}")
                    .AddExample("update-vsproj", "--soln ${SolutionPath} --proj ${ProjectPath} --cache C:/opt/cache")
                    .AddExample("update-vsproj", "--soln ${SolutionPath} --proj ${ProjectPath} --fail false")
            ;
        }

        public void InvokeWithArgs(string[] args) {
            var result = GetParser().Parse(args);
            if (result.IsCommand("--help")) {
                throw new CommandLineParseException("Help");
            } else if(result.IsCommand("update-vsproj")) {
                UpdateProjectCmd(result);
            } else {
                throw new CommandLineParseException("Don't recognize command:" + result.Command);
            }
        }

        private void UpdateProjectCmd(CommandLineParser.ParseResult result) {
            var logLevel = result.GetOptionValueOrDefault("--log", "warn");
            Logger.SetLevel(logLevel);

            var solutionFile = new FileInfo(result.GetOptionValue("--soln"));
            if (!solutionFile.Exists) {
                throw new ArgumentException(String.Format("Solution file '{0}' does not exist", solutionFile.FullName));
            }

            var projectFile = new FileInfo(result.GetOptionValue("--proj"));
            if (!projectFile.Exists) {
                throw new ArgumentException(String.Format("Project file '{0}' does not exist", projectFile.FullName));
            }

            var userHomeDir = GetUserHomeDir();
            if (!Directory.Exists(userHomeDir)) {
                throw new ArgumentException(String.Format("Could not find user home '{0}'", userHomeDir));
            }

            DependencyCache localCache;
            if (result.HasOptionValue("--cache")) {
                var cacheDir = new DirectoryInfo(result.GetOptionValue("--cache"));
                localCache = new DependencyCache.Builder() {
                    VSProjectBaseSymbol = cacheDir.FullName,
                    CacheDir = cacheDir
                }.Build();
            } else {
                localCache = new DependencyCache.Builder() {
                    VSProjectBaseSymbol = "%HOMEDRIVE%%HOMEPATH%" + "\\" + DEFAULT_CACHE_DIR_NAME,
                    CacheDir = new DirectoryInfo(Path.Combine(userHomeDir,DEFAULT_CACHE_DIR_NAME))
                }.Build();
            }

            FileUtil.EnsureExists(localCache.CacheDir);
            if (!localCache.CacheDir.Exists) {
                throw new ArgumentException(String.Format("Local cache dir '{0}' does not exist", localCache.CacheDir.FullName));
            }

            DependencyCache solutionCache;
            if (result.HasOptionValue("--soln-cache")) {
                var cacheDir = new DirectoryInfo(result.GetOptionValue("--soln-cach"));
                solutionCache = new DependencyCache.Builder() {
                    UpstreamCache = localCache,
                    VSProjectBaseSymbol = cacheDir.FullName,
                    CacheDir = cacheDir
                }.Build();
            } else {
                var solnDir = solutionFile.Directory;
                solutionCache = new DependencyCache.Builder() {
                    UpstreamCache = localCache,
                    VSProjectBaseSymbol = "$(SolutionDir)\\.cache",
                    CacheDir = new DirectoryInfo(Path.Combine(solnDir.FullName, ".cache"))
                }.Build();
            }

            FileUtil.EnsureExists(solutionCache.CacheDir);
            
            var cmd = new ProjectUpdateCommand {
                FailOnProjectChanged = result.GetOptionValueOrDefault("--fail", true) == "true",
                LocalCache = localCache,
                SolutionCache = solutionCache,
                ProjectFile = projectFile,
                SolutionFile = solutionFile
            };
            cmd.Invoke();
        }

        private static String GetUserHomeDir() {
            return Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
        }

        private class FailBuildException : Exception {
            internal FailBuildException(string msg)
                : base(msg) {
            }
        }
    }
}
