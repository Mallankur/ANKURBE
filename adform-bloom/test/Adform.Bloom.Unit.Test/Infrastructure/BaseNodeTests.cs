using System;
using Adform.Bloom.Domain.Entities;
using Xunit;

namespace Adform.Bloom.Unit.Test.Infrastructure
{
    public class BaseNodeTests
    {
        private class FakeNode : NamedNode
        {
            public FakeNode(string name) : base(name)
            { }
        }

        [Fact]
        public void New_Node_Should_Have_CreatedAt_And_UpdatedAt_Set_To_Unix_Default()
        {
            var timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
            var timestampTwo = timestamp + 30;
            var node = new FakeNode(string.Empty);
            Assert.InRange(node.CreatedAt,timestamp,timestampTwo);
            Assert.InRange(node.UpdatedAt, timestamp,timestampTwo);
        }

        [Fact]
        public void New_Node_Should_Be_Enabled_By_Default()
        {
            var node = new FakeNode(string.Empty);
            Assert.True(node.IsEnabled);
        }

        [Fact]
        public void Create_Node_With_Empty_Name()
        {
            var node = new FakeNode(string.Empty);
            Assert.Empty(node.Name);
        }

        [Fact]
        public void Create_Node_With_Non_Empty_Name()
        {
            var node = new FakeNode("aaa");
            Assert.Equal("aaa", node.Name);
        }
    }
}