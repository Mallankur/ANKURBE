using System;
using System.Collections.Generic;
using System.Security.Claims;
using Adform.Bloom.Domain.Entities;

namespace Adform.Bloom.Write.Commands
{
    public class CreateRoleCommand : NamedCreateCommand<Role>
    {
        public CreateRoleCommand(ClaimsPrincipal principal, 
            Guid? policyId, 
            Guid tenantId, 
            string name,
            string description = "",
            bool isEnabled = true,
            IReadOnlyCollection<Guid>? featureIds = null,
            bool isTemplateRole = false)
            : base(principal, name, description, isEnabled)
        {
            PolicyId = policyId;
            TenantId = tenantId;
            FeatureIds = featureIds;
            IsTemplateRole = isTemplateRole;
        }

        public Guid? PolicyId { get; }
        public Guid TenantId { get; }
        public IReadOnlyCollection<Guid>? FeatureIds { get; }
        public bool IsTemplateRole { get; }
    }
}