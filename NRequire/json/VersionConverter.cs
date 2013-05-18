﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using JReader = Newtonsoft.Json.JsonReader;
using JWriter = Newtonsoft.Json.JsonWriter;

namespace NRequire.Json {
    public class VersionConverter : JsonConverter {

        public override bool CanConvert(Type objectType) {
            return typeof(Version).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JReader reader, Type objectType, object existingValue,
            JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.String) {
                var val = (String)reader.Value;
                return Version.Parse(val);
            }
            throw new Exception(
            String.Format("Unexpected token parsing version. Expected String, got {0}.", reader.TokenType));
        }

        public override void WriteJson(JWriter writer, object value,
            JsonSerializer serializer) {
            if (value is Version) {
                var val = (value as Version).ToString();
                writer.WriteValue(val);
            }
        }
    }
}
