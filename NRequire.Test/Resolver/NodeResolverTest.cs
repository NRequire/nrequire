using System;
using System.Linq;
using System.Linq.Expressions;

using NUnit.Framework;
using NRequire.Matcher;
using System.Collections.Generic;
using System.Collections;

namespace NRequire
{
	[TestFixtureAttribute]
	public class NodeResolverTest
	{
		[Test]
		public void ResolveSimple()
		{
			var freeNodes = new NodeList();
			freeNodes.GetOrAdd("A").DependsOn("C");
			freeNodes.GetOrAdd("B").DependsOn("D");

			var allNodes = new NodeList();
			allNodes.AddAll(freeNodes);
			allNodes.GetOrAdd("C");
			allNodes.GetOrAdd("D").DependsOn("E");
			allNodes.GetOrAdd("E");
			allNodes.GetOrAdd("F");


			var resolver = new NodeResolver(allNodes);

			var resolved = resolver.Resolve(freeNodes);

			AssertAreEqual(allNodes.GetAll("A","B","C","D","E"),resolved);

		}

		private void AssertAreEqual(NodeList expect, NodeList actual){
			var expectList = expect.OrderBy(n=>n.Id).Select(n=>n.Id).ToList();
			var actualList = actual.OrderBy(n=>n.Id).Select(n=>n.Id).ToList();

			try {
				Assert.AreEqual(expectList,actualList);
			} catch(AssertionException e){
				throw new AssertionException(String.Format("Failed match, expected [{0}], but got [{1}]", String.Join(",",expectList), String.Join(",",actualList)),e);
			}
		}

		public class NodeList : IEnumerable<Node>{
			private IDictionary<String,Node> m_nodes = new Dictionary<String,Node>();

			
			public Node GetOrAdd(String nodeId){
				var node = Get (nodeId);
				if( node == null ){
					node = Add(nodeId);
				}
				return node;
			}

			public NodeList GetAll(params String[] nodeIds){
				var nodes = new NodeList();
				foreach(var id in nodeIds){
					var node = GetOrFail(id);
					nodes.Add(node);
				}
				return nodes;
			}

			public List<Node> ToList(){
				return new List<Node>(this);
			}

			public Node GetOrFail(String nodeId){
				var node = Get (nodeId);
				if( node == null ){
					throw new InvalidOperationException("No node with id" + nodeId);
				}
				return node;
			}

			public Node Get(String nodeId){
				Node node;
				if( m_nodes.TryGetValue(nodeId,out node)){
					return node;
				}
				return null;
			}

			public void AddAll(NodeList nodes){
				foreach(var node in nodes){
					Add (node);
				}
			}

			public Node Add(String nodeId){
				var node = new Node(nodeId);
				Add(node);
				return node;
			}

			public void Add(Node node){
				m_nodes.Add(node.Id, node);
			}

			public bool HasNode(String id){
				return m_nodes.ContainsKey(id);
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
			{
				return GetEnumerator();
			}

			public IEnumerator<Node> GetEnumerator ()
			{
				return m_nodes.Values.GetEnumerator();
			}
		}

		public class Node {
			public String Id{ get;set;}
			public List<string> Dependencies{ get;set;}

			public Node(String id){
				Id = id;
				Dependencies = new List<string>();
			}

			public Node DependsOn(string nodeId){
				Dependencies.Add(nodeId);
				return this;
			}

			public override String ToString(){
				return Id;
			}
		}

		public class NodeResolver {
			private NodeList m_allNodes;
			public NodeResolver(NodeList allNodes)
			{
				m_allNodes = allNodes;
			}

			public NodeList Resolve(NodeList freeNodes)
			{
				var resolved = new NodeList();
				foreach(var node in freeNodes){
					InternalResolve(node.Id,resolved);
				}
				return resolved;
			}

			private void InternalResolve(String nodeId, NodeList resolved){
				if(!resolved.HasNode(nodeId)){
					var dep = Lookup(nodeId);
					resolved.Add(dep);
					foreach(var id in dep.Dependencies){
						InternalResolve(id,resolved);
					}
				}
			}

			private Node Lookup(String nodeId){
				return m_allNodes.GetOrFail(nodeId);
			}
		}
	}
}

