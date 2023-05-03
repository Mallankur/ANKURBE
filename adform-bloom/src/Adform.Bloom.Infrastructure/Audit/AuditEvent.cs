using Adform.Ciam.SharedKernel.Extensions;
using MediatR;
using System;
using System.Security.Claims;
using System.Text.Json;

namespace Adform.Bloom.Infrastructure.Audit
{
    public class AuditEvent : INotification
    {
        public AuditEvent(ClaimsPrincipal subject, Guid entityId, string entityType, AuditOperation operation)
        {
            Subject = subject.GetSubId();
            EntityId = entityId.ToString();
            EntityType = entityType;
            Operation = operation.ToString();
        }

        public string? Subject { get; }
        public string EntityId { get; }
        public string EntityType { get; }
        public string Operation { get; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}