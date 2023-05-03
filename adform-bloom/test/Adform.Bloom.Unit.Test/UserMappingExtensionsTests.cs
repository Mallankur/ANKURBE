using Adform.Bloom.DataAccess.Extensions;
using Adform.Bloom.Read.Contracts.User;
using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Adform.Bloom.Unit.Test
{
    public class UserMappingExtensionsTests
    {
        private readonly Faker<UserResult> _userFaker;
        public UserMappingExtensionsTests()
        {
            _userFaker = new Faker<UserResult>()
                .RuleFor(p => p.Id, f => Guid.NewGuid())
                .RuleFor(p => p.Username, f => f.Person.UserName)
                .RuleFor(p => p.Email, f => f.Person.Email)
                .RuleFor(p => p.Name, f => f.Person.FullName)
                .RuleFor(p => p.Phone, f => f.Person.Phone)
                .RuleFor(p => p.Locale, f => f.Locale)
                .RuleFor(p => p.Timezone, f => f.Date.TimeZoneString());
        }

        [Fact]
        public void Can_Map_UserResult_To_Dto()
        {
            AssertMapFromReadModel();
        }

        private void AssertMapFromReadModel()
        {
            var node = _userFaker.Generate();
            var dto = node.MapFromReadModel();
            Assert.Equal(node.Id, dto.Id);
            Assert.Equal(node.Username, dto.Username);
            Assert.Equal(node.Email, dto.Email);
            Assert.Equal(node.Phone, dto.Phone);
            Assert.Equal(node.Locale, dto.Locale);
            Assert.Equal(node.Timezone, dto.Timezone);
        }

        [Fact]
        public void Can_Map_Collection_Of_Features_To_Dtos()
        {
            AssertMapCollectionFromReadModel();
        }

        private void AssertMapCollectionFromReadModel()
        {
            var n1 = _userFaker.Generate();
            var n2 = _userFaker.Generate();
            var nodes = new List<UserResult> { n1, n2 };
            var dtos = nodes.MapFromReadModel().ToList();

            Assert.Equal(nodes.Count, dtos.Count);
            for (var i = 0; i < nodes.Count; ++i)
            {
                var n = nodes[i];
                var dto = dtos[i];
                Assert.Equal(n.Id, dto.Id);
                Assert.Equal(n.Username, dto.Username);
                Assert.Equal(n.Email, dto.Email);
                Assert.Equal(n.Phone, dto.Phone);
                Assert.Equal(n.Locale, dto.Locale);
                Assert.Equal(n.Timezone, dto.Timezone);
            }
        }
    }
}