using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Matcher {
    public class NillMatchDiagnostics : IMatchDiagnostics {

        public static readonly IMatchDiagnostics Instance = new NillMatchDiagnostics();

        public IMatchDiagnostics NewChild() { return this; }
        public void Print(String msg, params Object[] args) { }
        public void Fail(String msg, params Object[] args) { }
        public void Pass(String msg, params Object[] args) { }
        public bool Enabled { get { return false; } }
    }
}
