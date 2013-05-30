using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using NRequire.Lang;

namespace NRequire.Test {
    
}
namespace NRequire.Test {
    /// <summary>
    /// I generate matcher classes
    /// </summary>
    public class MatchGenerator {

        DirectoryInfo m_generateTo;
        private IDictionary<String, String> m_equalMatchersByTypeName;
        private StringBuilder m_buff = new StringBuilder();
        private ISet<String> m_imports = new SortedSet<String>();


        public static Builder With() {
            return new Builder();
        }

        private MatchGenerator(DirectoryInfo generateTo, IDictionary<String,String> equalMatchers) {
            m_generateTo = generateTo;
            m_equalMatchersByTypeName = new Dictionary<String, String>(equalMatchers);

            m_equalMatchersByTypeName["System.String"] = "AString.EqualTo";
            m_equalMatchersByTypeName["System.string"] = "AString.EqualTo";
            m_equalMatchersByTypeName["System.int"] = "AnInt.EqualTo";
            m_equalMatchersByTypeName["System.bool"] = "ABool.EqualTo";
            m_equalMatchersByTypeName["System.IO.FileInfo"] = "AFileInfo.EqualTo";
        
        }

        public void GenerateFor<T>(String matcherName) {
            GenerateFor(typeof(T), matcherName);
        }

        public void GenerateFor<T>() {
            GenerateFor(typeof(T), "A" + typeof(T).Name);
        }
        
        public void GenerateFor(Type t, String matcherName) {
            var builder = new MatcherBuilder(t, matcherName, m_equalMatchersByTypeName);
            m_buff.Append(builder.Generate(m_imports));
        }

        public void WriteTo(FileInfo file) {
            var content = new StringBuilder();
            foreach (var import in m_imports) {
                content.AppendLine(import);
            }
            content.AppendLine(m_buff.ToString());

            WriteToFile(content.ToString(), file);
        }
        private void WriteToFile(String content, FileInfo file) {
            file.Directory.Create();

            using (var stream = file.Open(FileMode.Create, FileAccess.Write)) {
                var bytes = System.Text.Encoding.UTF8.GetBytes(content);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        class MatcherBuilder {
            private StringWriter m_w;
            private IList<String> m_imports = new List<String>();
            private IList<String> m_methods = new List<String>();

            private IDictionary<String, String> m_equalMatchersByTypeName;

            private String MatcherType { get;set; }
            private Type m_type;
            private const String Indent = "    ";

            private String m_indent = "";
            private int m_indentDepth = 0;

            public MatcherBuilder(Type type, String matcherName, IDictionary<String, String> equalMatchersByType) {
                m_type = type;
                MatcherType = matcherName;

                m_imports.Add("System");
                m_imports.Add("System.Collections.Generic");
                m_imports.Add("System.Linq");
                m_imports.Add("System.Text");
                m_imports.Add("NRequire.Test");
                m_imports.Add("NRequire.Matcher");


                m_equalMatchersByTypeName = equalMatchersByType;
            }

            public String Generate(ISet<String> imports) {
                m_w = new StringWriter();
                //imports
                foreach (var import in m_imports) {
                    imports.Add("using " + import + ";");
                }

                //namespace
                WriteLine();
                WriteLine("namespace NRequire.Matcher {");
                
                //class
                WriteLine();
                IncrementIndent();
                WriteLine("public partial class " + MatcherType + " : AReflectionMatcher<" + m_type.FullName + ">{");

                IncrementIndent();
                //With() method
                WriteLine();
                WriteLine("public static " + MatcherType + " With(){");
                WriteLine(m_indent + "return new " + MatcherType + "();");
                WriteLine("}");
                
                //custom methods
                foreach (var method in m_methods) {
                    WriteLine();
                    WriteLine(method);
                }
                GenerateMethods();

                DecrementIndent();
                WriteLine("}");//class
                DecrementIndent(); 
                WriteLine("}");//namespace

                return m_w.ToString();
            }

            private void GenerateMethods() {
                var props = m_type.GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);
                foreach (var p in props) {

                    var pType = p.PropertyType.FullName;
                    var pTypeShort = p.PropertyType.FullName;
                    if (p.PropertyType.Namespace == "System") {
                        pTypeShort = p.PropertyType.Name;
                    }
                    if (m_equalMatchersByTypeName.ContainsKey(pType)) {
                        var equalMatcherSnippet = m_equalMatchersByTypeName[pType];
                        
                        WriteLine();
                        WriteLine("public " + MatcherType + " " + p.Name + "(" + pTypeShort + " val) {");
                        IncrementIndent();
                        WriteLine(p.Name + "(" + equalMatcherSnippet + "(val));");
                        WriteLine("return this;");
                        DecrementIndent();
                        WriteLine("}");
                    }
                    
                    WriteLine();
                    WriteLine("public " + MatcherType + " " + p.Name + "(IExtendedMatcher<" + pTypeShort + "> matcher) {");
                    IncrementIndent();
                    WriteLine("AddProperty<" + pTypeShort + ">(\"" + p.Name + "\", matcher);");
                    WriteLine("return this;");
                    DecrementIndent();
                    WriteLine("}");
                }
            }


            private void WriteLine() {
                m_w.WriteLine();
            }
            
            private void Write(String line, params Object[] args) {
                if (args == null || args.Length == 0) {
                    m_w.Write(line);
                } else {
                    m_w.Write(line, args);
                }
            }
            
            private void WriteLine(String line, params Object[] args){
                m_w.Write(m_indent);
                if (args == null || args.Length == 0) {
                    m_w.WriteLine(line);
                } else {
                    m_w.WriteLine(line, args);       
                }
            }

            private void IncrementIndent() {
                SetIndent(++m_indentDepth);
            }

            private void DecrementIndent() {
                SetIndent(--m_indentDepth);
            }

            private void SetIndent(int depth) {
                m_indent = "";
                for (var i = 0; i < depth; i++) {
                    m_indent += Indent;
                }
            }

        }

        public class Builder {
            private DirectoryInfo m_generateToDir;
            private Dictionary<String, String> m_equalMatcherSnippets = new Dictionary<string, string>();
            
            public MatchGenerator Build() {
                PreConditions.NotNull(m_generateToDir, "Path");

                return new MatchGenerator(m_generateToDir, m_equalMatcherSnippets);
            }

            public Builder EqualMatcher<T>(String equalSnippet) {
                EqualMatcher(typeof(T).FullName,equalSnippet);
                return this;
            }

            public Builder EqualMatcher(String fullType,String equalSnippet) {
                m_equalMatcherSnippets[fullType] = equalSnippet;
                return this;
            }

            public Builder Path(String path) {
                Path(new DirectoryInfo(path));
                return this;
            }

            public Builder Path(DirectoryInfo path) {
                m_generateToDir = path;
                return this;
            }
        }
    }
}
