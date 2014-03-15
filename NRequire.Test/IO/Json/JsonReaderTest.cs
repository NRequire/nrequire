using System;
using System.IO;
using NUnit.Framework;

namespace NRequire.IO.Json {

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
        public void ReadAllFieldsTest() {
            var reader = new JsonReader();
            var depFile = FileHelper.FileFor<JsonReaderTest>("depsfile1.json");
            var dep = reader.ReadDependency(depFile);

            Assert.NotNull(dep);

            Assert.AreEqual("MyName", dep.Name);
            Assert.AreEqual("MyGroup", dep.Group);
            Assert.AreEqual("myarch", dep.Arch);
            Assert.AreEqual("myruntime", dep.Runtime);
            Assert.AreEqual("http://nowhere.com/mine", dep.Url.ToString());
            Assert.AreEqual("1.0.0.SNAPSHOT", dep.VersionString);

            var child1 = dep.TransitiveWishes[0];
            Assert.AreEqual("MyChildName1", child1.Name);
            Assert.AreEqual("MyChildGroup1", child1.Group);
            Assert.AreEqual("MyChildExt1", child1.Ext);
            Assert.AreEqual("mychildruntime1", child1.Runtime);
            Assert.AreEqual("mychildarch1", child1.Arch);

            Assert.AreEqual("http://nowhere.com/mychild1", child1.Url.ToString());
            Assert.AreEqual("1.2.3", child1.VersionString);
            Assert.AreEqual("arch-mychildarch1_key1-val1_key2-val2_runtime-mychildruntime1", child1.ClassifiersString);

         
            var child2 = dep.TransitiveWishes[1];
            Assert.AreEqual("MyChildName2", child2.Name);
            Assert.AreEqual("MyChildGroup2", child2.Group);
            Assert.AreEqual("MyChildExt2", child2.Ext);
            Assert.AreEqual("mychildruntime2", child2.Runtime);
            Assert.AreEqual("http://nowhere.com/mychild2", child2.Url.ToString());
            Assert.AreEqual("4.5.6", child2.VersionString);
         
        }
    }
}
