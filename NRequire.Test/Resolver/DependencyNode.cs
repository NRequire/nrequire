using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NRequire.Matcher;
using System.Collections.Generic;
using System.Collections;

namespace NRequire
{
	//represents a dependency and it's requirements
    public class DependencyNode : Dependency
    {
        public List<DependencyWish> Wishes { get; set; }

        public DependencyNode()
        {
            Group = "group";
            Wishes = new List<DependencyWish>();
        }

        public DependencyNode Wish(String name, String version)
        {
            Wish(new DependencyWish{ Name = name, Group = "group", Version = VersionMatcher.Parse(version)});
            return this;
        }

        public DependencyNode Wish(DependencyWish wish)
        {
            Wishes.Add(wish);
            return this;
        }
    }
}

