using NUnit.Framework;

namespace NRequire.Model {
    [TestFixture]
    public class VSSolutionTest {

        [Test]
        public void CanReadSolutionTest() {
            var from = FileHelper.ResourceFileFor<VSSolutionTest>("MySolution.sln");
            var solnFile = FileHelper.CopyToTmpFile(from);
            /*
            var soln = VSSolution.FromPath(solnFile);
            var projects = soln.ReadProjects();

            Expect
                .That(projects)
                .Is(AList.InOrder().WithOnly(AVSS))
            /*
            throw new NotImplementedException("need to read soln file and stuff");
        */
          }

    }
}
