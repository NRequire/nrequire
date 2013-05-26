using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NRequire.Matcher;

namespace NRequire {
    [TestFixture]
    public class SolutionTest {

        [Test]
        public void BuiltInDefaults() {
            var soln = new Solution();
            soln.SolutionFormat = Solution.SupportedVersion;
            soln.Wishes.Add(new Wish { Group = "MyGroup", Name = "MyName" });
            soln.AfterLoad();

            Expect
                .That(soln.Wishes)
                .Is(AList.WithOnly(AWish.With()
                    .Group("MyGroup").Name("MyName").Version(AString.Null()).NullExt().Scope(Scopes.Transitive).Classifiers("arch-any_runtime-any")));
        }

        [Test]
        public void DefaultsSet() {

            var soln = new Solution();
            soln.SolutionFormat = Solution.SupportedVersion;
            soln.WishDefaults = new Wish() {
                Group="Group", 
                Name="Name", 
                VersionString="1.2.3", 
                Ext="Ext",
                ClassifiersString="key-val"
            };

            var wishes = soln.Wishes;

            wishes.Add(new Wish { Name = "MyName1a"});
            wishes.Add(new Wish { Group = "MyGroup", Name = "MyName1b"});
            wishes.Add(new Wish { Group = "MyGroup", Name = "MyName1c", Version="1.0"});
            wishes.Add(new Wish { Group = "MyGroup", Name = "MyName1d", Version="1.0", Ext="MyExt"});
            wishes.Add(new Wish { Group = "MyGroup", Name = "MyName1e", Version="1.0", Ext="MyExt", ClassifiersString="MyKey-MyVal"});

            soln.AfterLoad();

            Expect
                .That(soln.Wishes)
                    .Is(AList.InOrder()
                        .With(AWish.With().Group("Group").Name("MyName1a").Version("1.2.3").Ext("Ext").Classifiers("arch-any_key-val_runtime-any"))
                        .And(AWish.With().Group("MyGroup").Name("MyName1b").Version("1.2.3").Ext("Ext").Classifiers("arch-any_key-val_runtime-any"))
                        .And(AWish.With().Group("MyGroup").Name("MyName1c").Version("1.0").Ext("Ext").Classifiers("arch-any_key-val_runtime-any"))
                        .And(AWish.With().Group("MyGroup").Name("MyName1d").Version("1.0").Ext("MyExt").Classifiers("arch-any_key-val_runtime-any"))
                        .And(AWish.With().Group("MyGroup").Name("MyName1e").Version("1.0").Ext("MyExt").Classifiers("arch-any_mykey-myval_runtime-any")));

        }
    }
}
