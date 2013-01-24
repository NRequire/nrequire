using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace net.ndep {
    public class JsonReader {
        public Dependency ReadDependency(FileInfo depFile) {
            if (!depFile.Exists) {
                throw new ArgumentException(String.Format("Could not read json dependency file: '{0}'", depFile.FullName));
            }

            try {
                using (var stream = depFile.OpenText()) {
                    var json = stream.ReadToEnd();
                    return JsonConvert.DeserializeObject<Dependency>(json);
                }
            } catch (Exception e) {
                throw new Exception(String.Format("Error while trying to parse '{0}'",depFile.FullName), e);
            }
        }

    }
}
