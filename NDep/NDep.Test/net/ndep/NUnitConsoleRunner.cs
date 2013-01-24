using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.ndep {
    class NUnitConsoleRunner {
        [STAThread]
        static void Main(string[] args) {
            NUnit.ConsoleRunner.Runner.Main(args);
        }
    }
}
