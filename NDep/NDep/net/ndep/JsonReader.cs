using System;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;

namespace net.ndep {
    public class JsonReader {
        public Dependency ReadDependency(FileInfo depFile) {
            if (!depFile.Exists) {
                throw new ArgumentException(String.Format("Could not read json dependency file: '{0}'", depFile.FullName));
            }

            try {
                var json = ReadFileAsString(depFile);
                return new JavaScriptSerializer().Deserialize<Dependency>(json);
            } catch (Exception e) {
                throw new Exception(String.Format("Error while trying to parse '{0}'",depFile.FullName), e);
            }
        }

        private static String ReadFileAsString(FileInfo file) {
            using (var stream = file.OpenText()) {
                return stream.ReadToEnd();
            }
        }
    }
}
