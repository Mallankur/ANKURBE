using System.Threading;
using System.Threading.Tasks;

namespace Adform.Bloom.Infrastructure.Audit
{
    public interface IAuditService
    {
        Task SendEvent(AuditEvent auditEvent, CancellationToken cancellationToken = default);
        Task SendChange(AuditChange auditChange, CancellationToken cancellationToken = default);
    }
}