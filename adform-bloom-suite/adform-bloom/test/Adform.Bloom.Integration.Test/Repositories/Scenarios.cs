using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.DataAccess.Repositories;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Runtime.Contracts.Response;
using Xunit;

namespace Adform.Bloom.Integration.Test.Repositories
{
    public static class Scenarios
    {
        public static TheoryData<string, FilterFeatureIdsWithAccessDeniedScenario>
            FilterFeatureIdsWithAccessDeniedFailureScenario()
        {
            var features = Graph.GetFeatures();
            var data = new TheoryData<string, FilterFeatureIdsWithAccessDeniedScenario>
            {
                {
                    "Case 0",
                    new FilterFeatureIdsWithAccessDeniedScenario
                    {
                        Identity = Common.Test.Common.BuildUser(new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant1),
                            TenantName = Graph.Tenant1Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        }),
                        Features = new[] {features[6]}, TenantIds = null,
                        Callback = r => r.Count == 1
                    }
                },
                {
                    "Case 1",
                    new FilterFeatureIdsWithAccessDeniedScenario
                    {
                        Identity = Common.Test.Common.BuildUser(new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant6),
                            TenantName = Graph.Tenant6Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        }),
                        Features = new[] {features[0], features[1]},
                        TenantIds = null,
                        Callback = r => r.Count == 2
                    }
                },
                {
                    "Case 2",
                    new FilterFeatureIdsWithAccessDeniedScenario
                    {
                        Identity = Common.Test.Common.BuildUser(new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant4),
                            TenantName = Graph.Tenant4Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        }),
                        Features = features,
                        TenantIds = null,
                        Callback = r => r.Count == 7
                    }
                },
                {
                    "Case 3",
                    new FilterFeatureIdsWithAccessDeniedScenario
                    {
                        Identity = Common.Test.Common.BuildUser(new[]
                        {
                            new RuntimeResponse
                            {
                                TenantId = Guid.Parse(Graph.Tenant1),
                                TenantName = Graph.Tenant1Name,
                                Roles = new[] {Graph.OtherRole},
                                Permissions = new[] {""}
                            },
                            new RuntimeResponse
                            {
                                TenantId = Guid.Parse(Graph.Tenant4),
                                TenantName = Graph.Tenant4Name,
                                Roles = new[] {Graph.OtherRole},
                                Permissions = new[] {""}
                            }
                        }),
                        Features = new[] {features[0], features[1], features[2], features[3], features[4]},
                        TenantIds = new[] {Guid.Parse(Graph.Tenant1)},
                        Callback = r => r.Count == 3
                    }
                },
                {
                    "Case 4",
                    new FilterFeatureIdsWithAccessDeniedScenario
                    {
                        Identity = Common.Test.Common.BuildUser(new[]
                        {
                            new RuntimeResponse
                            {
                                TenantId = Guid.Parse(Graph.Tenant6),
                                TenantName = Graph.Tenant6Name,
                                Roles = new[] {Graph.OtherRole},
                                Permissions = new[] {""}
                            },
                            new RuntimeResponse
                            {
                                TenantId = Guid.Parse(Graph.Tenant8),
                                TenantName = Graph.Tenant8Name,
                                Roles = new[] {Graph.OtherRole},
                                Permissions = new[] {""}
                            }
                        }),
                        Features = new[] {features[0], features[1], features[4], features[5], features[6]},
                        TenantIds = new[] {Guid.Parse(Graph.Tenant1)},
                        Callback = r => r.Count == 5
                    }
                }
            };

            return data;
        }

        public static TheoryData<Func<AdminGraphRepository, Task<(Guid tenantId, Guid featureId)>>, bool>
            IsTenantAssignedToFeatureCoDependenciesAsyncInitScenarios() =>
            new TheoryData<Func<AdminGraphRepository, Task<(Guid tenantId, Guid featureId)>>, bool>
            {
                {
                    async r =>
                    {
                        var tenant = await r.CreateNodeAsync(new Tenant());
                        var licensedFeature1 = await r.CreateNodeAsync(new LicensedFeature());
                        var feature = await r.CreateNodeAsync(new Feature());
                        var licensedFeature2 = await r.CreateNodeAsync(new LicensedFeature());
                        var dep1 = await r.CreateNodeAsync(new Feature());
                        var dep2 = await r.CreateNodeAsync(new Feature());
                        await Task.WhenAll(
                            r.CreateRelationshipAsync<Tenant, LicensedFeature>(t => t.Id == tenant.Id,
                                lf => lf.Id == licensedFeature1.Id, Constants.AssignedLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature1.Id,
                                f => f.Id == feature.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature2.Id,
                                f => f.Id == dep1.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature2.Id,
                                f => f.Id == dep2.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<Feature, Feature>(f => f.Id == feature.Id, d => d.Id == dep1.Id,
                                Constants.DependsOnLink),
                            r.CreateRelationshipAsync<Feature, Feature>(f => f.Id == feature.Id, d => d.Id == dep2.Id,
                                Constants.DependsOnLink));
                        return (tenant.Id, feature.Id);
                    },
                    false
                },
                {
                    async r =>
                    {
                        var tenant = await r.CreateNodeAsync(new Tenant());
                        var licensedFeature1 = await r.CreateNodeAsync(new LicensedFeature());
                        var feature = await r.CreateNodeAsync(new Feature());
                        var licensedFeature2 = await r.CreateNodeAsync(new LicensedFeature());
                        var dep1 = await r.CreateNodeAsync(new Feature());
                        var licensedFeature3 = await r.CreateNodeAsync(new LicensedFeature());
                        var dep2 = await r.CreateNodeAsync(new Feature());
                        await Task.WhenAll(
                            r.CreateRelationshipAsync<Tenant, LicensedFeature>(t => t.Id == tenant.Id,
                                lf => lf.Id == licensedFeature1.Id, Constants.AssignedLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature1.Id,
                                f => f.Id == feature.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature2.Id,
                                f => f.Id == dep1.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature3.Id,
                                f => f.Id == dep2.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<Feature, Feature>(f => f.Id == feature.Id, d => d.Id == dep1.Id,
                                Constants.DependsOnLink),
                            r.CreateRelationshipAsync<Feature, Feature>(f => f.Id == feature.Id, d => d.Id == dep2.Id,
                                Constants.DependsOnLink),
                            r.CreateRelationshipAsync<Tenant, LicensedFeature>(t => t.Id == tenant.Id,
                                lf => lf.Id == licensedFeature2.Id, Constants.AssignedLink));
                        return (tenant.Id, feature.Id);
                    },
                    false
                },
                {
                    async r =>
                    {
                        var tenant = await r.CreateNodeAsync(new Tenant());
                        var licensedFeature1 = await r.CreateNodeAsync(new LicensedFeature());
                        var feature = await r.CreateNodeAsync(new Feature());
                        var licensedFeature2 = await r.CreateNodeAsync(new LicensedFeature());
                        var dep1 = await r.CreateNodeAsync(new Feature());
                        var dep2 = await r.CreateNodeAsync(new Feature());
                        await Task.WhenAll(
                            r.CreateRelationshipAsync<Tenant, LicensedFeature>(t => t.Id == tenant.Id,
                                lf => lf.Id == licensedFeature1.Id, Constants.AssignedLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature1.Id,
                                f => f.Id == feature.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature2.Id,
                                f => f.Id == dep1.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature2.Id,
                                f => f.Id == dep2.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<Feature, Feature>(f => f.Id == feature.Id, d => d.Id == dep1.Id,
                                Constants.DependsOnLink),
                            r.CreateRelationshipAsync<Feature, Feature>(f => f.Id == feature.Id, d => d.Id == dep2.Id,
                                Constants.DependsOnLink),
                            r.CreateRelationshipAsync<Tenant, LicensedFeature>(t => t.Id == tenant.Id,
                                lf => lf.Id == licensedFeature2.Id, Constants.AssignedLink));
                        return (tenant.Id, feature.Id);
                    },
                    true
                },
                {
                    async r =>
                    {
                        var tenant = await r.CreateNodeAsync(new Tenant());
                        var licensedFeature1 = await r.CreateNodeAsync(new LicensedFeature());
                        var feature = await r.CreateNodeAsync(new Feature());
                        var licensedFeature2 = await r.CreateNodeAsync(new LicensedFeature());
                        var dep1 = await r.CreateNodeAsync(new Feature());
                        var licensedFeature3 = await r.CreateNodeAsync(new LicensedFeature());
                        var dep2 = await r.CreateNodeAsync(new Feature());
                        await Task.WhenAll(
                            r.CreateRelationshipAsync<Tenant, LicensedFeature>(t => t.Id == tenant.Id,
                                lf => lf.Id == licensedFeature1.Id, Constants.AssignedLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature1.Id,
                                f => f.Id == feature.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature2.Id,
                                f => f.Id == dep1.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature3.Id,
                                f => f.Id == dep2.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<Feature, Feature>(f => f.Id == feature.Id, d => d.Id == dep1.Id,
                                Constants.DependsOnLink),
                            r.CreateRelationshipAsync<Feature, Feature>(f => f.Id == dep1.Id, d => d.Id == dep2.Id,
                                Constants.DependsOnLink),
                            r.CreateRelationshipAsync<Tenant, LicensedFeature>(t => t.Id == tenant.Id,
                                lf => lf.Id == licensedFeature2.Id, Constants.AssignedLink));
                        return (tenant.Id, feature.Id);
                    },
                    false
                }
            };

        public static TheoryData<Func<AdminGraphRepository, Task<(Guid tenantId, Guid featureId)>>, bool>
            IsTenantAssignedToFeatureWithoutDependantsInitScenarios() =>
            new TheoryData<Func<AdminGraphRepository, Task<(Guid tenantId, Guid featureId)>>, bool>
            {
                {
                    async r =>
                    {
                        var tenant = await r.CreateNodeAsync(new Tenant());
                        var licensedFeature1 = await r.CreateNodeAsync(new LicensedFeature());
                        var feature = await r.CreateNodeAsync(new Feature());
                        var licensedFeature2 = await r.CreateNodeAsync(new LicensedFeature());
                        var dep1 = await r.CreateNodeAsync(new Feature());
                        var dep2 = await r.CreateNodeAsync(new Feature());
                        await Task.WhenAll(
                            r.CreateRelationshipAsync<Tenant, LicensedFeature>(t => t.Id == tenant.Id,
                                lf => lf.Id == licensedFeature1.Id, Constants.AssignedLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature1.Id,
                                f => f.Id == feature.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature2.Id,
                                f => f.Id == dep1.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature2.Id,
                                f => f.Id == dep2.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<Feature, Feature>(f => f.Id == feature.Id, d => d.Id == dep1.Id,
                                Constants.DependsOnLink),
                            r.CreateRelationshipAsync<Feature, Feature>(f => f.Id == feature.Id, d => d.Id == dep2.Id,
                                Constants.DependsOnLink),
                            r.CreateRelationshipAsync<Tenant, LicensedFeature>(t => t.Id == tenant.Id,
                                lf => lf.Id == licensedFeature2.Id, Constants.AssignedLink));
                        return (tenant.Id, dep1.Id);
                    },
                    false
                },
                {
                    async r =>
                    {
                        var tenant = await r.CreateNodeAsync(new Tenant());
                        var licensedFeature1 = await r.CreateNodeAsync(new LicensedFeature());
                        var feature = await r.CreateNodeAsync(new Feature());
                        var licensedFeature2 = await r.CreateNodeAsync(new LicensedFeature());
                        var dep1 = await r.CreateNodeAsync(new Feature());
                        var dep2 = await r.CreateNodeAsync(new Feature());
                        await Task.WhenAll(
                            r.CreateRelationshipAsync<Tenant, LicensedFeature>(t => t.Id == tenant.Id,
                                lf => lf.Id == licensedFeature1.Id, Constants.AssignedLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature1.Id,
                                f => f.Id == feature.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature2.Id,
                                f => f.Id == dep1.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature2.Id,
                                f => f.Id == dep2.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<Feature, Feature>(f => f.Id == feature.Id, d => d.Id == dep1.Id,
                                Constants.DependsOnLink),
                            r.CreateRelationshipAsync<Feature, Feature>(f => f.Id == feature.Id, d => d.Id == dep2.Id,
                                Constants.DependsOnLink),
                            r.CreateRelationshipAsync<Tenant, LicensedFeature>(t => t.Id == tenant.Id,
                                lf => lf.Id == licensedFeature2.Id, Constants.AssignedLink));
                        return (tenant.Id, feature.Id);
                    },
                    true
                },
                {
                    async r =>
                    {
                        var tenant = await r.CreateNodeAsync(new Tenant());
                        var licensedFeature1 = await r.CreateNodeAsync(new LicensedFeature());
                        var feature = await r.CreateNodeAsync(new Feature());
                        await Task.WhenAll(
                            r.CreateRelationshipAsync<Tenant, LicensedFeature>(t => t.Id == tenant.Id,
                                lf => lf.Id == licensedFeature1.Id, Constants.AssignedLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature1.Id,
                                f => f.Id == feature.Id, Constants.ContainsLink));
                        return (tenant.Id, feature.Id);
                    },
                    true
                },
                {
                    async r =>
                    {
                        var tenant = await r.CreateNodeAsync(new Tenant());
                        var licensedFeature1 = await r.CreateNodeAsync(new LicensedFeature());
                        var feature = await r.CreateNodeAsync(new Feature());
                        var licensedFeature2 = await r.CreateNodeAsync(new LicensedFeature());
                        var dep1 = await r.CreateNodeAsync(new Feature());
                        await Task.WhenAll(
                            r.CreateRelationshipAsync<Tenant, LicensedFeature>(t => t.Id == tenant.Id,
                                lf => lf.Id == licensedFeature1.Id, Constants.AssignedLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature1.Id,
                                f => f.Id == feature.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature2.Id,
                                f => f.Id == dep1.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<Feature, Feature>(f => f.Id == feature.Id, d => d.Id == dep1.Id,
                                Constants.DependsOnLink));
                        return (tenant.Id, dep1.Id);
                    },
                    false
                },
                {
                    async r =>
                    {
                        var tenant = await r.CreateNodeAsync(new Tenant());
                        var licensedFeature1 = await r.CreateNodeAsync(new LicensedFeature());
                        var feature = await r.CreateNodeAsync(new Feature());
                        var licensedFeature2 = await r.CreateNodeAsync(new LicensedFeature());
                        var dep1 = await r.CreateNodeAsync(new Feature());
                        var licensedFeature3 = await r.CreateNodeAsync(new LicensedFeature());
                        var dep2 = await r.CreateNodeAsync(new Feature());
                        await Task.WhenAll(
                            r.CreateRelationshipAsync<Tenant, LicensedFeature>(t => t.Id == tenant.Id,
                                lf => lf.Id == licensedFeature1.Id, Constants.AssignedLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature1.Id,
                                f => f.Id == feature.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature2.Id,
                                f => f.Id == dep1.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<LicensedFeature, Feature>(lf => lf.Id == licensedFeature3.Id,
                                f => f.Id == dep2.Id, Constants.ContainsLink),
                            r.CreateRelationshipAsync<Feature, Feature>(f => f.Id == feature.Id, d => d.Id == dep1.Id,
                                Constants.DependsOnLink),
                            r.CreateRelationshipAsync<Feature, Feature>(f => f.Id == dep1.Id, d => d.Id == dep2.Id,
                                Constants.DependsOnLink),
                            r.CreateRelationshipAsync<Tenant, LicensedFeature>(t => t.Id == tenant.Id,
                                lf => lf.Id == licensedFeature2.Id, Constants.AssignedLink),
                            r.CreateRelationshipAsync<Tenant, LicensedFeature>(t => t.Id == tenant.Id,
                                lf => lf.Id == licensedFeature3.Id, Constants.AssignedLink));
                        return (tenant.Id, dep2.Id);
                    },
                    false
                }
            };

        public class FilterFeatureIdsWithAccessDeniedScenario
        {
            public ClaimsPrincipal Identity { get; set; }
            public Guid[] TenantIds { get; set; }
            public Feature[] Features { get; set; }
            public Func<IReadOnlyCollection<Guid>, bool> Callback { get; set; }
        }
    }
}