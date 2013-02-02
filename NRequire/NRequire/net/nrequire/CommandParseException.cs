using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {
    internal class CommandParseException : Exception {
        public CommandParseException(String msg)
            : base(msg) {

        }
    }
}
