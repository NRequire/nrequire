using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRequire.Matcher;
using System.IO;

namespace NRequire.Matcher {
    public class AFileInfo : AReflectionMatcher<FileInfo>{

        public static AFileInfo With() {
            return new AFileInfo();
        }

        public static IExtendedMatcher<FileInfo> EqualTo(FileInfo file) {
            return With().FullName(file.FullName);
        }

        public AFileInfo Exists(bool val) {
            AddProperty<bool?>("Exists", ABool.EqualTo(val));
            return this;
        }

        public AFileInfo Name(String path) {
            Name(AString.EqualTo(path));
            return this;
        }

        public AFileInfo Name(IExtendedMatcher<String> matcher) {
            AddProperty<String>("Name", matcher);
            return this;
        }

        public AFileInfo FullName(String path) {
            FullName(AString.EqualTo(path));
            return this;
        }

        public AFileInfo FullName(IExtendedMatcher<String> matcher) {
            AddProperty<String>("FullName", matcher);
            return this;
        }

    }
}
