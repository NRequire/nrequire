
namespace NRequire {
    /// <summary>
    /// Used to provide rudimentary support for tracing elements back to their original sources in case of errors (a.k.a where did
    /// this dependency requirement come from?)
    /// </summary>
    public interface ITakeSourceLocation {
        SourceLocations Source { get; set; }
    }
}
