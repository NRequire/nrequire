using System;
using System.IO;

namespace NRequire {

    public static class FileHelper {

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0);

        public static String ReadResourceFileAsString<T>(String relPath) {
            return ReadFileAsString(ResourceFileFor<T>(relPath));
        }

        public static String ReadFileAsString(FileInfo file) {
            using (var stream = file.OpenText()) {
                return stream.ReadToEnd();
            }
        }
        public static FileInfo ResourceFileFor<T>(String relPath) {
            var file = FileFor<T>(relPath);
            if (!file.Exists) {
                throw new FileNotFoundException(String.Format("Could not find resource file '{0}' for type {1} and relative path '{2}'", file.FullName,typeof(T).FullName, relPath));
            }
            return file;
        }

        public static FileInfo CopyToTmpFile(FileInfo srcFile) {
            var targetFile = NewTmpFile();
            CopyFile(srcFile, targetFile);
            return targetFile;
        }

        public static DirectoryInfo CopyToTmpDir(DirectoryInfo srcDir) {
            var targetDir = NewTmpDir();
            CopyDir(srcDir, targetDir);
            return targetDir;
        }

        public static void CopyDir(DirectoryInfo srcDir, DirectoryInfo targetDir) {
            targetDir.Create();
            foreach (var fromFile in srcDir.GetFiles()) {
                var toFile = new FileInfo(Path.Combine(targetDir.FullName, fromFile.Name));
                CopyFile(fromFile, toFile);
            }
            foreach (var fromChildDir in srcDir.GetDirectories()) {
                var toChildDir = new DirectoryInfo(Path.Combine(targetDir.FullName, fromChildDir.Name));
                CopyDir(fromChildDir, toChildDir);
            }
        }

        public static void CopyFile(FileInfo from, FileInfo to) {
            using (var streamFrom = from.Open(FileMode.Open, FileAccess.Read))
            using (var streamTo = to.Open(FileMode.CreateNew, FileAccess.Write)) {
                CopyStream(streamFrom, streamTo);
            }
        }

        private static void CopyStream(FileStream streamFrom, FileStream streamTo) {
            var buf = new byte[2048];
            int numBytesRead;
            while ((numBytesRead = streamFrom.Read(buf, 0, buf.Length)) > 0) {
                streamTo.Write(buf, 0, numBytesRead);
            };
        }

        public static FileInfo NewTmpFile() {
            return new FileInfo(Path.Combine(TmpRootPath(), (DateTime.Now - Epoch).TotalMilliseconds.ToString()));
        }

        public static DirectoryInfo NewTmpDir(String namePart) {
            return new DirectoryInfo(Path.Combine(TmpRootPath(), (DateTime.Now - Epoch).TotalMilliseconds.ToString() + "-" + namePart));
        }

        public static DirectoryInfo NewTmpDir() {
            return new DirectoryInfo(Path.Combine(TmpRootPath(),(DateTime.Now - Epoch).TotalMilliseconds.ToString()));
        }

        private static String TmpRootPath(){
            return "C:\\tmp\\nunit-" + typeof(FileHelper).FullName; 
        }

        public static FileInfo FileFor<T>(String relPath) {
            return new FileInfo(ResourceFor<T>() + "/" + relPath);
        }

        public static DirectoryInfo DirectoryFor<T>() {
            return new DirectoryInfo(ResourceFor<T>());
        }

        private static String ResourceFor<T>() {
            var fn = typeof(T).FullName;
            if (fn.StartsWith("NRequire.")) {
                fn = fn.Substring("NRequire.".Length);
            }
            return fn.Replace(".","\\");
        }
    }
}
