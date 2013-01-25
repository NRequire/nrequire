using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System;
using NUnit.Framework;

namespace net.ndep {
    [TestFixture]
    public class ProgramTest {

        private static readonly DateTime Epoch = new DateTime(1970,1,1,0,0,0);

        [Test]
        public void HappyPathTest() {
            var resourceDir = new DirectoryInfo("net/ndep/" + typeof(ProgramTest).Name);
            var localCacheDir = new DirectoryInfo(Path.Combine(resourceDir.FullName,"LocalCache"));
            
            var solnDir = CopyToTmpDir(new DirectoryInfo(Path.Combine(resourceDir.FullName, "Soln")));
            var projectFile = new FileInfo(Path.Combine(solnDir.FullName, "MyProject/MyProject.csproj"));
            
            Assert.IsTrue(solnDir.Exists);
            Assert.IsTrue(projectFile.Exists);
            Assert.IsTrue(localCacheDir.Exists);

            new Program().InvokeWithArgs(new String[]{solnDir.FullName,projectFile.FullName,localCacheDir.FullName});
        }

        private void GenerateFileAt(FileInfo file) {
            var random = new Random();
            var totalBytesWritten = 0;
            var buf = new byte[1024];
            using (var stream = file.Open(FileMode.CreateNew, FileAccess.Write)) {
                while (totalBytesWritten < 8000) {
                    random.NextBytes(buf);
                    stream.Write(buf, 0, buf.Length);
                }
            }
        }

        private DirectoryInfo CopyToTmpDir(DirectoryInfo srcDir) {
            var targetDir = newTmpDir();
            CopyDir(srcDir, targetDir);
            return targetDir;
        }

        private static void CopyDir(DirectoryInfo srcDir, DirectoryInfo targetDir)
        {
            targetDir.Create();
            foreach(var fromFile in srcDir.GetFiles()){
                var toFile = new FileInfo(Path.Combine(targetDir.FullName,fromFile.Name));
                CopyFile(fromFile, toFile);
            }
            foreach(var fromChildDir in srcDir.GetDirectories()){
                var toChildDir = new DirectoryInfo(Path.Combine(targetDir.FullName,fromChildDir.Name));
                CopyDir(fromChildDir, toChildDir);
            }
        }

        private static void CopyFile(FileInfo from, FileInfo to)
        {
            using(var streamFrom = from.Open(FileMode.Open,FileAccess.Read))
            using(var streamTo = to.Open(FileMode.CreateNew,FileAccess.Write)){
                CopyStream(streamFrom, streamTo);
            }
        }

        private static void CopyStream(FileStream streamFrom, FileStream streamTo)
        {
            var buf = new byte[2048];
            int numBytesRead;
            while( (numBytesRead= streamFrom.Read(buf,0,buf.Length)) > 0){
                streamTo.Write(buf,0, numBytesRead);
            };
        }

        private DirectoryInfo newTmpDir() {
            return new DirectoryInfo("C:\\tmp\\ndep-tmp-" + (DateTime.Now-Epoch).TotalMilliseconds);
        }
    }
}
