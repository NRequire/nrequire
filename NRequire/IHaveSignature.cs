using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire {
    public interface IResolvable {
        string Group { get; }
        string Name { get; }
        String Ext { get; }
        Classifiers Classifiers { get; }

        String GetKey();

        String ToSummary();
    }
}
