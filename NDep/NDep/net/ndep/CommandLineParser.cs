using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.ndep {
    internal class CommandLineParser {

        private IDictionary<String, Options> m_options = new Dictionary<string, Options>();

        public String PrintHelp() {
            var sb = new StringBuilder();
            sb.AppendLine("Usage:");
            foreach (var cmd in m_options.Keys) {
                var opts = m_options[cmd];

                sb.Append("  ").AppendLine(cmd);
                foreach (var optName in opts.OptionKeys) {
                    sb.Append("   ").Append(optName).Append("  --").AppendLine(opts.GetHelpFor(optName));
                }
            }
            return sb.ToString();
        }

        internal CommandLineParser AddCommand(string command, string commandHelp) {
            if (!m_options.ContainsKey(command)) {
                m_options[command] = new Options();
            }
            return this;
        }

        internal CommandLineParser AddOptionWithValue(string command, string optionName, string optionHelp) {
            if (!m_options.ContainsKey(command)) {
                m_options[command] = new Options();
            }
            m_options[command].AddOptionHelp(optionName, optionHelp);

            return this;
        }

        internal ParseResult Parse(string[] args) {
            if (args == null || args.Length == 0) {
                throw new CommandParseException(String.Format("Expected atleast one command. Options are : [{0}]", 
                    String.Join(",", m_options.Keys)));
            }
            var result = new ParseResult();

            var cmd = args[0];
            if (!m_options.ContainsKey(cmd)) {
                throw new CommandParseException(String.Format("Unrecognized command '{0}', valid commands are : [{1}]", 
                    cmd, String.Join(",", m_options.Keys)));
            }
            result.Command = cmd;
            var options = m_options[cmd];

            for (int i = 1; i < args.Length; i++) {
                var optName = args[i];
                if (!options.ContainsOption(optName)) {
                    throw new CommandParseException(String.Format("Unrecognized option '{0}' for command '{1}', valid options are : [{2}]",
                        optName, cmd, String.Join(",", options.OptionKeys)));
                }
                if (i < args.Length - 1) {
                    i++;
                    var optVal = args[i];
                    result.AddOptionValue(optName, optVal);
                } else {
                    result.AddOptionValue(optName, null);
                }
            }
            return result;
        }

        private class Options {
            private IDictionary<string, string> m_optionsHelp = new Dictionary<string, string>();

            public IEnumerable<string> OptionKeys { get { return m_optionsHelp.Keys;  } }

            internal bool ContainsOption(string optName) {
                return m_optionsHelp.ContainsKey(optName);
            }

            internal void AddOptionHelp(string optName, string optHelpText) {
                m_optionsHelp[optName] = optHelpText;
            }

            internal string GetHelpFor(string optName) {
                return m_optionsHelp[optName];
            }
        }

        internal class ParseResult {
            private IDictionary<string, string> m_optionsValues = new Dictionary<string, string>();

            public String Command { get; set; }

            internal bool HasOptionValue(string optName) {
                return m_optionsValues.ContainsKey(optName);
            }

            internal String GetOptionValue(string optName) {
                if (!m_optionsValues.ContainsKey(optName)) {
                    throw new CommandParseException(
                            String.Format("No option named '{0}' set, but do have [{1}]", 
                                optName, String.Join(",", m_optionsValues.Keys)));
                }
                return m_optionsValues[optName];
            }

            internal void AddOptionValue(string optName, string optVal) {
                m_optionsValues[optName] = optVal;
            }
        }

    }
}
