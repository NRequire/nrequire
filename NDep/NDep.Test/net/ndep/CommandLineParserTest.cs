using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace net.ndep {
    [TestFixture]
    public class CommandLineParserTest {

        [Test]
        public void NoArgsThrowsExceptionTest() {
            CommandParseException thrown = null;
            try {
                new CommandLineParser().Parse(new String[] {});
            } catch (CommandParseException e) {
                thrown = e;
            }
            Assert.NotNull(thrown);
        }
        
        [Test]
        public void ExtractOptionsTest() {
            var args = new[] { "mycmd", "-opt1", "optval1", "-opt2", "optval2" }; 
            
            var parser = new CommandLineParser()
                .AddCommand("mycmd", "the command")
                .AddOptionWithValue("mycmd", "-opt1", "opt1 help text")
                .AddOptionWithValue("mycmd", "-opt2", "opt2 help text");

            var result = parser.Parse(args);

            Assert.AreEqual("mycmd", result.Command);
            Assert.IsTrue(result.HasOptionValue("-opt1"));
            Assert.AreEqual("optval1", result.GetOptionValue("-opt1"));
            Assert.IsTrue(result.HasOptionValue("-opt2"));
            Assert.AreEqual("optval2", result.GetOptionValue("-opt2"));

        }
    }
}
