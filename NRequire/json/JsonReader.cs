using System;
using System.IO;
using Newtonsoft.Json;

namespace NRequire {
    public class JsonReader {
        static JsonReader() {
            LoadEmbeddedDlls.Load();
        }
        
        public Solution ReadSolution(FileInfo jsonFile) {
            var soln = Read<Solution>(jsonFile);
            soln.AfterLoad();
            return soln;
        }

        public Project ReadProject(FileInfo jsonFile) {
            var p = Read<Project>(jsonFile);
            p.AfterLoad();
            return p;
        }

        public DependencyWish ReadDependency(FileInfo jsonFile) {
            var dep = Read<DependencyWish>(jsonFile);
            dep.AfterLoad();
            return dep;
        }

        public T Read<T>(FileInfo jsonFile) {
            if (!jsonFile.Exists) {
                throw new ArgumentException(String.Format("Could not read json dependency file: '{0}'", jsonFile.FullName));
            }

            String json = null;
            try {
                json = ReadFileAsString(jsonFile);
                var settings = new JsonSerializerSettings();
                settings.MissingMemberHandling = MissingMemberHandling.Error;
                settings.NullValueHandling = NullValueHandling.Ignore;
                
                return JsonConvert.DeserializeObject<T>(json, settings);
            } catch (Exception e) {
                throw new Exception(String.Format("Error while trying to parse file '{0}', with contents:\n{1}", jsonFile.FullName, json), e);
            }
        }

        private static String ReadFileAsString(FileInfo file) {
            using (var stream = file.OpenText()) {
                return stream.ReadToEnd();
            }
        }
    }
}
