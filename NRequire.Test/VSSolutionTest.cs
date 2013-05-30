using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using NUnit.Framework;
using NRequire.Matcher;

namespace NRequire {
    [TestFixture]
    public class VSSolutionTest {

        [Test]
        public void CanReadSolutionTest() {
            var from = FileUtil.ResourceFileFor<VSSolutionTest>("MySolution.sln");
            var solnFile = FileUtil.CopyToTmpFile(from);
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
