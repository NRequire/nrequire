using System;
using System.IO;
using Newtonsoft.Json;

namespace NRequire {
    public class JsonReader {
        static JsonReader() {
            LoadEmbeddedDlls.Load();
        }
        
        public Solution ReadSolution(FileInfo jsonFile) {
            return Read<Solution>(jsonFile);
        }

        public Project ReadProject(FileInfo jsonFile) {
            return Read<Project>(jsonFile);
        }

        public Wish ReadDependency(FileInfo jsonFile) {
            return Read<Wish>(jsonFile);
        }

        public T Read<T>(FileInfo jsonFile) {
            if (!jsonFile.Exists) {
                throw new ArgumentException(String.Format("Json file does not exist for {0} using path '{1}'", typeof(T).Name, jsonFile.FullName));
            }

            String json = null;
            try {
                json = ReadFileAsString(jsonFile);
                var settings = new JsonSerializerSettings();
                settings.MissingMemberHandling = MissingMemberHandling.Error;
                settings.NullValueHandling = NullValueHandling.Ignore;

                var obj = JsonConvert.DeserializeObject<T>(json, settings);

                SetSourceLocations(obj, new FileSource{ SourceName = jsonFile.FullName } );

                var taker = obj as IRequireLoadNotification;
                if( taker!= null){
                    taker.AfterLoad();
                }

                return obj;
            } catch (Exception e) {
                throw new Exception(String.Format("Error while trying to parse file '{0}', {1}, with contents:{2}", jsonFile.FullName, e.Message,json), e);
            }
        }

        private class FileSource : ISource {
            public String SourceName { get; set;}
        }
        
        private static void SetSourceLocations(Object obj, ISource source) {
            SourceLocations.AddToSourceLocations(obj, source);
        }

        private static String ReadFileAsString(FileInfo file) {
            using (var stream = file.OpenText()) {
                return stream.ReadToEnd();
            }
        }
    }
}
