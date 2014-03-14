using System;
using System.IO;
using NRequire.Util;

namespace NRequire {
    public class Resource {

        public String VSProjectPath { get; private set; }
        public String Type { get { return File.Extension.Substring(1).ToLowerInvariant() ; } }
        public bool Exists { get { return File.Exists; } }
        public FileInfo File { get; private set; }
        /// <summary>
        /// The dependency which led to this resource being chosen
        /// </summary>
        public IResolved Dep { get; set; }
        public DateTime TimeStamp { get { return File.LastWriteTime;  } }

        public Resource(IResolved dep, FileInfo fullPath, String vsProjectPath) {
            Dep = dep;
            File = fullPath;
            VSProjectPath = vsProjectPath;
        }

        /// <summary>
        /// Case insensitive compare of the resource file extension
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsType(String type) {
            return Type.Equals(type.ToLowerInvariant());
        }

        /// <summary>
        /// copy this resource to the given path
        /// </summary>
        /// <param name="targetFile"></param>
        public void CopyTo(FileInfo targetFile) {
            CopyFile(this.File, targetFile);
        }

        private static void CopyFile(FileInfo from, FileInfo to) {
            FileUtil.EnsureExists((to.Directory));
            
            using (var streamFrom = from.Open(FileMode.Open, FileAccess.Read))
            using (var streamTo = to.Open(FileMode.CreateNew, FileAccess.Write)) {
                CopyStream(streamFrom, streamTo);
            }

            to.CreationTime = from.CreationTime;
            to.LastWriteTime = from.LastWriteTime;
        }

        private static void CopyStream(Stream streamFrom, Stream streamTo) {
            var buf = new byte[2048];
            int numBytesRead;
            while ((numBytesRead = streamFrom.Read(buf, 0, buf.Length)) > 0) {
                streamTo.Write(buf, 0, numBytesRead);
            };
        }

        public override string ToString() {
            return String.Format("Resource@{0}<FullPath:{1},VSProjectPath:{2},Dependency:{3}>",
                base.GetHashCode(),
                File.FullName,
                VSProjectPath,
                Dep
            );
        }
    }
}
