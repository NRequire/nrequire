using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace net.ndep {
    [TestFixture]
    public class ProgramTest {

        [Test]
        public void HappyPathTest() {
            var solnDir = new DirectoryInfo("net/ndep/" + typeof(ProgramTest).Name + ".Data/");
            var projectFile = new FileInfo(Path.Combine(solnDir.FullName, "MyProject/MyProject.csproj"));

            Assert.IsTrue(solnDir.Exists);
            Assert.IsTrue(projectFile.Exists);

            new Program().InvokeWithArgs(new String[]{solnDir.FullName,projectFile.FullName});
        }
    }
}
