namespace NRequire.IO.Json
{
    /// <summary>
    /// Marks an instance as requiring to be notified once it has been loaded say from disk. Allows it to 
    /// perform internal checking and building of data structures
    /// </summary>
    public interface IRequireLoadNotification
    {
        void AfterLoad();
    }
}

