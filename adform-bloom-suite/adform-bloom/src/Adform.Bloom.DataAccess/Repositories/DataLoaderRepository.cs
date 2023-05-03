using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Ciam.OngDb.Core.Extensions;
using Adform.Ciam.OngDb.Core.Interfaces;
using Adform.Ciam.OngDb.Repository;
using Neo4jClient.Transactions;

namespace Adform.Bloom.DataAccess.Repositories
{
    public class DataLoaderRepository: GraphRepository, IDataLoaderRepository
    {
        public DataLoaderRepository(ITransactionalGraphClient graphClient)
            : base(graphClient)
        {
        }
        
        public async Task<IEnumerable<ConnectedEntity<TOut>>> GetNodesWithConnectedAsync<TIn, TOut>(
            IEnumerable<Guid> startNodeIds,
            ILink relationship)
            where TOut : BaseNode
            where TIn : BaseNode
        {
            const string startParameter = "t";
            var query = (await GraphClient).Cypher
                .Match($"({startParameter}:{typeof(TIn).Name})")
                .Where($"{startParameter}.Id in {{startNodeIds}}")
                .WithParam("startNodeIds", startNodeIds)
                .OptionalMatch($"({startParameter}){relationship.ToCypher()}(y:{typeof(TOut).Name})")
                .With($"{startParameter}.Id as id, y as connectedNode")
                .Return((id, connectedNode) => new ConnectedEntity<TOut>
                {
                    StartNodeId = id.As<Guid>(),
                    ConnectedNode = connectedNode.As<TOut>()
                });
            return await query.ResultsAsync ?? Enumerable.Empty<ConnectedEntity<TOut>>();
        }

        public async Task<IEnumerable<ConnectedEntity<TOut>>> GetNodesWithIntermediateWithConnectedAsync<TIn,
            TIntermediate, TOut>(IEnumerable<Guid> startNodeIds,
            ILink linkParentToIntermediate,
            ILink linkIntermediateToChild)
            where TOut : BaseNode
            where TIntermediate : BaseNode
            where TIn : BaseNode
        {
            const string startParameter = "n";
            const string intermediateParameter = "o";
            const string outputParameter = "p";

            var query = (await GraphClient).Cypher
                .Match($"({startParameter}:{typeof(TIn).Name})")
                .Where($"{startParameter}.Id in {{startNodeIds}}")
                .WithParam("startNodeIds", startNodeIds)
                .OptionalMatch(
                    $"({startParameter}){linkParentToIntermediate.ToCypher()}({intermediateParameter}:{typeof(TIntermediate).Name})")
                .OptionalMatch(
                    $"({intermediateParameter}){linkIntermediateToChild.ToCypher()}({outputParameter}:{typeof(TOut).Name})")
                .With($"{startParameter}.Id as id, {outputParameter} as connectedNode")
                .Return((id, connectedNode) => new ConnectedEntity<TOut>
                {
                    StartNodeId = id.As<Guid>(),
                    ConnectedNode = connectedNode.As<TOut>()
                });
            return await query.ResultsAsync ?? Enumerable.Empty<ConnectedEntity<TOut>>();
        }
    }
}