using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Adform.Ciam.OngDb.Core.Interfaces;

namespace Adform.Bloom.DataAccess.Interfaces
{
    public interface IDataLoaderRepository
    {
        Task<IEnumerable<ConnectedEntity<TOut>>> GetNodesWithConnectedAsync<TIn, TOut>(
            IEnumerable<Guid> startNodeIds,
            ILink relationship)
            where TOut : BaseNode
            where TIn : BaseNode;

        Task<IEnumerable<ConnectedEntity<TOut>>> GetNodesWithIntermediateWithConnectedAsync<TIn,
            TIntermediate, TOut>(IEnumerable<Guid> startNodeIds,
            ILink linkParentToIntermediate,
            ILink linkIntermediateToChild)
            where TOut : BaseNode
            where TIntermediate : BaseNode
            where TIn : BaseNode;
    }
}