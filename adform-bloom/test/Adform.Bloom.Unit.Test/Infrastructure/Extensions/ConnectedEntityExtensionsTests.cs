using System;
using System.Collections.Generic;
using System.Linq;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure.Extensions;
using Xunit;

namespace Adform.Bloom.Unit.Test.Infrastructure.Extensions
{
    public class ConnectedEntityExtensionsTests
    {
        [Fact]
        public void ToGraphQlFriendlyDictionary_Creates_Dictionary_With_Key_And_Nodes()
        {
            var key = Guid.NewGuid();
            var nodes = new List<ConnectedEntity<Node>>
            {
                new ConnectedEntity<Node> {StartNodeId = key, ConnectedNode = new Node("1")},
                new ConnectedEntity<Node> {StartNodeId = key, ConnectedNode = new Node("2")},
                new ConnectedEntity<Node> {StartNodeId = key, ConnectedNode = new Node("3")}
            };

            var dic = nodes.ToGraphQlFriendlyDictionary(x => new MappedNode());
            
            Assert.True(dic.ContainsKey(key));
            Assert.True(dic.Count == 1);
            Assert.True(dic[key].Count == 3);
            Assert.True(dic[key].All(y => y != null));
        }
        
        [Fact]
        public void ToGraphQlFriendlyDictionary_Creates_Dictionary_With_Key_And_Empty_List()
        {
            var key = Guid.NewGuid();
            var nodes = new List<ConnectedEntity<Node>>
            {
                new ConnectedEntity<Node> {StartNodeId = key, ConnectedNode = null}
            };

            var dic = nodes.ToGraphQlFriendlyDictionary(x => new MappedNode());
            
            Assert.True(dic.ContainsKey(key));
            Assert.True(dic.Count == 1);
            Assert.True(dic[key].Count == 0);
        }
        
        [Fact]
        public void ToGraphQlFriendlyDictionary_Creates_Dictionary_With_Key_And_Combined_List()
        {
            var key1 = Guid.NewGuid();
            var key2 = Guid.NewGuid();
            
            var nodes = new List<ConnectedEntity<Node>>
            {
                new ConnectedEntity<Node> {StartNodeId = key1, ConnectedNode = null},
                new ConnectedEntity<Node> {StartNodeId = key2, ConnectedNode = new Node("1")},
                new ConnectedEntity<Node> {StartNodeId = key2, ConnectedNode = new Node("2")},
                new ConnectedEntity<Node> {StartNodeId = key2, ConnectedNode = new Node("3")}
            };

            var dic = nodes.ToGraphQlFriendlyDictionary(x => new MappedNode());
            
            Assert.True(dic.ContainsKey(key1));
            Assert.True(dic.Count == 2);
            Assert.True(dic[key1].Count == 0);
            
            Assert.True(dic.ContainsKey(key2));
            Assert.True(dic[key2].Count == 3);
            Assert.True(dic[key2].All(y => y != null));
        }
    }

    internal class Node : NamedNode
    {
        public Node(string nodeName) : base(nodeName)
        {
        }
    }

    internal class MappedNode
    {
        
    }
}