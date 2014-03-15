using System;
using System.Collections.Generic;
using System.IO;

namespace NRequire.Model {

    //*.sln
    public class VSSolution {

        public FileInfo Path { get; private set; }

        private VSSolution(FileInfo filePath) {
            Path = filePath;
        }

        public static VSSolution FromPath(FileInfo path) {
            return new VSSolution(path);
        }

        internal IList<ProjectReference> ReadReferences() {
            return null;
        }

        
        //TODO
        public class ProjectReference {
            public String Name { get; set; }
            public String Path { get; set; }
            public Guid Guid{ get; set; }

            public override bool Equals(object obj) {
                if (obj == null || !(obj is ProjectReference)) {
                    return false;
                }
                if(ReferenceEquals(this,obj)){
                    return true;
                }
                var other = obj as ProjectReference;
                return this.Path == other.Path
                    && this.Guid == other.Guid
                    && this.Name == other.Name;
            }

            public override int GetHashCode() {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 11;
                    if (Path != null) {
                        hash = hash * 23 + Path.GetHashCode();
                    }
                    if (Name != null) {
                        hash = hash * 25 + Name.GetHashCode();
                    }
  
                    hash = hash * 27 + Guid.GetHashCode();
                
                    return hash;
                }
            }
        }
    }

   
}
