using Adform.Ciam.Cache.Extensions;
using Bogus;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Adform.Bloom.Integration.Test.Cache
{
    public class TestEntity
    {
        public string Name { get; set; }
        public IEnumerable<string> Types { get; set; }
    }

    [Collection(nameof(CacheCollection))]
    public class CacheTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;

        public CacheTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(GenerateCachePayloads))]
        public async Task GetAsync(TestEntity entity)
        {
            var cached = await _fixture.Cache.GetAsync<TestEntity>(entity.Name, CancellationToken.None);
            Assert.Null(cached);
            await _fixture.Cache.SetAsync(entity.Name, entity, new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(60)
            });
            cached = await _fixture.Cache.GetAsync<TestEntity>(entity.Name, CancellationToken.None);
            Assert.NotNull(cached);
        }

        public static TheoryData<TestEntity> GenerateCachePayloads()
        {
            var types = Enumerable.Range(0, 5)
                .Select(n => new Faker().Random.Guid().ToString())
                .ToList();
            var data = new TheoryData<TestEntity>();
            var fakeResult = new Faker<TestEntity>();
            fakeResult.RuleFor(p => p.Name, o => $"{Guid.NewGuid()}_TEST");
            fakeResult.RuleFor(p => p.Types, types);
            var results = fakeResult.Generate(10);
            foreach (var item in results)
            {
                data.Add(item);
            }

            return data;
        }
    }
}