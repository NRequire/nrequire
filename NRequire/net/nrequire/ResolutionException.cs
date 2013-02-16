using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {
    public class ResolutionException : Exception {

        public ResolutionException(String msg, params Object[] args) : base(String.Format(msg, args)) { }
        public ResolutionException(String msg, Exception e) : base(msg,e) { }

    }
}
