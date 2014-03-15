using System;
using NRequire.Model;

namespace NRequire
{
    public static class TestDefaults
    {
        public const String Group ="MyGroup";
        public const String Name = "MyName";
        public const String Arch = "any";
        public const String Runtime ="any";
        public const String Ext = null;//"dll";
        public const String Url = "http://localhost/somewhere";


        public static void Apply(AbstractDependency dep){
            dep.Group = Group;
            dep.Name = Name;
            dep.Arch = Arch;
            dep.Runtime = Runtime;
            dep.Ext = Ext;
            dep.Scope = Scopes.Runtime;
        }
    }
}

