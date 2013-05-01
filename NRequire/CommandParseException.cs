using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire {
    internal class CommandParseException : Exception {
        public CommandParseException(String msg)
            : base(msg) {

        }
    }
}
