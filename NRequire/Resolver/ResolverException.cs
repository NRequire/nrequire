using System;

namespace NRequire.Resolver
{
    //thrown to indicate there was an error while trying to resolve the dependencies
    public class ResolverException : ApplicationException
    {

        public static readonly String NoSolutions = "No dependency solution could be found";
        public static readonly String InfiniteRecursion = "Infinite recursion detected";

        public ResolverException(String msg):base(msg)
        {
        }

        public ResolverException(String msg, Exception e):base(msg,e)
        {
        }
    }
}

