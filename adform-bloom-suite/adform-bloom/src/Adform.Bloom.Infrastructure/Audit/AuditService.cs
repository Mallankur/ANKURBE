using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Adform.Bloom.Infrastructure.Audit
{
    public class AuditService : IAuditService
    {
        private readonly ILogger<AuditService> _logger;

        public AuditService(ILogger<AuditService> logger)
        {
            _logger = logger;
        }

        public Task SendEvent(AuditEvent auditEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(auditEvent.ToString());
            return Task.CompletedTask;
        }

        public Task SendChange(AuditChange auditChange, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(auditChange.ToString());
            return Task.CompletedTask;
        }
    }
}