using System;

namespace NRequire.Resolver {
    public class ResolutionException : ApplicationException {

        public ResolutionException(String msg, params Object[] args) : base(String.Format(msg, args)) { }
        public ResolutionException(String msg, Exception e) : base(msg,e) { }

    }
}
