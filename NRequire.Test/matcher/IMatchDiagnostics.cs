using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Matcher {
    public interface IMatchDiagnostics {
        bool Enabled { get; } 
        IMatchDiagnostics NewChild();
        void Print(String msg, params Object[] args);
        void Fail(String msg, params Object[] args);
        void Pass(String msg, params Object[] args);
    }
}
