using System;
using System.Collections.Generic;
using System.Linq;
using Adform.Bloom.DataAccess.Extensions;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Bogus;
using Xunit;

namespace Adform.Bloom.Unit.Test
{
    public class BusinessAccountMappingExtensionsTests
    {
        private readonly Faker<BusinessAccountResult> _businessAccountFaker;
        public BusinessAccountMappingExtensionsTests()
        {
            _businessAccountFaker = new Faker<BusinessAccountResult>()
                .RuleFor(p => p.Id, f => Guid.NewGuid())
                .RuleFor(p => p.Name, f => f.Person.FullName)
                .RuleFor(p => p.LegacyId, f => f.Random.Int(0,1000))
                .RuleFor(p => p.Status, f => f.PickRandom<BusinessAccountStatus>())
                .RuleFor(p => p.Type, f => f.PickRandom<BusinessAccountType>());
        }

        [Fact]
        public void Can_Map_UserResult_To_Dto()
        {
            AssertMapFromReadModel();
        }

        private void AssertMapFromReadModel()
        {
            var node = _businessAccountFaker.Generate();
            var dto = node.MapFromReadModel();
            Assert.Equal(node.Id, dto.Id);
            Assert.Equal(node.Name, dto.Name);
            Assert.Equal(node.LegacyId, dto.LegacyId);
            Assert.Equal(node.Status, dto.Status);
            Assert.Equal(node.Type, dto.Type);
        }

        [Fact]
        public void Can_Map_Collection_Of_Features_To_Dtos()
        {
            AssertMapCollectionFromReadModel();
        }

        private void AssertMapCollectionFromReadModel()
        {
            var n1 = _businessAccountFaker.Generate();
            var n2 = _businessAccountFaker.Generate();
            var nodes = new List<BusinessAccountResult> { n1, n2 };
            var dtos = nodes.MapFromReadModel().ToList();

            Assert.Equal(nodes.Count, dtos.Count);
            for (var i = 0; i < nodes.Count; ++i)
            {
                var n = nodes[i];
                var dto = dtos[i];
                Assert.Equal(n.Id, dto.Id);
                Assert.Equal(n.Name, dto.Name);
                Assert.Equal(n.LegacyId, dto.LegacyId);
                Assert.Equal(n.Status, dto.Status);
                Assert.Equal(n.Type, dto.Type);
            }
        }
    }
}