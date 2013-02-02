using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Text.RegularExpressions;

namespace net.nrequire {

    //*.vsproj
    public class VSProject {

        public FileInfo Path { get; private set; }

        private VSProject(FileInfo filePath) {
            Path = filePath;
        }

        public static VSProject FromPath(FileInfo path) {
            return new VSProject(path);
        }

        internal bool UpdateReferences(IList<Resource> resources) {
            return UpdateReferences(resources.Select((res) => new Reference {
                Include = res.Dep.ArtifactId,
                HintPath = res.VSProjectPath
            }).ToList());
        }

        /// <summary>
        /// Update the project file with the given references. Only change the file if 
        /// there would be an actual change tot he file required
        /// </summary>
        /// <param name="references"></param>
        /// <returns>true if the project file was updated</returns>
        internal bool UpdateReferences(IList<Reference> references) {
            var xmlDoc = ReadXML();
            var existing = ReadReferences(xmlDoc).Where((r)=>r.HintPath != null).ToList();

            var update = existing.Count != references.Count;
            if (!update) {
                update = existing.Except(references).ToList().Count > 0;
            }
            if (!update) {
                update = references.Except(existing).ToList().Count > 0;
            }
            if (update) {
                WriteReferences(xmlDoc,references);
                return true;
            }
            return false;
        }

        private void WriteReferences(IList<Reference> references) {
            WriteReferences(ReadXML(), references);
        }

        private void WriteReferences(XmlDocument xmlDoc, IList<Reference> references) {    
            var proj = xmlDoc.GetElementsByTagName("Project").Item(0) as XmlNode;

            XmlNode refItemGroup = null;
            var refNodes = xmlDoc.GetElementsByTagName("Reference");
            var removeNodes = new List<XmlNode>();
            foreach (var refNode in refNodes) {
                var node = refNode as XmlNode;
                refItemGroup = node.ParentNode;
                var hintNode = node.GetChildNamed("HintPath");
                if (hintNode != null) {
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

            var appendReferences = references.Reverse();
            foreach (var reference in appendReferences) {
                AddReferenceTo(refItemGroup, reference);
            }
            WriteXml(xmlDoc);
        }

        private static void AddReferenceTo(XmlNode refItemGroup, Reference reference) {
            var doc = refItemGroup.OwnerDocument;
            var frag = doc.CreateDocumentFragment();
            //can't seem to be able to remove the xmlns attribute so let's atleast set
            //it to the correct value
            frag.InnerXml = String.Format(@"
    <Reference Include=""{0}"">
      <HintPath>{1}</HintPath>
    </Reference>", reference.Include, reference.HintPath);
            refItemGroup.InsertBefore(frag, refItemGroup.FirstChild);
        }

        internal IList<Reference> ReadReferences() {
            return ReadReferences(ReadXML());
        }

        private IList<Reference> ReadReferences(XmlDocument xmlDoc) {
            var references = new List<Reference>();
            var refNodes = xmlDoc.GetElementsByTagName("Reference");
            foreach (var refNode in refNodes) {
                var node = refNode as XmlNode;
                var hintNode = node.GetChildNamed("HintPath");
                var reference = new Reference {
                    Include = ((XmlAttribute)node.Attributes.GetNamedItem("Include")).Value,
                    HintPath = hintNode == null ? null : hintNode.InnerXml
                };
                references.Add(reference);
            }
            return references;
        }

        private XmlDocument ReadXML() {
            if (!Path.Exists) {
                throw new FileNotFoundException(String.Format("VS project file '{0}' does not exist", Path.FullName));
            }
            try {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.Load(Path.FullName);
                return xmlDoc;
            } catch (Exception e) {
                throw new Exception(String.Format("Error reading VS project file '{0}'", Path.FullName), e);
            }
        }

        private void WriteXml(XmlDocument xmlDoc) {
            if (Path.IsReadOnly) {
                throw new FileNotFoundException(String.Format("VS Project file '{0}' is readonly. Cannot update references", Path.FullName));
            }
            var xml = WriteXmlToString(xmlDoc);
            xml = RemoveEmptyNamespace(xml);
            using (var fstream = new StreamWriter(Path.Open(FileMode.Create, FileAccess.Write))) {
                fstream.Write(xml);
            }
        }

        private String WriteXmlToString(XmlDocument xmlDoc) {
            using (var stream = new MemoryStream())
            using (var writer = new XmlTextWriter(stream, Encoding.UTF8))
                try {
                    // writer.Formatting = Formatting.Indented; //this preserves indentation
                    xmlDoc.Save(writer);
                    stream.Position = 0;
                    return new StreamReader(stream).ReadToEnd();
                } finally {
                    writer.Close();
                }
        }

        private static string RemoveEmptyNamespace(String xml) {
            var reRemoveEmptyNamespace = new Regex(@"\s?xmlns\s?=\s?""""");
            var cleanXml = reRemoveEmptyNamespace.Replace(xml, "");
            return cleanXml;
        }

        public class Reference {
            public String Include { get; set; }
            public String HintPath { get; set; }

            public override bool Equals(object obj) {
                if (obj == null || !(obj is Reference)) {
                    return false;
                }
                if(ReferenceEquals(this,obj)){
                    return true;
                }
                var other = obj as Reference;
                return this.HintPath == other.HintPath
                    && this.Include == other.Include;
            }

            public override int GetHashCode() {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 11;
                    if (HintPath != null) {
                        hash = hash * 27 + HintPath.GetHashCode();
                    }
                    if (Include != null) {
                        hash = hash * 27 + Include.GetHashCode();
                    }
                    return hash;
                }
            }
        }
    }

    internal static class XmlNodeExtensions {
        public static XmlNode GetChildNamed(this XmlNode node, String childName) {
            if (node.HasChildNodes) {
                foreach (var child in node.ChildNodes) {
                    var xchild = child as XmlNode;
                    if (xchild.Name.Equals(childName)) {
                        return xchild;
                    }
                }
            }
            return null;
        }
    }
}
