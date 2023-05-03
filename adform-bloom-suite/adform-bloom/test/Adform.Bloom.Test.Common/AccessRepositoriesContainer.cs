using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.Access;
using Neo4jClient.Transactions;

namespace Adform.Bloom.Common.Test
{
    public class AccessRepositoriesContainer
    {
        private readonly IDictionary<(Type, Type), object> _dict;

        public AccessRepositoriesContainer(ITransactionalGraphClient client)
        {
            _dict = new ConcurrentDictionary<(Type,Type), object>();
            _dict.Add((typeof(Subject),typeof(Contracts.Output.Role)), new RoleByUserIdAccessProvider(client));
        }
        
        public IAccessProvider<TContextDto,TFilterInput, TOutputDto> Get<TContextDto,TFilterInput, TOutputDto>()
            where TContextDto : class 
            where TOutputDto : class
            where TFilterInput : class
        {
            return (IAccessProvider<TContextDto,TFilterInput, TOutputDto>)_dict[(typeof(TContextDto),typeof(TOutputDto))];
        }
    }
}
