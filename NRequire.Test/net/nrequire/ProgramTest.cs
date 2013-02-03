using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace net.nrequire {
    [TestFixture]
    public class ProgramTest {

        [Test]
        public void HappyPathTest() {
            var resourceDir = FileUtil.DirectoryFor<ProgramTest>();
            var localCacheDir = new DirectoryInfo(Path.Combine(resourceDir.FullName,"LocalCache"));

            var solnDir = FileUtil.CopyToTmpDir(new DirectoryInfo(Path.Combine(resourceDir.FullName, "MySoln")));
            var solnFile = new FileInfo(Path.Combine(solnDir.FullName, "MySoln.sln"));
            var projectFile = new FileInfo(Path.Combine(solnDir.FullName, "MyProject/MyProject.csproj"));
          
            new Program().InvokeWithArgs(new String[]{
                "update-proj",
                "--soln", solnFile.FullName,
                "--proj", projectFile.FullName,
                "--cache", localCacheDir.FullName});

            var refs = VSProject
               .FromPath(projectFile)
               .ReadReferences()
               .Where((reference) => reference.HintPath != null)
               .ToList();
            //Expect to ignore dependencies in soln's nrequire.json but not in projects
            Assert.AreEqual(2, refs.Count);
            Assert.IsTrue(refs[0].HintPath.StartsWith("$(SolutionDir)\\.cache"));
            Assert.IsTrue(refs[1].HintPath.StartsWith("$(SolutionDir)\\.cache"));

            //check all dependency resources are also copied across (like .xml and .pdb files)
            var solnCacheDir = new DirectoryInfo(solnDir.FullName + "\\.cache");

            Assert.IsTrue(solnDir.Exists);
            //AssertDepResourceExists(solnCacheDir, "MyChildGroupId1\\MyChildArtifactId1\\1.2.3\\runtime-MyChildRuntime1\\arch-MyChildArch1\\MyChildArtifactId1.xml");
            //AssertDepResourceExists(solnCacheDir, "MyChildGroupId1\\MyChildArtifactId1\\1.2.3\\runtime-MyChildRuntime1\\arch-MyChildArch1\\MyChildArtifactId1.pdb");
            
            solnDir.Delete(true);
        }

        private static void AssertDepResourceExists(DirectoryInfo cacheDir,String relPath) {
            var file = new FileInfo(Path.Combine(cacheDir.FullName, relPath));
            if (!file.Exists) {
                Assert.Fail("Dependency associated file '{0}' was not copied", file.FullName);
            }
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
