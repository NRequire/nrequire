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
                ArtifactId = "MyArtifactId",
                GroupId = "MyGroupId",
                Runtime = "MyRuntime",
                Ext = "MyExt",
                Arch = "MyArch",
                Version = "1.2.3",
                CopyTo = "/path/to/copy/to",

                Url = new Uri("http://nowhere.com/file")
            };

            var clone = dep.Clone();

            Assert.AreNotSame(dep, clone);
            Assert.AreEqual("MyName", clone.Name);
            Assert.AreEqual("MyGroupId", clone.GroupId);
            Assert.AreEqual("MyArtifactId", clone.ArtifactId);
            Assert.AreEqual("MyExt", clone.Ext);
            Assert.AreEqual("MyRuntime", clone.Runtime);
            Assert.AreEqual("http://nowhere.com/file", clone.Url.ToString());
            Assert.AreEqual("1.2.3", clone.Version);
            Assert.AreEqual("/path/to/copy/to", clone.CopyTo);
        }

        [Test]
        public void CloneDepsTest() {

            var dep = new Dependency {
                Dependencies = new List<Dependency> { 
                    new Dependency{
                        Name = "MyName1",
                        ArtifactId = "MyArtifactId1", 
                    },
                    new Dependency{
                        Name = "MyName2",
                        ArtifactId = "MyArtifactId2", 
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
                ArtifactId = "MyArtifactId",
                GroupId = "MyGroupId",
                Runtime = "MyRuntime",
                Ext = "MyExt",
                Arch = "MyArch",
                Version = "1.2.3",
                CopyTo = "path/to/copy/to",
                Url = new Uri("http://nowhere.com/file")
            };

            CheckMergeProp(dep, (d, val) => d.Arch = val, d => d.Arch, "", "MyParentVal", "MyChildVal");
            CheckMergeProp(dep, (d, val) => d.ArtifactId = val, d => d.ArtifactId, "", "MyParentVal", "MyChildVal");
            CheckMergeProp(dep, (d, val) => d.Ext = val, d => d.Ext, "", "MyParentVal", "MyChildVal");
            CheckMergeProp(dep, (d, val) => d.GroupId = val, d => d.GroupId, "", "MyParentVal", "MyChildVal");
            CheckMergeProp(dep, (d, val) => d.Name = val, d => d.Name, "", "MyParentVal", "MyChildVal");
            CheckMergeProp(dep, (d, val) => d.Runtime = val, d => d.Runtime, "", "MyParentVal", "MyChildVal");
            CheckMergeProp(dep, (d, val) => d.Version = val, d => d.Version, "", "MyParentVal", "MyChildVal");
            CheckMergeProp(dep, (d, val) => d.CopyTo = val, d => d.CopyTo, "", "MyParentVal", "MyChildVal");

            CheckMergeProp(dep, (d, val) => d.Url = val, d => d.Url, null, new Uri("http://nowhere/myparent"), new Uri("http://nowhere/mychild"));
        }

        private void CheckMergeProp<T>(Dependency d, Action<Dependency, T> setter, Func<Dependency, T> getter, T emptyVal, T setParentVal,T setChildVal) {
            try {
                var parent = d.Clone();
                var child = d.Clone();

                setter.Invoke(parent, setParentVal);
                setter.Invoke(child, emptyVal);
                var merged = child.MergeWithParent(parent);
                Assert.AreEqual(getter.Invoke(merged), setParentVal);

                setter.Invoke(parent, setParentVal);
                setter.Invoke(child, setChildVal);
                merged = child.MergeWithParent(parent);
                Assert.AreEqual(getter.Invoke(merged), setChildVal);

                setter.Invoke(parent, emptyVal);
                setter.Invoke(child, setChildVal);
                merged = child.MergeWithParent(parent);
                Assert.AreEqual(getter.Invoke(merged), setChildVal);
            } catch(Exception e){
                throw new AssertionException( "Exception while testing merge", e);
            }
        }

        [Test]
        public void MergeChildDependenciesTest() {

            var parent = new Dependency {
                ArtifactId = "MyArtifactId",
                GroupId = "MyGroupId",

                Dependencies = new List<Dependency> { 
                    new Dependency{
                        GroupId = "MyGroupId1",
                        ArtifactId = "MyArtifactId1", 
                        Version = "1.2.3"
                    },
                    new Dependency{
                        GroupId = "MyGroupId2",
                        ArtifactId = "MyArtifactId2", 
                        Version = "1.2.3"
                    },
                }
            };

            var dep = new Dependency {
                ArtifactId = "MyArtifactId",
                GroupId = "MyGroupId",

                Dependencies = new List<Dependency> { 
                    new Dependency{
                        GroupId = "MyGroupId1",
                        ArtifactId = "MyArtifactId1", 
                    },
                    //overrides the verison
                    new Dependency{
                        GroupId = "MyGroupId2",
                        ArtifactId = "MyArtifactId2", 
                        Version = "4.5.6" 
                    },
                }
            };

            var merged = dep.MergeWithParent(parent);

            Assert.AreEqual(2, merged.Dependencies.Count);
            var child1 = merged.Dependencies[0];
            Assert.AreEqual("MyGroupId1",child1.GroupId);
            Assert.AreEqual("MyArtifactId1",child1.ArtifactId);
            Assert.AreEqual("1.2.3",child1.Version);
            
            var child2 = merged.Dependencies[1];
            Assert.AreEqual("MyGroupId2", child2.GroupId);
            Assert.AreEqual("MyArtifactId2", child2.ArtifactId);
            Assert.AreEqual("4.5.6", child2.Version);

        }

        [Test]
        public void MergeChildChildDependenciesTest() {

            var parent = new Dependency {
                Dependencies = new List<Dependency> { 
                    new Dependency{
                        GroupId = "MyGroupId",
                        ArtifactId = "MyArtifactId", 
                        Version = "1",
                        Dependencies = new List<Dependency> { 
                            new Dependency{
                                GroupId = "MyGroupId",
                                ArtifactId = "MyArtifactId", 
                                Version = "1.2",
                                Dependencies = new List<Dependency> { 
                                    new Dependency{
                                        GroupId = "MyGroupId",
                                        ArtifactId = "MyArtifactId", 
                                        Version = "1.2.3"
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
                        GroupId = "MyGroupId",
                        ArtifactId = "MyArtifactId", 
                        Dependencies = new List<Dependency> { 
                            new Dependency{
                                GroupId = "MyGroupId",
                                ArtifactId = "MyArtifactId",
                                Dependencies = new List<Dependency> { 
                                    new Dependency{
                                        GroupId = "MyGroupId",
                                        ArtifactId = "MyArtifactId"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var merged = dep.MergeWithParent(parent);

            Assert.AreEqual(1, merged.Dependencies.Count);
            var child = merged.Dependencies[0];
            Assert.AreEqual("MyGroupId", child.GroupId);
            Assert.AreEqual("MyArtifactId", child.ArtifactId);
            Assert.AreEqual("1", child.Version);

            Assert.AreEqual(1, child.Dependencies.Count);
            var child2 = child.Dependencies[0];
            Assert.AreEqual("MyGroupId", child2.GroupId);
            Assert.AreEqual("MyArtifactId", child2.ArtifactId);
            Assert.AreEqual("1.2", child2.Version);

            Assert.AreEqual(1, child2.Dependencies.Count);
            var child3 = child2.Dependencies[0];
            Assert.AreEqual("MyGroupId", child3.GroupId);
            Assert.AreEqual("MyArtifactId", child3.ArtifactId);
            Assert.AreEqual("1.2.3", child3.Version);

        }
    }
}
