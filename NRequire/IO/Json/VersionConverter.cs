using System;
using Newtonsoft.Json;
using Version = NRequire.Model.Version;

namespace NRequire.IO.Json {
    public class VersionConverter : JsonConverter {

        public override bool CanConvert(Type objectType) {
            return typeof(Version).IsAssignableFrom(objectType);
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.String) {
                var val = (String)reader.Value;
                return Version.Parse(val);
            }
            throw new Exception(
            String.Format("Unexpected token parsing version. Expected String, got {0}.", reader.TokenType));
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value,
            JsonSerializer serializer) {
            if (value is Version) {
                var val = (value as Version).ToString();
                writer.WriteValue(val);
            }
        }
    }
}
