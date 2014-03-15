using System;
using Newtonsoft.Json;

namespace NRequire.IO.Json {
    public class VersionMatcherConverter : JsonConverter {

        public override bool CanConvert(Type objectType) {
            return typeof(VersionMatcher).IsAssignableFrom(objectType);
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.String) {
                var val = (String)reader.Value;
                return VersionMatcher.Parse(val);
            }
            throw new Exception(
            String.Format("Unexpected token parsing version matcher. Expected String, got {0}.", reader.TokenType));
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value,
            JsonSerializer serializer) {
            if (value is VersionMatcher) {
                var val = (value as VersionMatcher).ToString();
                writer.WriteValue(val);
            }
        }
    }
}
