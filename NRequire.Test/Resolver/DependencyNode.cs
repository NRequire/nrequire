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

        public DependencyNode(String id)
        {
            Name = id;
            Group = "group";
            Wishes = new List<DependencyWish>();
        }

        public DependencyNode Requires(String name, String version)
        {
            Requires(new DependencyWish{ Name = name, Group = "group", Version = VersionMatcher.Parse(version)});
            return this;
        }

        public DependencyNode Requires(DependencyWish wish)
        {
            Wishes.Add(wish);
            return this;
        }
    }
}

