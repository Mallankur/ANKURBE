using System;
using System.Collections.Generic;
using Adform.Bloom.Domain.Entities;

namespace Adform.Bloom.Infrastructure.Extensions
{
    public static class ConnectedEntityExtensions
    {
        public static IDictionary<Guid, List<TOut>> ToGraphQlFriendlyDictionary<TValue, TOut>(
            this IEnumerable<ConnectedEntity<TValue>> ce, Func<TValue?, TOut> nonNullableSelector)
            where TValue : BaseNode
            where TOut : notnull
        {
            var dic = new Dictionary<Guid, List<TOut>>();
            foreach (var r in ce)
            {
                if (r.ConnectedNode is null)
                {
                    dic[r.StartNodeId] = new List<TOut>(0);
                    continue;
                }

                if (dic.ContainsKey(r.StartNodeId))
                {
                    dic[r.StartNodeId].Add(nonNullableSelector(r.ConnectedNode));
                    continue;
                }

                dic[r.StartNodeId] = new List<TOut>
                {
                    nonNullableSelector(r.ConnectedNode)
                };
            }

            return dic;
        }
    }
}