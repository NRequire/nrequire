using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using JReader = Newtonsoft.Json.JsonReader;

namespace net.nrequire.json {    
    public class ClassifierConverter : JsonConverter {
        private static readonly KeyValuePairConverter DictConverter = new KeyValuePairConverter();

        public override bool CanConvert(Type objectType) {
            return typeof(IDictionary<String,String>).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JReader reader, Type objectType, object existingValue,
            JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.String) {
                var val = (String)reader.Value;
                return ParseClassifierString(val);
            }
            if (reader.TokenType == JsonToken.StartObject) {
                return DictConverter.ReadJson(reader, typeof(IDictionary<String, String>), existingValue, serializer);
            }
            throw new Exception(String.Format("Unexpected token parsing classifiers. Expected String, got {0}.", reader.TokenType));
        }

        public override void WriteJson(JsonWriter writer, object value,
            JsonSerializer serializer) {
            if (value is IDictionary<String,String>) {
                var val = ClassifiersAsString(value as IDictionary<String, String>);
                writer.WriteValue(val);
            }
        }

        private static IDictionary<String, String> ParseClassifierString(String s) {
            var opts = new Dictionary<String, String>();
            var parts = s.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts) {
                var pair = part.Split(new char[] { '-' });
                if (pair.Length == 1) {
                    opts[pair[0]] = "true";
                } else if (pair.Length == 2) {
                    opts[pair[0]] = pair[1];
                } else {
                    throw new ArgumentException(String.Format("Error parsing part '{0}' in options string'{1}' expected name-value pair", part, s));
                }
            }
            return opts;
        }

        private static String ClassifiersAsString(IDictionary<String, String> classifiers) {
            if (classifiers != null && classifiers.Count > 0) {
                var keys = new List<String>(classifiers.Keys);
                keys.Sort();
                var sb = new StringBuilder();
                foreach (var key in keys) {
                    var val = classifiers[key];
                    if (val == "true") {
                        if (sb.Length > 0) {
                            sb.Append("_");
                        }
                        sb.Append(key);
                    } else if (val == "false") {
                        //bool option and it doesn'texist, don't include modifier
                    } else {
                        if (sb.Length > 0) {
                            sb.Append("_");
                        }
                        sb.Append(key).Append("-").Append(val);
                    }
                }
                return sb.ToString();
            }
            return null;
        }
    }
}
