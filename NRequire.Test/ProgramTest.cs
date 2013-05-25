using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.IO;
using NUnit.Framework;
using NRequire.Matcher;

namespace NRequire {
    [TestFixture]
    public class ProgramTest : BaseDependencyTest {

        [Test]
        public void HappyPathTest() {
            var cache = CacheWith()
                ;

            var resourceDir = FileUtil.DirectoryFor<ProgramTest>();
            var localCacheDir = new DirectoryInfo(Path.Combine(resourceDir.FullName,"LocalCache"));

            var solnDir = FileUtil.CopyToTmpDir(new DirectoryInfo(Path.Combine(resourceDir.FullName, "MySoln")));
            var solnFile = new FileInfo(Path.Combine(solnDir.FullName, "MySoln.sln"));
            var projectFile = new FileInfo(Path.Combine(solnDir.FullName, "MyProject/MyProject.csproj"));
          
            new Program().InvokeWithArgs(new String[]{
                "update-vsproj",
                "--soln", solnFile.FullName,
                "--proj", projectFile.FullName,
                "--cache", localCacheDir.FullName,
                "--log", "trace"
            });

            var refs = VSProject
               .FromPath(projectFile)
               .ReadReferences()
               .Where((reference) => reference.HintPath != null)
               .ToList();
            //Expect to ignore dependencies in soln's nrequire.json but not in projects

         /*   Expect
                .That(refs.Select(r=>r.HintPath).ToList())
                .Is(AList.InOrder()
                    .With(AString.StartingWith("$(SolutionDir)\\.cache\\Group0"))
                    .And(AString.EqualTo("$(SolutionDir)\\.cache\\Group1\\Name1\\1.2.3\\arch-any_runtime-4.0\\Name1.Ext1"))
                    .And(AString.StartingWith("$(SolutionDir)\\.cache\\Group2"))
                    .And(AString.StartingWith("$(SolutionDir)\\.cache\\TransitiveGroup")));*/

            //check all dependency resources are also copied across (like .xml and .pdb files)
            var solnCacheDir = new DirectoryInfo(solnDir.FullName + "\\.cache");

            Assert.IsTrue(solnDir.Exists);
            AssertDepResourceExists(solnCacheDir, "Group0\\Name0\\0.0.0\\arch-Arch0_runtime-Runtime0\\Name0.dll");
            AssertDepResourceExists(solnCacheDir, "Group0\\Name0\\0.0.0\\arch-Arch0_runtime-Runtime0\\Name0.xml");
            AssertDepResourceExists(solnCacheDir, "Group0\\Name0\\0.0.0\\arch-Arch0_runtime-Runtime0\\Name0.pdb");
            AssertDepResourceNotExists(solnCacheDir, "Group0\\Name0\\0.0.0\\arch-Arch0_runtime-Runtime0\\Name0.ignored");

            AssertDepResourceNotExists(solnCacheDir, "Group1\\Name1\\1.2.3\\arch-any_runtime-4.0\\Name1.dll");
            AssertDepResourceExists(solnCacheDir, "Group1\\Name1\\1.2.3\\arch-any_runtime-4.0\\Name1.Ext1");
            AssertDepResourceNotExists(solnCacheDir, "Group1\\Name1\\1.2.3\\arch-any_runtime-4.0\\Name1.xml");
            AssertDepResourceNotExists(solnCacheDir, "Group1\\Name1\\1.2.3\\arch-any_runtime-4.0\\Name1.pdb");

            var projDir = new DirectoryInfo(projectFile.Directory.FullName);

            AssertDependencyCopied(projDir, "MyLibDir\\Name0.dll");
            AssertDependencyCopied(projDir, "MyLibDir\\Name0.xml");
            AssertDependencyCopied(projDir, "MyLibDir\\Name0.pdb");

            AssertDependencyCopied(projDir, "MyLibDir\\Name1.Ext1");
            AssertDependencyCopied(projDir, "MyLibDir\\Name1.related");

            AssertDependencyCopied(projDir, "MyLibDir\\TransitiveName.TransitiveExt");
            AssertDependencyCopied(projDir, "MyLibDir\\ProvidedName.dll");
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
