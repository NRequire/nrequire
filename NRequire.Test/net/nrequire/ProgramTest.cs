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
            Assert.AreEqual(3, refs.Count);
            Assert.IsTrue(refs[0].HintPath.StartsWith("$(SolutionDir)\\.cache"));
            Assert.IsTrue(refs[1].HintPath.StartsWith("$(SolutionDir)\\.cache"));
            Assert.IsTrue(refs[2].HintPath.StartsWith("$(SolutionDir)\\.cache"));

            //check all dependency resources are also copied across (like .xml and .pdb files)
            var solnCacheDir = new DirectoryInfo(solnDir.FullName + "\\.cache");

            Assert.IsTrue(solnDir.Exists);
            AssertDepResourceExists(solnCacheDir, "MyChildGroupId0\\MyChildArtifactId0\\0.0.0\\runtime-MyChildRuntime0\\arch-MyChildArch0\\MyChildArtifactId0.xml");
            AssertDepResourceExists(solnCacheDir, "MyChildGroupId0\\MyChildArtifactId0\\0.0.0\\runtime-MyChildRuntime0\\arch-MyChildArch0\\MyChildArtifactId0.pdb");
            AssertDepResourceNotExists(solnCacheDir, "MyChildGroupId0\\MyChildArtifactId0\\0.0.0\\runtime-MyChildRuntime0\\arch-MyChildArch0\\MyChildArtifactId0.ignored");

            AssertDepResourceExists(solnCacheDir, "MyChildGroupId1\\MyChildArtifactId1\\1.2.3\\runtime-MyChildRuntime1\\arch-MyChildArch1\\MyChildArtifactId1.related");
            AssertDepResourceNotExists(solnCacheDir, "MyChildGroupId1\\MyChildArtifactId1\\1.2.3\\runtime-MyChildRuntime1\\arch-MyChildArch1\\MyChildArtifactId1.xml");
            AssertDepResourceNotExists(solnCacheDir, "MyChildGroupId1\\MyChildArtifactId1\\1.2.3\\runtime-MyChildRuntime1\\arch-MyChildArch1\\MyChildArtifactId1.pdb");

            var projDir = new DirectoryInfo(projectFile.Directory.FullName);

            AssertDependencyCopied(projDir, "MyLibDir\\MyChildArtifactId0.dll");
            AssertDependencyCopied(projDir, "MyLibDir\\MyChildArtifactId0.xml");
            AssertDependencyCopied(projDir, "MyLibDir\\MyChildArtifactId0.pdb");

            AssertDependencyCopied(projDir, "MyLibDir\\MyChildArtifactId1.MyChildExt1");
            AssertDependencyCopied(projDir, "MyLibDir\\MyChildArtifactId1.related");

            solnDir.Delete(true);
        }

        private static void AssertDepResourceExists(DirectoryInfo cacheDir,String relPath) {
            var file = new FileInfo(Path.Combine(cacheDir.FullName, relPath));
            if (!file.Exists) {
                Assert.Fail("Dependency associated file '{0}' was not copied", file.FullName);
            }
        }

        private static void AssertDepResourceNotExists(DirectoryInfo cacheDir, String relPath) {
            var file = new FileInfo(Path.Combine(cacheDir.FullName, relPath));
            if (file.Exists) {
                Assert.Fail("Dependency associated file '{0}' was not expected to be copied", file.FullName);
            }
        }

        private static void AssertDependencyCopied(DirectoryInfo baseTargetDir, String relPath) {
            var file = new FileInfo(Path.Combine(baseTargetDir.FullName, relPath));
            if (!file.Exists) {
                Assert.Fail("Dependency file '{0}' was not copied", file.FullName);
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
