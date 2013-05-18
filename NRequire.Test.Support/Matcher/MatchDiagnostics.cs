using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NRequire.Matcher
{
    public class MatchDiagnostics : IMatchDiagnostics
    {
        private readonly String m_prefix = "";
        private static readonly String Indent = "    ";
        private readonly StringWriter m_sb = new StringWriter();

        public MatchDiagnostics()
        {
        }

        private MatchDiagnostics(String prefix)
        {
            m_prefix = prefix;
        }

        public IMatchDiagnostics NewChild()
        {
            return new MatchDiagnostics(m_prefix + Indent);
        }

        public void Print(String msg, params Object[] args)
        {
            m_sb.Write(Indent);  
            m_sb.WriteLine(msg, args);
        }

        public void Fail(String msg, params Object[] args)
        {
            m_sb.Write(Indent);  
            m_sb.WriteLine("MisMatched!");
            m_sb.Write(Indent);  
            m_sb.WriteLine(msg, args);
        }

        public void Pass(String msg, params Object[] args)
        {
            m_sb.Write(Indent);  
            m_sb.WriteLine("Matched");
            m_sb.Write(Indent);  
            m_sb.WriteLine(msg, args);
        }

        public bool Enabled {
            get {
                return true;
            }
        }

        public String ToString()
        {
            return m_sb.ToString();
        }
    }
}
