using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace net.nrequire {
    [TestFixture]
    public class JsonReaderTest {

        [Test]
        public void ThrowsExceptionOnNonExistentJsonFileTest() {
            ArgumentException thrown = null;
            try {
                new JsonReader().ReadDependency(new FileInfo("/path/to/non/existent/file.json"));
            } catch (ArgumentException e) {
                thrown = e;
            }

            Assert.NotNull(thrown);
            Assert.IsTrue(thrown.Message.Contains("\\path\\to\\non\\existent\\file.json"));
        }
            
        [Test]
        public void CorrectlyReadsJsonFileTest() {
            var reader = new JsonReader();
            var depFile = FileUtil.FileFor<JsonReaderTest>(".depsfile1.json");
            var dep = reader.ReadDependency(depFile);

            Assert.NotNull(dep);

            Assert.AreEqual("MyName", dep.Name);
            Assert.AreEqual("MyGroupId", dep.GroupId);
            Assert.AreEqual("MyArtifactId", dep.ArtifactId);
            Assert.AreEqual("MyRuntime", dep.Runtime);
            Assert.AreEqual("http://nowhere.com/mine", dep.Url.ToString());
            Assert.AreEqual("1.0.0-SNAPSHOT", dep.Version);

            var child1 = dep.Dependencies[0];
            Assert.AreEqual("MyChildName1", child1.Name);
            Assert.AreEqual("MyChildGroupId1", child1.GroupId);
            Assert.AreEqual("MyChildArtifactId1", child1.ArtifactId);
            Assert.AreEqual("MyChildExt1", child1.Ext);
            Assert.AreEqual("MyChildRuntime1", child1.Runtime);
            Assert.AreEqual("http://nowhere.com/mychild1", child1.Url.ToString());
            Assert.AreEqual("1.2.3", child1.Version);
            
            var child2 = dep.Dependencies[1];
            Assert.AreEqual("MyChildName2", child2.Name);
            Assert.AreEqual("MyChildGroupId2", child2.GroupId);
            Assert.AreEqual("MyChildArtifactId2", child2.ArtifactId);
            Assert.AreEqual("MyChildExt2", child2.Ext);
            Assert.AreEqual("MyChildRuntime2", child2.Runtime);
            Assert.AreEqual("http://nowhere.com/mychild2", child2.Url.ToString());
            Assert.AreEqual("4.5.6", child2.Version);
        }
    }
}
