
namespace NRequire.Matcher {
    internal class AlwaysTrueMatcher<T>:IMatcher<T> {
        public bool Match(T actual) {
            return true;
        }
    }
}
