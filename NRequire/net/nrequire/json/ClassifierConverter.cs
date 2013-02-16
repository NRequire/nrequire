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
            return typeof(Classifiers).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JReader reader, Type objectType, object existingValue,
            JsonSerializer serializer) {
            var classifier = existingValue as Classifiers;
            if (classifier == null) {
                classifier = new Classifiers();
            }
            if (reader.TokenType == JsonToken.String) {
                var val = (String)reader.Value;
                return Classifiers.Parse(classifier, val);
            }
            if (reader.TokenType == JsonToken.StartObject) {
                var dict = DictConverter.ReadJson(reader, typeof(IDictionary<String, String>), existingValue, serializer);
                classifier.SetAll(dict as IDictionary<String, String>);
                return classifier;
            }
            throw new Exception(String.Format("Unexpected token parsing classifiers. Expected String, got {0}.", reader.TokenType));
        }

        public override void WriteJson(JsonWriter writer, object value,
            JsonSerializer serializer) {
            if (value is Classifiers) {
                var val = (value as Classifiers).ToString();
                writer.WriteValue(val);
            }
        }
    }
}
