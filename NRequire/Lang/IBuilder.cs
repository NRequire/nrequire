using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Lang {
    public interface IBuilder<T> {
        T Build();
    }
}
