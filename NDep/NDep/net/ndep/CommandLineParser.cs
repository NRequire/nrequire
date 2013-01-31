using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.ndep {
    internal class CommandLineParser {

        private IDictionary<String, Options> m_options = new Dictionary<string, Options>();

        private String m_executableName = "prog";

        public String PrintHelp() {
            var sb = new StringBuilder();
            sb.AppendLine("Usage: ").Append(m_executableName).Append(" CMD OPTIONS");
            foreach (var cmd in m_options.Keys) {
                var opts = m_options[cmd];

                sb.AppendLine().Append("command: ").Append(cmd);
                if (opts.CommandHelp != null) {
                    sb.AppendLine().Append("      -- ").Append(opts.CommandHelp);
                }
                sb.AppendLine().Append("options: ");
                foreach (var opt in opts.GetOptions()) {
                    sb.AppendLine().Append("   ").Append(opt.Name);
                    if( opt.ArgName != null){
                        sb.Append(" $").Append(opt.ArgName);
                    }
                    sb.AppendLine().Append("       -- ").Append(opt.IsRequired ? "required" : "optional");
                    if (opt.HelpText != null) {
                        sb.AppendLine().Append("       -- ").Append(opt.HelpText);
                    }
                }
            }
            return sb.ToString();
        }



        internal CommandLineParser ProgramName(string progName) {
            m_executableName = progName;
            return this;
        }

        internal CommandLineParser AddCommand(string command, string commandHelp) {
            if (!m_options.ContainsKey(command)) {
                m_options[command] = new Options();
            }
            return this;
        }

        internal CommandLineParser AddOption(string command,  Opt opt) {
            if (!m_options.ContainsKey(command)) {
                m_options[command] = new Options();
            }

            m_options[command].AddOption(opt);

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

            internal String CommandHelp { get; set; }

            private IDictionary<string, Opt> m_optionsHelp = new Dictionary<string, Opt>();

            public IEnumerable<string> OptionKeys { get { return m_optionsHelp.Keys;  } }

            internal bool ContainsOption(string optName) {
                return m_optionsHelp.ContainsKey(optName);
            }

            internal void AddOption(Opt opt) {
                m_optionsHelp[opt.Name] = opt;
            }

            internal Opt GetOptionNamed(string optName) {
                return m_optionsHelp[optName];
            }

            internal IEnumerable<Opt> GetOptions() {
                return m_optionsHelp.Values;
            }
        }

        internal class ParseResult {
            private IDictionary<string, string> m_optionsValues = new Dictionary<string, string>();

            public String Command { get; set; }

            public bool IsCommand(string cmd) {
                return cmd.Equals(Command);
            }

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

    internal class Opt {
        public String Name { get; private set; }
        public String HelpText { get; private set; }
        public bool IsRequired { get; private set; }
        public String ArgName { get; private set; }

        public static Opt Named(string name) {
            return new Opt { Name = name };
        }

        private Opt() {
            IsRequired = true;
        }

        public Opt Required(bool required) {
            this.IsRequired = required;
            return this;
        }

        public Opt Help(string helpText) {
            this.HelpText = helpText;
            return this;
        }

        public Opt Arg(string argName) {
            this.ArgName = argName;
            return this;
        }


    }

}
