using System.IO;

namespace NRequire.IO
{
    public static class FileUtil
    {
        public static void CopyFile(FileInfo from, FileInfo to)
        {
            EnsureExists((to.Directory));

            using (var streamFrom = from.Open(FileMode.Open, FileAccess.Read))
            using (var streamTo = to.Open(FileMode.CreateNew, FileAccess.Write))
            {
                CopyStream(streamFrom, streamTo);
            }

            to.CreationTime = from.CreationTime;
            to.LastWriteTime = from.LastWriteTime;
        }

        public static void EnsureExists(DirectoryInfo dir)
        {
            Directory.CreateDirectory(dir.FullName);
        }

        public static void CopyStream(Stream streamFrom, Stream streamTo)
        {
            var buf = new byte[2048];
            int numBytesRead;
            while ((numBytesRead = streamFrom.Read(buf, 0, buf.Length)) > 0)
            {
                streamTo.Write(buf, 0, numBytesRead);
            };
        }
    }
}
