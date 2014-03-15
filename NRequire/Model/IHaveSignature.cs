using System;

namespace NRequire.Model {
    public interface IResolvable {
        string Group { get; }
        string Name { get; }
        String Ext { get; }
        Classifiers Classifiers { get; }

        Key Key { get; }

        String ToSummary();
    }
}
