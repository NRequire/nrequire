﻿using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace NRequire.Model {
    [TestFixture]
    public class VSProjectTest {

        [Test]
        public void CanUpdateProjectTest() {
            var from = FileHelper.ResourceFileFor<VSProjectTest>("before.csproj.xml");
            var projFile = FileHelper.CopyToTmpFile(from);

            var proj = VSProject.FromPath(projFile);
            var resources = new List<VSProject.Reference> {
                new VSProject.Reference{Include="MyChildArtifactId1", HintPath="%CACHE_PATH%\\path\\to\\child1.ext" },
                new VSProject.Reference{Include="MyChildArtifactId2", EmbeddedResource = true, HintPath="%CACHE_PATH%\\path\\to\\child2.ext"}
            };
            proj.UpdateReferences(resources);

            //now check file equals the expected one
            var expectTxt = FileHelper.ReadResourceFileAsString<VSProjectTest>("expect.csproj.xml");
            var actualTxt = FileHelper.ReadFileAsString(projFile);
            Assert.AreEqual(expectTxt,actualTxt);

            projFile.Delete();
        }

        [Test]
        public void WontUpdateIfNoChangeInReferencesTest() {
            var from = FileHelper.ResourceFileFor<VSProjectTest>("before.csproj.xml");
            var projFile = FileHelper.CopyToTmpFile(from);

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
