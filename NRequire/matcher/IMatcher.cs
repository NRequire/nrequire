﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Matcher {

    public interface IMatcher<in T> {
        bool Match(T actual);
    }

}
