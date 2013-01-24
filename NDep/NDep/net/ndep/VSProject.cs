using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.IO;

namespace net.ndep {

    //*.vsproj
    public class VSProject {

        public FileInfo Path { get; private set; }

        public VSProject(FileInfo filePath) {
            Path = filePath;
        }


        internal void WriteReferences(IList<Resource> resources) {
            var xmlDoc = ReadXML();

            XPathNavigator nav = xmlDoc.CreateNavigator();
            XPathExpression expr = nav.Compile("/ItemGroup/Reference/HintPath");
            XPathNodeIterator iter = nav.Select(expr);
            while (iter.MoveNext()) {
                var node = iter.Current;
               
            }

            XmlNodeList references = xmlDoc.GetElementsByTagName("Reference");
            for (int i = 0; i < references.Count;i++ ) {
                var node = references[i];
                Console.WriteLine(node.Value);
            }
            //// Compile a standard XPath expression
            //XPathExpression expr = nav.Compile("/catalog/name");
            //XPathNodeIterator iterator = nav.Select(expr);


            //while (iterator.MoveNext()) {
            //    XPathNavigator nav2 = iterator.Current.Clone();
            //    listBox1.Items.Add("price: " + nav2.Value);
            //}

            //WriteXml(xmlDoc);
        }

        private XmlDocument ReadXML() {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(Path.FullName);
            //xmlDoc.Save("testOut.xml");

            return xmlDoc;
        }

        private void WriteXml(XmlDocument xmlDoc) {
            var writer = new XmlTextWriter(Path.FullName, Encoding.Unicode);
            try {
                writer.Formatting = Formatting.Indented; //this preserves indentation
                xmlDoc.Save(writer);
            } finally {
                writer.Close();
            }
        }
    }
}
