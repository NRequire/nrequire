using System;

namespace NRequire.Model
{
    public class Key
    {
        private readonly string _value;

        public Key(string value)
        {
            _value = value;
        }

        public static Key FromGroupNameClassifiers(string group, string name, Classifiers classifiers)
        {
            return new Key(String.Format("{0}:{1}:{2}"
                , group
                , name
                , classifiers.ToString()
            ).ToLower());
        }

        public override string ToString()
        {
            return _value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var id = obj as Key; 
            if (id == null)
            {
                return false;
            }
            return _value.Equals(id._value);
        }
    }
}
