
namespace NRequire.Matcher {

    public interface IMatcher<in T> {
        bool Match(T actual);
    }

}
