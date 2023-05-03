using Adform.Ciam.SharedKernel.Extensions;
using MediatR;
using System;
using System.Security.Claims;
using System.Text.Json;

namespace Adform.Bloom.Infrastructure.Audit
{
    public class AuditChange : INotification
    {
        public AuditChange(ClaimsPrincipal subject, ConnectedNode oldEntity, ConnectedNode newEntity,
            AuditOperation operation)
        {
            Subject = subject.GetSubId() ?? Guid.Empty.ToString();
            OldEntity = oldEntity.ToString();
            NewEntity = newEntity.ToString();
            Operation = operation.ToString();
        }

        public string Subject { get; }
        public string Operation { get; }
        public string OldEntity { get; }
        public string NewEntity { get; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}