using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.IO;
using System;
using NUnit.Framework;

namespace net.ndep {
    [TestFixture]
    public class ProgramTest {

        [Test]
        public void HappyPathTest() {
            var resourceDir = new DirectoryInfo("net/ndep/" + typeof(ProgramTest).Name);
            var localCacheDir = new DirectoryInfo(Path.Combine(resourceDir.FullName,"LocalCache"));
            
            var solnDir = FileUtil.CopyToTmpDir(new DirectoryInfo(Path.Combine(resourceDir.FullName, "Soln")));
            var projectFile = new FileInfo(Path.Combine(solnDir.FullName, "MyProject/MyProject.csproj"));
          
            new Program().InvokeWithArgs(new String[]{solnDir.FullName,projectFile.FullName,localCacheDir.FullName});

            var refs = VSProject
               .FromPath(projectFile)
               .ReadReference()
               .Where((reference) => reference.HintPath != null)
               .ToList();
            Assert.AreEqual(2, refs.Count);
            Assert.IsTrue(refs[0].HintPath.StartsWith(localCacheDir.FullName));
            Assert.IsTrue(refs[1].HintPath.StartsWith(localCacheDir.FullName));

            //check updated

            solnDir.Delete(true);
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

        
    }
}
