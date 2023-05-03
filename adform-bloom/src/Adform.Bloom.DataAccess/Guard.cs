using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure.Extensions;
using Adform.Bloom.Domain.Entities;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Neo4jClient.Extensions;

namespace Adform.Bloom.DataAccess
{
    public static class Guard
    {
        public static void ThrowNotFound<T>(Guid? id = null)
            where T : BaseNode
        {
            throw new NotFoundException(message: $"{typeof(T).Name} not found.",
                parameters: new Dictionary<string, object>
                {
                    {typeof(T).Name.ToLowerFirstCharacter(), id != null ? id.Value.ToString() : $"{null}"}
                });
        }

        public static void ThrowNotFound<T>(IReadOnlyCollection<Guid> ids)
            where T : BaseNode
        {
            throw new NotFoundException(message: $"{typeof(T).Name} not found.",
                parameters: new Dictionary<string, object>
                {
                    {typeof(T).Name.ToLowerFirstCharacter(), $"{typeof(T).Name} not found."}
                });
        }

        public static async Task ThrowIfNotFound<T>(this IAdminGraphRepository repository, Guid id)
            where T : BaseNode
        {
            var count = await repository.GetCountAsync<T>(n => n.Id == id);
            if (count < 1)
                ThrowNotFound<T>(id);
        }

        public static async Task ThrowIfNotFound<T>(this IAdminGraphRepository repository,
            IReadOnlyCollection<Guid> ids)
            where T : BaseNode
        {
            var count = await repository.GetCountAsync<T>(x => x.Id.In(ids));
            if (count < ids.Count)
                ThrowNotFound<T>(ids);
        }
    }
}