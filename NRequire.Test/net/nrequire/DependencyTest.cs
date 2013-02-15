using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace net.nrequire {

    [TestFixture]
    public class DependencyTest {

        [Test]
        public void CloneEmptyDepTest() {
            var dep = new Dependency();
            var clone = dep.Clone();

            Assert.AreNotSame(dep, clone);
            Assert.NotNull(clone);
        }

        [Test]
        public void CloneSimplePropsTest() {

            var dep = new Dependency {
                Name = "MyName",
                Group = "MyGroupId",
                Runtime = "MyRuntime",
                Ext = "MyExt",
                Arch = "MyArch",
                VersionString = "1.2.3",
                CopyTo = "/path/to/copy/to",
                Related = new[]{"ext1","ext2"},
                Url = new Uri("http://nowhere.com/file")
            };

            var clone = dep.Clone();

            Assert.AreNotSame(dep, clone);
            Assert.AreEqual("MyName", clone.Name);
            Assert.AreEqual("MyGroupId", clone.Group);
            Assert.AreEqual("MyExt", clone.Ext);
            Assert.AreEqual("MyRuntime", clone.Runtime);
            Assert.AreEqual("http://nowhere.com/file", clone.Url.ToString());
            Assert.AreEqual("1.2.3", clone.VersionString);
            Assert.AreEqual("/path/to/copy/to", clone.CopyTo);
            Assert.AreEqual(new List<String> { "ext1","ext2" }, clone.Related);

        }

        [Test]
        public void CloneChildDependenciesTest() {

            var dep = new Dependency {
                Dependencies = new List<Dependency> { 
                    new Dependency{
                        Name = "MyName1"
                    },
                    new Dependency{
                        Name = "MyName2"
                    },
                }
            };

            var clone = dep.Clone();

            Assert.AreNotSame(dep, clone);
            Assert.AreNotSame(dep.Dependencies, clone.Dependencies);

            Assert.AreEqual(dep.Dependencies, clone.Dependencies);
        }


        [Test]
        public void MergeSimplePropsTest() {

            var dep = new Dependency {
                Name = "MyName",
                Group = "MyGroupId",
                Runtime = "MyRuntime",
                Ext = "MyExt",
                Arch = "MyArch",
                VersionString = "1.2.3",
                CopyTo = "path/to/copy/to",
                Related = new[] { "ext1", "ext2" },
                Url = new Uri("http://nowhere.com/file")
            };

            CheckMergeProp(dep, (d, val) => d.Arch = val, d => d.Arch, "", "MyParentVal", "MyChildVal");
            CheckMergeProp(dep, (d, val) => d.Name = val, d => d.Name, "", "MyParentVal", "MyChildVal");
            CheckMergeProp(dep, (d, val) => d.Ext = val, d => d.Ext, "", "MyParentVal", "MyChildVal");
            CheckMergeProp(dep, (d, val) => d.Group = val, d => d.Group, "", "MyParentVal", "MyChildVal");
            CheckMergeProp(dep, (d, val) => d.Runtime = val, d => d.Runtime, "", "MyParentVal", "MyChildVal");
            CheckMergeProp(dep, (d, val) => d.VersionString = val, d => d.VersionString, null, "1.2.3", "4.5.6");
            CheckMergeProp(dep, (d, val) => d.CopyTo = val, d => d.CopyTo, "", "MyParentVal", "MyChildVal");
            CheckMergeProp(dep, (d, val) => d.Related = val, d => d.Related, List(), List("MyParentVal"), List("MyChildVal"));

            CheckMergeProp(dep, (d, val) => d.Url = val, d => d.Url, null, new Uri("http://nowhere/myparent"), new Uri("http://nowhere/mychild"));
        }

        private static IList<String> List(params String[] vals) {
            return new List<String>(vals);
        }

        private void CheckMergeProp<T>(Dependency d, Action<Dependency, T> setter, Func<Dependency, T> getter, T emptyVal, T setParentVal,T setChildVal) {
            try {
                var parent = d.Clone();
                var child = d.Clone();

                setter.Invoke(parent, setParentVal);
                setter.Invoke(child, emptyVal);
                var merged = child.FillInBlanksFrom(parent);
                Assert.AreEqual(getter.Invoke(merged), setParentVal);

                setter.Invoke(parent, setParentVal);
                setter.Invoke(child, setChildVal);
                merged = child.FillInBlanksFrom(parent);
                Assert.AreEqual(getter.Invoke(merged), setChildVal);

                setter.Invoke(parent, emptyVal);
                setter.Invoke(child, setChildVal);
                merged = child.FillInBlanksFrom(parent);
                Assert.AreEqual(getter.Invoke(merged), setChildVal);
            } catch(Exception e){
                throw new AssertionException( "Exception while testing merge", e);
            }
        }

        [Test]
        public void MergeChildDependenciesTest() {

            var parent = new Dependency {
                Name = "MyArtifactId",
                Group = "MyGroupId",

                Dependencies = new List<Dependency> { 
                    new Dependency{
                        Group = "MyGroupId1",
                        Name = "MyArtifactId1", 
                        VersionString = "1.2.3"
                    },
                    new Dependency{
                        Group = "MyGroupId2",
                        Name = "MyArtifactId2", 
                        VersionString = "1.2.3"
                    },
                }
            };

            var dep = new Dependency {
                Name = "MyArtifactId",
                Group = "MyGroupId",

                Dependencies = new List<Dependency> { 
                    new Dependency{
                        Group = "MyGroupId1",
                        Name = "MyArtifactId1", 
                    },
                    //overrides the verison
                    new Dependency{
                        Group = "MyGroupId2",
                        Name = "MyArtifactId2", 
                        VersionString = "4.5.6" 
                    },
                }
            };

            var merged = dep.FillInBlanksFrom(parent);

            Assert.AreEqual(2, merged.Dependencies.Count);
            var child1 = merged.Dependencies[0];
            Assert.AreEqual("MyGroupId1",child1.Group);
            Assert.AreEqual("MyArtifactId1",child1.Name);
            Assert.AreEqual("1.2.3",child1.VersionString);
            
            var child2 = merged.Dependencies[1];
            Assert.AreEqual("MyGroupId2", child2.Group);
            Assert.AreEqual("MyArtifactId2", child2.Name);
            Assert.AreEqual("4.5.6", child2.VersionString);

        }

        [Test]
        public void MergeChildChildDependenciesTest() {

            var parent = new Dependency {
                Dependencies = new List<Dependency> { 
                    new Dependency{
                        Group = "MyGroupId",
                        Name = "MyArtifactId", 
                        VersionString = "1",
                        Dependencies = new List<Dependency> { 
                            new Dependency{
                                Group = "MyGroupId",
                                Name = "MyArtifactId", 
                                VersionString = "1.2",
                                Dependencies = new List<Dependency> { 
                                    new Dependency{
                                        Group = "MyGroupId",
                                        Name = "MyArtifactId", 
                                        VersionString = "1.2.3"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var dep = new Dependency {
                Dependencies = new List<Dependency> { 
                    new Dependency{
                        Group = "MyGroupId",
                        Name = "MyArtifactId", 
                        Dependencies = new List<Dependency> { 
                            new Dependency{
                                Group = "MyGroupId",
                                Name = "MyArtifactId",
                                Dependencies = new List<Dependency> { 
                                    new Dependency{
                                        Group = "MyGroupId",
                                        Name = "MyArtifactId"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var merged = dep.FillInBlanksFrom(parent);

            Assert.AreEqual(1, merged.Dependencies.Count);
            var child = merged.Dependencies[0];
            Assert.AreEqual("MyGroupId", child.Group);
            Assert.AreEqual("MyArtifactId", child.Name);
            Assert.AreEqual("1", child.VersionString);

            Assert.AreEqual(1, child.Dependencies.Count);
            var child2 = child.Dependencies[0];
            Assert.AreEqual("MyGroupId", child2.Group);
            Assert.AreEqual("MyArtifactId", child2.Name);
            Assert.AreEqual("1.2", child2.VersionString);

            Assert.AreEqual(1, child2.Dependencies.Count);
            var child3 = child2.Dependencies[0];
            Assert.AreEqual("MyGroupId", child3.Group);
            Assert.AreEqual("MyArtifactId", child3.Name);
            Assert.AreEqual("1.2.3", child3.VersionString);

        }
    }
}
