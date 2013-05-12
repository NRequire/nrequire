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
    public class Node : Dependency
    {
		
        public List<DependencyWish> Dependencies{ get; set; }

        public Node(String id)
        {
            Name = id;
            Group = "group";
            Dependencies = new List<DependencyWish>();
        }

        public Node Requires(String name, String version)
        {
            Requires(new DependencyWish{ Name = name, Group = "group", Version = VersionMatcher.Parse(version)});
            return this;
        }

        public Node Requires(DependencyWish wish)
        {
            Dependencies.Add(wish);
            return this;
        }
    }
}

