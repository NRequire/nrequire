using System.IO;

namespace NRequire.Util
{
    public static class FileUtil
    {
        public static void EnsureExists(DirectoryInfo dir)
        {
            Directory.CreateDirectory(dir.FullName);
        }
    }
}
