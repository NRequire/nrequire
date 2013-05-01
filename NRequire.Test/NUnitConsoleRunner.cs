using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire {
    class NUnitConsoleRunner {
        [STAThread]
        static void Main(string[] args) {
            //try {
                NUnit.ConsoleRunner.Runner.Main(args);
            //} catch (Exception e) {
            //    System.Environment.ExitCode = -1;
            //    Console.WriteLine(e.Message);
            //    Console.WriteLine("");
            //    Console.WriteLine("Stacktrace:");
            //    Console.WriteLine(e.StackTrace);
            //}
        }
    }
}
