using System;

namespace NRequire {
    internal class CommandLineParseException : Exception {
        public CommandLineParseException(String msg)
            : base(msg) {

        }
    }
}
