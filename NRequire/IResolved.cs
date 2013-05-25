using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire {
    public interface IResolved : IResolvable {

        Version Version { get; }

    }
}
