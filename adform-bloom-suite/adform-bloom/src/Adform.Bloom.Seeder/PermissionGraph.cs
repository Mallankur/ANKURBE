using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure;
using Adform.Ciam.OngDb.Extensions;
using Adform.Ciam.OngDb.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adform.Bloom.Seeder
{
    public class PermissionGraph
    {
        private IGraphRepository _repository;

        public enum Operation
        {
            CanAssign,
            CanRead,
            CanManage
        }

        public PermissionGraph(IConfigurationRoot configuration)
        {
            var services = new ServiceCollection();
            services.ConfigureNeo(configuration);
            var serviceProvider = services.BuildServiceProvider();
            _repository = serviceProvider.GetService<IGraphRepository>();

        }

        public async Task GeneratePermission(string licenseFeatureName, string domain, string entity)
        {
            var permissions = await CreatePermissions(domain, entity);
            var features = await CreateFeature(entity, permissions);
            var licensedFeature = await CreateLicensedFeature(licenseFeatureName, features);
        }

        private async Task<ICollection<Permission>> CreatePermissions(string domain, string entity)
        {
            var permissions = new List<Permission>();
            foreach (var op in Enum.GetValues(typeof(Operation)))
            {
                permissions.Add(await _repository.CreateNodeAsync(new Permission($"{domain}.{entity}.{op}")));
            }

            return permissions;
        }


        private async Task<LicensedFeature> CreateLicensedFeature(string licenseFeatureName, ICollection<Feature> features)
        {
            var licenseFeature = await _repository.CreateNodeAsync(new LicensedFeature(licenseFeatureName));
            foreach (var item in features)
            {
                await _repository.CreateRelationshipAsync<LicensedFeature, Feature>(o => o.Id == licenseFeature.Id,
                    p => p.Id == item.Id, Constants.ContainsLink);
            }

            return licenseFeature;
        }


        private async Task<ICollection<Feature>> CreateFeature(string entity, ICollection<Permission> permissions)
        {
            var features = new List<Feature>();
            foreach (var op in Enum.GetValues(typeof(Operation)))
            {
                if (op.ToString() != Operation.CanAssign.ToString())
                {
                    var substring = op.ToString().Split("Can")[1];
                    features.Add(await _repository.CreateNodeAsync(new Feature($"{substring} {entity}")));
                }
            }

            var permissionManage = permissions.FirstOrDefault(o => o.Name.Contains(Operation.CanManage.ToString()));
            var permissionAssign = permissions.FirstOrDefault(o => o.Name.Contains(Operation.CanAssign.ToString()));
            var featureManage = features.FirstOrDefault(o => o.Name.Contains(Operation.CanManage.ToString().Split("Can")[1]));
            await _repository.CreateRelationshipAsync<Feature, Permission>(o => o.Id == featureManage.Id,
                p => p.Id == permissionManage.Id, Constants.ContainsLink); 
            await _repository.CreateRelationshipAsync<Feature, Permission>(o => o.Id == featureManage.Id,
                p => p.Id == permissionAssign.Id, Constants.ContainsLink);


            var permissioRead = permissions.FirstOrDefault(o => o.Name.Contains(Operation.CanRead.ToString()));
            var featureRead = features.FirstOrDefault(o => o.Name.Contains(Operation.CanRead.ToString().Split("Can")[1]));
            await _repository.CreateRelationshipAsync<Feature, Permission>(o => o.Id == featureRead.Id,
                p => p.Id == permissioRead.Id, Constants.ContainsLink);


            await _repository.CreateRelationshipAsync<Feature, Feature>(o => o.Id == featureManage.Id,
                p => p.Id == featureRead.Id, Constants.DependsOnLink);

            return features;
        }


    }
}