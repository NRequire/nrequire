﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.IO;

namespace net.ndep {

    //*.vsproj
    public class VSProject {

        public FileInfo Path { get; private set; }

        private VSProject(FileInfo filePath) {
            Path = filePath;
        }

        public static  VSProject FromPath(FileInfo path) {
            return new VSProject(path);
        }

        internal void WriteReferences(IList<Resource> resources) {
            Console.WriteLine("Updating references!");
            
            var xmlDoc = ReadXML();
            var proj = xmlDoc.GetElementsByTagName("Project").Item(0) as XmlNode;
            var ns = proj.NamespaceURI;

            XmlNode refItemGroup = null;
            var references = xmlDoc.GetElementsByTagName("Reference");
            var removeNodes = new List<XmlNode>();
            foreach (var refNode in references) {
                var node = refNode as XmlNode;
                refItemGroup = node.ParentNode;
                var hintNode = node.GetChildNamed("HintPath");
                if(hintNode != null){
                    removeNodes.Add(node);
                    //and whitespace node so we don't add loads of
                    //empty lines
                    if (node.PreviousSibling.NodeType == XmlNodeType.Whitespace) {
                        removeNodes.Add(node.PreviousSibling);
                    }
                }
            }

            foreach (var node in removeNodes) {
                if (node.ParentNode != null) {
                    node.ParentNode.RemoveChild(node);
                }
            }

            var appendResources = resources.Reverse();
            foreach (var resource in appendResources) {
                AddReference(refItemGroup, resource, ns);
            }
            WriteXml(xmlDoc);
        }

        private static void AddReference(XmlNode refItemGroup, Resource resource, String ns) {
            var doc = refItemGroup.OwnerDocument;
            var frag = doc.CreateDocumentFragment();
            //can't seem to be able to remove the xmlns attribute so let's atleast set
            //it to the correct value
            frag.InnerXml = String.Format(@"
    <Reference Include=""{0}"" xmlns=""{1}"">
      <HintPath>{2}</HintPath>
    </Reference>", resource.Dep.ArtifactId, ns, resource.VSProjectPath);
            refItemGroup.InsertBefore(frag, refItemGroup.FirstChild);
        }

        private XmlDocument ReadXML() {
            if (!Path.Exists) {
                throw new FileNotFoundException(String.Format("VS project file '{0}' does not exist", Path.FullName));
            }
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(Path.FullName);
            xmlDoc.Save("testOut.xml");

            return xmlDoc;
        }

        private void WriteXml(XmlDocument xmlDoc) {
            if (Path.IsReadOnly) {
                throw new FileNotFoundException(String.Format("VS Project file '{0}' is readonly. Cannot update references", Path.FullName));
            }
            
            var writer = new XmlTextWriter(Path.FullName, Encoding.UTF8);
            try {
               // writer.Formatting = Formatting.Indented; //this preserves indentation
                xmlDoc.Save(writer);
            } finally {
                writer.Close();
            }
        }
    }

    internal static class XmlNodeExtensions {
        public static XmlNode GetChildNamed(this XmlNode node,String childName){
            if (node.HasChildNodes) {
                foreach (var child in node.ChildNodes) {
                    var xchild = child as XmlNode;
                    if(xchild.Name.Equals(childName)){
                         return xchild;
                    }
                }
            }
            return null;
        }
    }
}
