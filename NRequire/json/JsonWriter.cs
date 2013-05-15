using System;
using System.IO;
using Newtonsoft.Json;

namespace NRequire {
    public class JsonWriter {
        static JsonWriter() {
            LoadEmbeddedDlls.Load();
        }

        public void WriteTo<T>(T instance,FileInfo file) {
            var json = Write<T>(instance);
            WriteToFileAsUTF8(file, json);
        }

        public String Write<T>(T instance) {
            if (instance == null) {
                throw new ArgumentException("Can not write a null object");
            }

            try {
                var settings = new JsonSerializerSettings();
                settings.MissingMemberHandling = MissingMemberHandling.Error;
                settings.NullValueHandling = NullValueHandling.Ignore;
                settings.Formatting = Formatting.Indented;
                return JsonConvert.SerializeObject(instance, Formatting.Indented, settings);
            } catch (Exception e) {
                throw new Exception(String.Format("Error while trying to serialize object {0}", instance), e);
            }
        }

        private void WriteToFileAsUTF8(FileInfo file, String content) {
            file.Directory.Create();
            using (var stream = file.Open(FileMode.Create, FileAccess.Write)) {
                var bytes = System.Text.Encoding.UTF8.GetBytes(content);
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
