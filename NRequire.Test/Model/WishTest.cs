using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace NRequire.Model {

    [TestFixture]
    public class WishTest {

        [Test]
        public void CloneEmptyDepTest() {
            var dep = new Wish();
            var clone = dep.Clone();

            Assert.AreNotSame(dep, clone);
            Assert.NotNull(clone);
        }

        [Test]
        public void CloneSimplePropsTest() {

            var dep = new Wish {
                Name = "MyName",
                Group = "MyGroupId",
                Runtime = "MyRuntime",
                Ext = "MyExt",
                Arch = "MyArch",
                VersionString = "1.2.3",
                CopyTo = "/path/to/copy/to",
                Url = new Uri("http://nowhere.com/file"),
                Scope = Scopes.Provided
            };

            var clone = dep.Clone();

            Assert.AreNotSame(dep, clone);
            Assert.AreEqual("MyName", clone.Name);
            Assert.AreEqual("MyGroupId", clone.Group);
            Assert.AreEqual("MyExt", clone.Ext);
            Assert.AreEqual("myruntime", clone.Runtime);
            Assert.AreEqual("myarch", clone.Arch);
            Assert.AreEqual("http://nowhere.com/file", clone.Url.ToString());
            Assert.AreEqual("1.2.3", clone.VersionString);
            Assert.AreEqual("/path/to/copy/to", clone.CopyTo);
            Assert.AreEqual(Scopes.Provided, clone.Scope);
        }

        [Test]
        public void CloneChildDependenciesTest() {

            var dep = new Wish {
                TransitiveWishes = new List<Wish> { 
                    new Wish{
                        Name = "MyName1"
                    },
                    new Wish{
                        Name = "MyName2"
                    },
                }
            };

            var clone = dep.Clone();

            Assert.AreNotSame(dep, clone);
            Assert.AreNotSame(dep.TransitiveWishes, clone.TransitiveWishes);

            Assert.AreEqual(dep.TransitiveWishes, clone.TransitiveWishes);
        }


        [Test]
        public void MergeSimplePropsTest() {

            var wish = new Wish {
                Name = "MyName",
                Group = "MyGroupId",
                Runtime = "MyRuntime",
                Ext = "MyExt",
                Arch = "MyArch",
                VersionString = "1.2.3",
                ClassifiersString = "key_val",
                CopyTo = "path/to/copy/to",
                Url = new Uri("http://nowhere.com/file"),
                Scope = Scopes.Provided
            };

            CheckMergeProp(wish, (d, val) => d.Group = val, d => d.Group, "", "MyParentVal", "MyChildVal");
            CheckMergeProp(wish, (d, val) => d.Name = val, d => d.Name, "", "MyParentVal", "MyChildVal");
            CheckMergeProp(wish, (d, val) => d.Ext = val, d => d.Ext, "", "MyParentVal", "MyChildVal");
            CheckMergeProp(wish, (d, val) => d.VersionString = val, d => d.VersionString, null, "1.2.3", "4.5.6");
            CheckMergeProp(wish, (d, val) => d.CopyTo = val, d => d.CopyTo, "", "MyParentVal", "MyChildVal");
            CheckMergeProp(wish, (d, val) => d.Url = val, d => d.Url, null, new Uri("http://nowhere/myparent"), new Uri("http://nowhere/mychild"));
            
            //these all go via the classifiers which forces lowercase on everything
            CheckMergeProp(wish, (d, val) => d.Arch = val, d => d.Arch, "", "myparentval", "mychildval");
            CheckMergeProp(wish, (d, val) => d.Runtime = val, d => d.Runtime, "", "myparentval", "mychildval");
            CheckMergeProp(wish, (d, val) => d.ClassifiersString = val, d => d.ClassifiersString, null, "parent_pval", "child_cval");
            //CheckMergeProp(wish, (d, val) => d.Scope = val, d => d.Scope, Scopes.Compile, Scopes.Runtime, Scopes.Provided);
        }

        private static IList<String> List(params String[] vals) {
            return new List<String>(vals);
        }

        private void CheckMergeProp<T>(Wish wish, Action<Wish, T> setter, Func<Wish, T> getter, T emptyVal, T parentSetterVal,T childSetterVal) {
            try {
                var parent = wish.Clone();
                var child = wish.Clone();

                setter.Invoke(parent, parentSetterVal);
                setter.Invoke(child, emptyVal);
                var merged = child.CloneAndFillInBlanksFrom(parent);
                Assert.AreEqual(getter.Invoke(merged), parentSetterVal);

                setter.Invoke(parent, parentSetterVal);
                setter.Invoke(child, childSetterVal);
                merged = child.CloneAndFillInBlanksFrom(parent);
                Assert.AreEqual(getter.Invoke(merged), childSetterVal);

                setter.Invoke(parent, emptyVal);
                setter.Invoke(child, childSetterVal);
                merged = child.CloneAndFillInBlanksFrom(parent);
                Assert.AreEqual(getter.Invoke(merged), childSetterVal);
            } catch(Exception e){
                throw new Exception( "Exception while testing merge", e);
            }
        }

        [Test]
        public void MergeChildDependenciesTest() {

            var parent = new Wish {
                Name = "MyArtifactId",
                Group = "MyGroupId",

                TransitiveWishes = new List<Wish> { 
                    new Wish{
                        Group = "MyGroupId1",
                        Name = "MyArtifactId1", 
                        VersionString = "1.2.3"
                    },
                    new Wish{
                        Group = "MyGroupId2",
                        Name = "MyArtifactId2", 
                        VersionString = "1.2.4"
                    },
                }
            };

            var child = new Wish {
                Name = "MyArtifactId",
                Group = "MyGroupId",

                TransitiveWishes = new List<Wish> {
                    //overrides all from parent
                    new Wish{
                        Group = "MyGroupId1",
                        Name = "MyArtifactId1", 
                        VersionString = "1.2.2"
                    },
                    //parent supplies the version
                    new Wish{
                        Group = "MyGroupId2",
                        Name = "MyArtifactId2"
                    },

                    //additional
                    new Wish{
                        Group = "MyGroupId3",
                        Name = "MyArtifactId3",
                        VersionString = "4.5.6" 
                    },
                }
            };

            var merged = child.CloneAndFillInBlanksFrom(parent);

            Assert.AreEqual(3, merged.TransitiveWishes.Count);
            var child1 = merged.TransitiveWishes[0];
            Assert.AreEqual("MyGroupId1",child1.Group);
            Assert.AreEqual("MyArtifactId1",child1.Name);
            Assert.AreEqual("1.2.2",child1.VersionString);
            
            var child2 = merged.TransitiveWishes[1];
            Assert.AreEqual("MyGroupId2", child2.Group);
            Assert.AreEqual("MyArtifactId2", child2.Name);
            Assert.AreEqual("1.2.4", child2.VersionString);

            var child3 = merged.TransitiveWishes[2];
            Assert.AreEqual("MyGroupId3", child3.Group);
            Assert.AreEqual("MyArtifactId3", child3.Name);
            Assert.AreEqual("4.5.6", child3.VersionString);

        }

        [Test]
        public void MergeChildChildDependenciesTest() {

            var parent = new Wish {
                TransitiveWishes = new List<Wish> { 
                    new Wish{
                        Group = "MyGroupId",
                        Name = "MyArtifactId", 
                        VersionString = "1",
                        TransitiveWishes = new List<Wish> { 
                            new Wish{
                                Group = "MyGroupId",
                                Name = "MyArtifactId", 
                                VersionString = "1.2",
                                TransitiveWishes = new List<Wish> { 
                                    new Wish{
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

            var dep = new Wish {
                TransitiveWishes = new List<Wish> { 
                    new Wish{
                        Group = "MyGroupId",
                        Name = "MyArtifactId", 
                        TransitiveWishes = new List<Wish> { 
                            new Wish{
                                Group = "MyGroupId",
                                Name = "MyArtifactId",
                                TransitiveWishes = new List<Wish> { 
                                    new Wish{
                                        Group = "MyGroupId",
                                        Name = "MyArtifactId"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var merged = dep.CloneAndFillInBlanksFrom(parent);

            Assert.AreEqual(1, merged.TransitiveWishes.Count);
            var child = merged.TransitiveWishes[0];
            Assert.AreEqual("MyGroupId", child.Group);
            Assert.AreEqual("MyArtifactId", child.Name);
            Assert.AreEqual("1", child.VersionString);

            Assert.AreEqual(1, child.TransitiveWishes.Count);
            var child2 = child.TransitiveWishes[0];
            Assert.AreEqual("MyGroupId", child2.Group);
            Assert.AreEqual("MyArtifactId", child2.Name);
            Assert.AreEqual("1.2", child2.VersionString);

            Assert.AreEqual(1, child2.TransitiveWishes.Count);
            var child3 = child2.TransitiveWishes[0];
            Assert.AreEqual("MyGroupId", child3.Group);
            Assert.AreEqual("MyArtifactId", child3.Name);
            Assert.AreEqual("1.2.3", child3.VersionString);

        }
    }
}
