using Adform.Bloom.Api;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using Adform.Bloom.Domain.Entities;
using Xunit;
using Feature = Adform.Bloom.Domain.Entities.Feature;
using Permission = Adform.Bloom.Domain.Entities.Permission;
using Policy = Adform.Bloom.Domain.Entities.Policy;
using Role = Adform.Bloom.Domain.Entities.Role;
using Tenant = Adform.Bloom.Domain.Entities.Tenant;

namespace Adform.Bloom.Unit.Test
{
    public class MappingExtensionsTests
    {
        [Fact]
        public void When_I_Try_To_Map_Unknown_LinkOperations_Value_Exception_Is_Thrown()
        {
            Assert.Throws<InvalidOperationException>(() => ((LinkOperation)8).MapToDomain());
        }

        [Fact]
        public void Assign_LinkOperation_Value_Can_Be_Mapped_To_Domain()
        {
            Assert.Equal(Bloom.Write.LinkOperation.Assign, LinkOperation.Assign.MapToDomain());
        }

        [Fact]
        public void Unassign_LinkOperation_Value_Can_Be_Mapped_To_Domain()
        {
            Assert.Equal(Bloom.Write.LinkOperation.Unassign, LinkOperation.Unassign.MapToDomain());
        }

        [Fact]
        public void Can_Map_Feature_To_Dto()
        {
            AssertMapFromDomain<Feature, Contracts.Output.Feature>();
        }

        [Fact]
        public void Can_Map_Permission_To_Dto()
        {
            AssertMapFromDomain<Permission, Contracts.Output.Permission>();
        }

        [Fact]
        public void Can_Map_Policy_To_Dto()
        {
            AssertMapFromDomain<Policy, Contracts.Output.Policy>();
        }

        [Fact]
        public void Can_Map_Role_To_Dto()
        {
            AssertMapFromDomain<Role, Contracts.Output.Role>();
        }

        [Fact]
        public void Can_Map_Tenant_To_Dto()
        {
            AssertMapFromDomain<Tenant, Contracts.Output.Tenant>();
        }

        private static void AssertMapFromDomain<TNode, TDto>()
            where TNode : NamedNode, new()
            where TDto : NamedNodeDto, new()
        {
            var node = new TNode
            {
                Id = Guid.NewGuid(),
                Name = DateTime.Now.Ticks.ToString(),
                Description = DateTime.Now.Ticks.ToString(),
                IsEnabled = false
            };
            var dto = node.MapFromDomain<TNode, TDto>();
            Assert.Equal(node.Id, dto.Id);
            Assert.Equal(node.Name, dto.Name);
            Assert.Equal(node.Description, dto.Description);
            Assert.Equal(node.IsEnabled, dto.Enabled);
        }

        [Fact]
        public void Can_Map_Collection_Of_Features_To_Dtos()
        {
            AssertMapCollectionFromDomain<Feature, Contracts.Output.Feature>();
        }

        [Fact]
        public void Can_Map_Collection_Of_Permissions_To_Dtos()
        {
            AssertMapCollectionFromDomain<Permission, Contracts.Output.Permission>();
        }

        [Fact]
        public void Can_Map_Collection_Of_Policies_To_Dtos()
        {
            AssertMapCollectionFromDomain<Policy, Contracts.Output.Policy>();
        }

        [Fact]
        public void Can_Map_Collection_Of_Roles_To_Dtos()
        {
            AssertMapCollectionFromDomain<Role, Contracts.Output.Role>();
        }

        [Fact]
        public void Can_Map_Collection_Of_Tenants_To_Dtos()
        {
            AssertMapCollectionFromDomain<Tenant, Contracts.Output.Tenant>();
        }

        private static void AssertMapCollectionFromDomain<TNode, TDto>()
            where TNode : NamedNode, new()
            where TDto : NamedNodeDto, new()
        {
            var n1 = new TNode
            {
                Id = Guid.NewGuid(),
                Name = DateTime.Now.Ticks.ToString(),
                Description = DateTime.Now.Ticks.ToString(),
                IsEnabled = false
            };

            var n2 = new TNode
            {
                Id = Guid.NewGuid(),
                Name = DateTime.Now.Ticks.ToString(),
                Description = DateTime.Now.Ticks.ToString(),
                IsEnabled = false
            };

            var nodes = new List<TNode> { n1, n2 };
            var dtos = nodes.MapFromDomain<TNode, TDto>().ToList();

            Assert.Equal(nodes.Count, dtos.Count);
            for (var i = 0; i < nodes.Count; ++i)
            {
                var n = nodes[i];
                var dto = dtos[i];
                Assert.Equal(dto.Id, n.Id);
                Assert.Equal(dto.Name, n.Name);
                Assert.Equal(dto.Description, n.Description);
                Assert.Equal(dto.Enabled, n.IsEnabled);
            }
        }
    }
}