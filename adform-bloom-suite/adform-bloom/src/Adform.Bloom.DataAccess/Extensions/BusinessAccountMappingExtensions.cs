using System.Collections.Generic;
using System.Linq;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Read.Contracts.BusinessAccount;

namespace Adform.Bloom.DataAccess.Extensions
{
    public static class BusinessAccountMappingExtensions
    {
        public static BusinessAccount MapFromReadModel(this BusinessAccountResult node)
        {
            return new BusinessAccount
            {
                Id = node.Id,
                Name = node.Name,
                LegacyId = node.LegacyId,
                Status = node.Status,
                Type = node.Type
            };
        }

        public static IReadOnlyCollection<BusinessAccount> MapFromReadModel(this IEnumerable<BusinessAccountResult> nodes)
        {
            return nodes.Select(n => n.MapFromReadModel()).ToArray();
        }
    }
}