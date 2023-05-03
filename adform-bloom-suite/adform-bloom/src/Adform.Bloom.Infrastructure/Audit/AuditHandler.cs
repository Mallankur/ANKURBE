using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Adform.Bloom.Infrastructure.Audit
{
    public class AuditHandler :
        INotificationHandler<AuditEvent>,
        INotificationHandler<AuditChange>
    {
        private readonly IAuditService _auditService;

        public AuditHandler(IAuditService auditService)
        {
            _auditService = auditService;
        }

        public Task Handle(AuditChange notification, CancellationToken cancellationToken)
        {
            return _auditService.SendChange(notification, cancellationToken);
        }

        public Task Handle(AuditEvent notification, CancellationToken cancellationToken)
        {
            return _auditService.SendEvent(notification, cancellationToken);
        }
    }
}