using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Runtime.Contracts.Response;
using Aerospike.Client;
using Bogus;
using Xunit;

namespace Adform.Bloom.Integration.Test.Cache
{
    [Collection(nameof(CacheCollection))]
    public class BloomCacheManagerTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;

        public BloomCacheManagerTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task BloomCacheManager_VariousOperations_ShouldBehaveCorrectly()
        {
            //Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var generated = GenerateCachePayloads();

            //Act & Assert
            await _fixture.CacheManager.FlushAsync(cancellationToken);
            await TestRememberAsyncAndFetchAsync(generated, cancellationToken);
            await TestForgetAsyncAndFetchAsync(generated, "roles0", cancellationToken);
            await TestFlush(cancellationToken);
        }

        private async Task TestRememberAsyncAndFetchAsync(Dictionary<Guid, List<RuntimeResponse>> generated, CancellationToken cancellationToken)
        {
            var expiration = TimeSpan.FromDays(1);
            foreach (var item in generated)
            {
                await _fixture.CacheManager.RememberAsync(item.Key.ToString(), item.Value, expiration,
                    cancellationToken);
                var cachedItem = await _fixture.CacheManager.FetchAsync(item.Key.ToString(), cancellationToken);

                Assert.Equal(item.Value.Count, cachedItem.Count());
                Assert.True(item.Value.Select(p => p.TenantId).SequenceEqual(cachedItem.Select(p => p.TenantId)));
                Assert.True(item.Value.SelectMany(p => p.Roles).SequenceEqual(cachedItem.SelectMany(p => p.Roles)));
                Assert.True(item.Value.SelectMany(p => p.Permissions).SequenceEqual(cachedItem.SelectMany(p => p.Permissions)));
            }
        }

        private async Task TestForgetAsyncAndFetchAsync(Dictionary<Guid, List<RuntimeResponse>> generated, string role, CancellationToken cancellationToken)
        {
            var currentState = GetRecordsCount();
            Assert.Equal(generated.Values.Count, currentState);
            await _fixture.CacheManager.ForgetByRoleAsync(role, cancellationToken);
            var newState = GetRecordsCount();
            Assert.Equal(currentState - generated.Values.Where(o => o[0].Roles.Contains(role)).Count(), newState);
            var lastKey = generated.Keys.Last().ToString();
            await _fixture.CacheManager.ForgetBySubjectAsync(lastKey, cancellationToken);
            var deleteItem = await _fixture.CacheManager.FetchAsync(lastKey, cancellationToken);
            Assert.Null(deleteItem);
        }

        private async Task TestFlush(CancellationToken cancellationToken)
        {
            var currentState = GetRecordsCount();
            await _fixture.CacheManager.FlushAsync(cancellationToken);
            var newState = GetRecordsCount();
            Assert.NotEqual(currentState, newState);
            Assert.Equal(0, newState);
        }

        private int GetRecordsCount()
        {
            var stmt = new Statement();
            stmt.SetNamespace(_fixture.CacheConfig.Namespace);
            stmt.SetSetName(_fixture.CacheConfig.Set);
            var rs = _fixture.CacheConnection.Client.Query(null, stmt);

            var count = 0;
            try
            {
                while (rs.Next())
                {
                    count++;
                }
            }
            finally
            {
                rs.Close();
            }

            return count;
        }

        public Dictionary<Guid, List<RuntimeResponse>> GenerateCachePayloads()
        {
            var data = new Dictionary<Guid, List<RuntimeResponse>>();
            var roles = new List<string> { "roles0", "roles1", "roles2", "roles3" };
            var permissions = new List<string>
            {
                "permission0", "permission1", "permission2", "permission3", "permission4", "permission5", "permission6"
            };
            var subjects = Enumerable.Range(0, 4)
                .Select(n => new Faker().Random.Guid())
                .ToList();
            var tenants = Enumerable.Range(0, 3)
                .Select(n => new Faker().Random.Guid())
                .ToList();


            data.Add(subjects[0], new List<RuntimeResponse>()
            {
                new RuntimeResponse
                {
                    TenantId = tenants[0],
                    Permissions = new List<string>
                    {
                        permissions[0],
                        permissions[1],
                        permissions[3],
                        permissions[4]
                    },
                    Roles = new List<string>
                    {
                        roles[0],
                        roles[2]
                    }
                }
            });

            data.Add(subjects[1], new List<RuntimeResponse>()
            {
                new RuntimeResponse
                {
                    TenantId = tenants[0],
                    Permissions = new List<string>
                    {
                        permissions[0],
                        permissions[1],
                        permissions[2]
                    },
                    Roles = new List<string>
                    {
                        roles[1]
                    }
                }
            });

            data.Add(subjects[2], new List<RuntimeResponse>()
            {
                new RuntimeResponse
                {
                    TenantId = tenants[1],
                    Permissions = new List<string>
                    {
                        permissions[0],
                        permissions[1],
                        permissions[2]
                    },
                    Roles = new List<string>
                    {
                        roles[0],
                        roles[1]
                    }
                }
            });
            data.Add(subjects[3], new List<RuntimeResponse>()
            {
                new RuntimeResponse
                {
                    TenantId = tenants[2],
                    Permissions = new List<string>
                    {
                        permissions[5],
                        permissions[6]
                    },
                    Roles = new List<string>
                    {
                        roles[3]
                    }
                }
            });

            return data;
        }
    }
}