//
//  Copyright 2013  bert
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Linq;
using System.Linq.Expressions;

using NUnit.Framework;
using NRequire.Matcher;
using System.Collections.Generic;
using System.Collections;

namespace NRequire
{
	//holder for a list of nodes plus some convenience methods above those provided
	//by a list
	public class NodeList : IEnumerable<Node>{
		private IDictionary<String,Node> m_nodes = new Dictionary<String,Node>();
		
		public List<Node> ToList(){
			return new List<Node>(this);
		}
		
		public Node GetOrFail(String name){
			var node = Get (name);
			if( node == null ){
				throw new InvalidOperationException("No node with id" + name);
			}
			return node;
		}
		
		public Node Get(String name){
			Node node;
			if( m_nodes.TryGetValue(name,out node)){
				return node;
			}
			return null;
		}
		
		public void AddAll(NodeList nodes){
			foreach(var node in nodes){
				Add (node);
			}
		}
		
		public void Add(Node node){
			m_nodes.Add(node.Name, node);
		}
		
		public bool HasNode(String name){
			return m_nodes.ContainsKey(name);
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
}

