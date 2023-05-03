using System;
using System.Linq;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Extensions;
using Xunit;

namespace Adform.Bloom.Unit.Test.DataAccess
{
    public class ExtensionsTests
    {
        [Fact]
        public void ToEntityPagination_Returns_Expected_Result()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var queryResult = new[]
            {
                new CypherPaginationResult<Role>
                {
                    Node = new Role
                    {
                        Id = guid
                    },
                    TotalCount = 0
                },
                new CypherPaginationResult<Role>
                {
                    Node = null,
                    TotalCount = 1
                }
            };

            // Act
            var paginated = queryResult.ToEntityPagination(0, 10);

            // Assert
            Assert.Equal(1, paginated.TotalItems);
            Assert.Equal(guid, paginated.Data.First().Id);
            Assert.Equal(1, paginated.Data.Count);
        }

        [Fact]
        public void ToEntityPagination_Returns_Expected_Result_Multiple()
        {
            // Arrange
            const int count = 10;
            var guids = Enumerable.Range(0, count).Select(x => Guid.NewGuid()).ToList();
            var queryResult = guids.Select(x => new CypherPaginationResult<Role>
            {
                Node = new Role
                {
                    Id = x
                }
            }).ToList();
            queryResult.Add(new CypherPaginationResult<Role>
            {
                Node = null,
                TotalCount = count
            });

            // Act
            var paginated = queryResult.ToEntityPagination(0, 10);

            // Assert
            Assert.Equal(count, paginated.TotalItems);
            Assert.True(paginated.Data.Select(x => x.Id).SequenceEqual(guids));
            Assert.Equal(count, paginated.Data.Count);
        }
    }
}