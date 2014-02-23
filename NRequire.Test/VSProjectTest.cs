using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace NRequire {
    [TestFixture]
    public class VSProjectTest {

        [Test]
        public void CanUpdateProjectTest() {
            var from = FileUtil.ResourceFileFor<VSProjectTest>("before.csproj.xml");
            var projFile = FileUtil.CopyToTmpFile(from);

            var proj = VSProject.FromPath(projFile);
            var resources = new List<VSProject.Reference> {
                new VSProject.Reference{Include="MyChildArtifactId1", HintPath="%CACHE_PATH%\\path\\to\\child1.ext" },
                new VSProject.Reference{Include="MyChildArtifactId2", EmbeddedResource = true, HintPath="%CACHE_PATH%\\path\\to\\child2.ext"}
            };
            proj.UpdateReferences(resources);

            //now check file equals the expected one
            var expectTxt = FileUtil.ReadResourceFileAsString<VSProjectTest>("expect.csproj.xml");
            var actualTxt = FileUtil.ReadFileAsString(projFile);
            Assert.AreEqual(expectTxt,actualTxt);

            projFile.Delete();
        }

        [Test]
        public void WontUpdateIfNoChangeInReferencesTest() {
            var from = FileUtil.ResourceFileFor<VSProjectTest>("before.csproj.xml");
            var projFile = FileUtil.CopyToTmpFile(from);

            var proj = VSProject.FromPath(projFile);
            var resources = new List<VSProject.Reference> {
                new VSProject.Reference{Include="MyChildArtifactId1", HintPath="%CACHE_PATH%\\path\\to\\child1.ext"},
                new VSProject.Reference{Include="MyChildArtifactId2",HintPath="%CACHE_PATH%\\path\\to\\child2.ext"}
            };
            var changed1stTime = proj.UpdateReferences(resources);
            Assert.True(changed1stTime);

            var orgWriteTime = projFile.LastWriteTime;
            //wait a bit to let clock tick
            Thread.Sleep(TimeSpan.FromSeconds(1));
            //force an update, shou6ld check if already exist
            resources.Reverse();
            var changed2ndTime = proj.UpdateReferences(resources);
            var newWriteTime = projFile.LastWriteTime;
            
            Assert.False(changed2ndTime);
            Assert.AreEqual(orgWriteTime, newWriteTime);

            projFile.Delete();
        }
    }
}
