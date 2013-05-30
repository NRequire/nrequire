using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRequire.Matcher;
using NUnit.Framework;
using System.IO;

namespace NRequire.Test {
    [TestFixture]
    public class MatchGeneratorTest {

        [Test]
        public void TestMethod() {
            var gen = MatchGenerator.With()
                .Path(Path.Combine(GetProjectDir().FullName, "Generated/Matchers.cs"))
                .EqualMatcher<Version>("AVersion.EqualTo")
                .EqualMatcher<Scopes>("AnInstance.EqualTo")
                
                .Build();
            gen.GenerateFor<VSSolution>();
            gen.GenerateFor<VSSolution.ProjectReference>();
            gen.GenerateFor<VSProject>();
            gen.GenerateFor<Classifiers>("AClassifier");
            gen.GenerateFor<Dependency>();
            gen.GenerateFor<Wish>();
            gen.GenerateFor<Module>();
            gen.GenerateFor<Solution>();
            gen.GenerateFor<Project>();

            gen.WriteTo(new FileInfo(Path.Combine(GetProjectDir().Parent.FullName, "NRequire.Test.Support/Matcher/Generated.cs")));
        }

        private static DirectoryInfo GetProjectDir() {
            var dir = Directory.GetCurrentDirectory();
            var idx1 = dir.LastIndexOf("\\NRequire");
            var idx2 = dir.LastIndexOf("\\bin\\");
            if (idx1 > 0 && idx2 > 0 && (idx2 > idx1)) {
                var path = dir.Substring(0, idx2);
                return new DirectoryInfo(path);
            }
            throw new InvalidOperationException("Can't find the current project dir");
        }
    }
}
