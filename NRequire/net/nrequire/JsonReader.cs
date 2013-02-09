﻿using System;
using System.IO;
using Newtonsoft.Json;

namespace net.nrequire {
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

        public Dependency ReadDependency(FileInfo jsonFile) {
            return Read<Dependency>(jsonFile);
        }

        public T Read<T>(FileInfo jsonFile) {
            if (!jsonFile.Exists) {
                throw new ArgumentException(String.Format("Could not read json dependency file: '{0}'", jsonFile.FullName));
            }

            String json = null;
            try {
                json = ReadFileAsString(jsonFile);
                return JsonConvert.DeserializeObject<T>(json); 
                ///return m_serializer.Deserialize<T>(json);
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
