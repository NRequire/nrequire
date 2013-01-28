using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace net.ndep {
    [TestFixture]
    public class VSProjectTest {

        [Test]
        public void CanUpdateProjectTest() {
            var from = FileUtil.ResourceFileFor<VSProjectTest>("_before.csproj.xml");
            var projFile = FileUtil.CopyToTmpFile(from);

            var proj = VSProject.FromPath(projFile);
            var resources = new List<Resource> {
                new Resource(new Dependency{ArtifactId="MyChildArtifactId1"}, null, "%CACHE_PATH%\\path\\to\\child1.ext"),
                new Resource(new Dependency{ArtifactId="MyChildArtifactId2"}, null, "%CACHE_PATH%\\path\\to\\child2.ext")
            };
            proj.WriteReferences(resources);

            //now check file equals the expected one
            var expectTxt = FileUtil.ReadResourceFileAsString<VSProjectTest>("_expect.csproj.xml");
            var actualTxt = FileUtil.ReadFileAsString(projFile);
            Assert.AreEqual(expectTxt,actualTxt);
            
        }


    }
}
