using System;
using System.Collections.Generic;
using NRequire.Model;
using NUnit.Framework;
using TestFirst.Net;
using TestFirst.Net.Matcher;

namespace NRequire.Resolver {
    [TestFixture]
    public class ProjectTest {

        [Test]
        public void BuiltInDefaults() {
            var p = new Project();
            p.ProjectFormat = Project.SupportedVersion;
            p.TransitiveWishes.Add(new Wish { Group = "MyGroup", Name = "MyName" });
            p.RuntimeWishes.Add(new Wish { Group = "MyGroup2", Name = "MyName2" });
            p.OptionalWishes.Add(new Wish { Group = "MyGroup3", Name = "MyName3" });
            p.AfterLoad();

            Expect
                .That(p.TransitiveWishes)
                .Is(AList.WithOnly(AWish.With()
                    .Group("MyGroup").Name("MyName").Version("*").ExtNull().Scope(Scopes.Transitive).Classifiers("arch-any_runtime-any")));
            Expect
                .That(p.RuntimeWishes)
                .Is(AList.WithOnly(AWish.With()
                    .Group("MyGroup2").Name("MyName2").Version("*").ExtNull().Scope(Scopes.Runtime).Classifiers("arch-any_runtime-any")));
            Expect
                .That(p.OptionalWishes)
                .Is(AList.WithOnly(AWish.With()
                    .Group("MyGroup3").Name("MyName3").Version("*").ExtNull().Scope(Scopes.Transitive).Classifiers("arch-any_runtime-any")));
        }

        [Test]
        public void DefaultsSet() {

            foreach (var listGetter in new Func<Project,List<Wish>>[]{ p=>p.TransitiveWishes,p=>p.RuntimeWishes, p=>p.OptionalWishes}) {
                var p = new Project();
                p.ProjectFormat = Project.SupportedVersion;
                p.WishDefaults = new Wish() {
                    Group="Group", 
                    Name="Name", 
                    VersionString="1.2.3", 
                    Ext="Ext",
                    ClassifiersString="key-val"
                };

                var wishes = listGetter.Invoke(p);

                wishes.Add(new Wish { Name = "MyName1a"});
                wishes.Add(new Wish { Group = "MyGroup", Name = "MyName1b"});
                wishes.Add(new Wish { Group = "MyGroup", Name = "MyName1c", Version="1.0"});
                wishes.Add(new Wish { Group = "MyGroup", Name = "MyName1d", Version="1.0", Ext="MyExt"});
                wishes.Add(new Wish { Group = "MyGroup", Name = "MyName1e", Version="1.0", Ext="MyExt", ClassifiersString="MyKey-MyVal"});

                p.AfterLoad();

                var newWishes = listGetter.Invoke(p);

                Expect
                    .That(newWishes)
                        .Is(AList.InOrder()
                            .WithOnly(AWish.With().Group("Group").Name("MyName1a").Version("1.2.3").Ext("Ext").Classifiers("arch-any_key-val_runtime-any"))
                            .And(AWish.With().Group("MyGroup").Name("MyName1b").Version("1.2.3").Ext("Ext").Classifiers("arch-any_key-val_runtime-any"))
                            .And(AWish.With().Group("MyGroup").Name("MyName1c").Version("1.0").Ext("Ext").Classifiers("arch-any_key-val_runtime-any"))
                            .And(AWish.With().Group("MyGroup").Name("MyName1d").Version("1.0").Ext("MyExt").Classifiers("arch-any_key-val_runtime-any"))
                            .And(AWish.With().Group("MyGroup").Name("MyName1e").Version("1.0").Ext("MyExt").Classifiers("arch-any_mykey-myval_runtime-any")));


            }
        }


    }
}
