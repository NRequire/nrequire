using System;
using NRequire.Model;
using TestFirst.Net;
using TestFirst.Net.Matcher;

namespace NRequire
{
    public partial class AClassifier
    {
        public static IMatcher<Classifiers> EqualTo(Classifiers expect)
        {
            return EqualTo(expect==null?null:expect.ToString());
        }
        
        public static IMatcher<Classifiers> EqualTo(String expect)
        {
            if(expect == null){
                return AnInstance.Null<Classifiers>();
            }
            return Matchers.Function<Classifiers>(actual => expect.Equals(actual.ToString()), expect);
        }

    }
}
