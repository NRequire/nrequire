using System;

namespace NRequire
{
    public class NRequireException : Exception
    {
        public NRequireException(String msg):base(msg)
        {
        }

        public NRequireException(String msg, Exception e)
            : base(msg, e)
        {
        }
    }
}
