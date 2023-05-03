using System;
using System.Collections.Generic;
using System.Linq;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Domain.Entities;

namespace Adform.Bloom.Api
{
    public static class MappingExtensions
    {
        public static Write.LinkOperation MapToDomain(this LinkOperation link)
        {
            return link switch
            {
                LinkOperation.Assign => Write.LinkOperation.Assign,
                LinkOperation.Unassign => Write.LinkOperation.Unassign,
                _ => throw new InvalidOperationException($"Unknown link operation: '{link}'!"),
            };
        }

        public static TDto MapFromDomain<TNode, TDto>(this TNode node) 
            where TNode : NamedNode, new()
            where TDto : NamedNodeDto, new()
        {
            return new TDto
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description,
                Enabled = node.IsEnabled,
                CreatedAt = node.CreatedAt,
                UpdatedAt = node.UpdatedAt
            };
        }

        public static IEnumerable<TDto> MapFromDomain<TNode, TDto>(this IEnumerable<TNode> nodes)
            where TNode : NamedNode, new()
            where TDto : NamedNodeDto, new()
        {
            return nodes.Select(n => n.MapFromDomain<TNode, TDto>()).ToArray();
        }

        public static Contracts.Output.Subject MapFromDomain(this Domain.Entities.Subject node)
        {
            return new Contracts.Output.Subject
            {
                Id = node.Id,
                Enabled = node.IsEnabled,
                CreatedAt = node.CreatedAt,
                UpdatedAt = node.UpdatedAt
            };
        }

        public static IEnumerable<Contracts.Output.Subject> MapFromDomain(this IEnumerable<Domain.Entities.Subject> nodes)
        {
            return nodes.Select(n => n.MapFromDomain()).ToArray();
        }

        public static Contracts.Output.Tenant MapFromDomain(this Domain.Entities.Tenant node)
        {
            return new Contracts.Output.Tenant
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description,
                LegacyId = node.LegacyId,
                Enabled = node.IsEnabled,
                CreatedAt = node.CreatedAt,
                UpdatedAt = node.UpdatedAt
            };
        }

        public static IEnumerable<Contracts.Output.Tenant> MapFromDomain(this IEnumerable<Domain.Entities.Tenant> nodes)
        {
            return nodes.Select(n => n.MapFromDomain()).ToArray();
        }
    }
}
