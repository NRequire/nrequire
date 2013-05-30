using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NRequire.Matcher;

namespace NRequire.Test.Matcher {
    public class AVSProject : AReflectionMatcher<VSProject>{
        
        public static AVSProject With() {
            return new AVSProject();
        }

        public AVSProject Path(String path) {
            Path(AFileInfo.With().FullName(AString.EqualTo(path)));
            return this;
        }

        public AVSProject Path(IExtendedMatcher<FileInfo> matcher) {
            AddProperty<FileInfo>("Path", matcher); ;
            return this;
        }

    }
}
