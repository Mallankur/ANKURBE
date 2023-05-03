using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.Visibility;
using Adform.Bloom.Infrastructure.Models;
using Neo4jClient.Transactions;

namespace Adform.Bloom.Common.Test
{
    public class VisibilityRepositoriesContainer
    {
        private readonly IDictionary<Type, object> _dict;

        public VisibilityRepositoriesContainer(ITransactionalGraphClient client)
        {
            _dict = new ConcurrentDictionary<Type, object>();
            _dict.Add(typeof(Contracts.Output.Feature), new FeatureVisibilityProvider(client));
            _dict.Add(typeof(Contracts.Output.Permission), new PermissionVisibilityProvider(client));
            _dict.Add(typeof(Contracts.Output.Role), new RoleVisibilityProvider(client));
            _dict.Add(typeof(Contracts.Output.Subject), new SubjectVisibilityProvider(client));
            _dict.Add(typeof(Contracts.Output.Tenant), new TenantVisibilityProvider(client));
            _dict.Add(typeof(Contracts.Output.LicensedFeature), new LicensedFeatureVisibilityProvider(client));
        }

        public IVisibilityProvider<TFilter, T> Get<TFilter, T>() 
            where T : class
            where TFilter : QueryParams
        {
            return (IVisibilityProvider<TFilter, T>) _dict[typeof(T)];
        }
    }
}