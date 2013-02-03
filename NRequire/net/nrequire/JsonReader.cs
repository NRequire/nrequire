using System;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;

namespace net.nrequire {
    public class JsonReader {

        private readonly JavaScriptSerializer m_serializer = new JavaScriptSerializer();

        public Solution ReadSolution(FileInfo jsonFile) {
            return Read<Solution>(jsonFile);
        }

        public Project ReadProject(FileInfo jsonFile) {
            return Read<Project>(jsonFile);
        }

        public Dependency ReadDependency(FileInfo jsonFile) {
            return Read<Dependency>(jsonFile);
        }

        public T Read<T>(FileInfo jsonFile) {
            if (!jsonFile.Exists) {
                throw new ArgumentException(String.Format("Could not read json dependency file: '{0}'", jsonFile.FullName));
            }

            try {
                var json = ReadFileAsString(jsonFile);
                return m_serializer.Deserialize<T>(json);
            } catch (Exception e) {
                throw new Exception(String.Format("Error while trying to parse '{0}'", jsonFile.FullName), e);
            }
        }

        private static String ReadFileAsString(FileInfo file) {
            using (var stream = file.OpenText()) {
                return stream.ReadToEnd();
            }
        }
    }
}
