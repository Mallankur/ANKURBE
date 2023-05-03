using System;
using System.Collections.Generic;

namespace Adform.Bloom.Contracts.Input
{
    public class CreateSubject : BaseNodeWriteDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public Guid ActorId { get; set; }
        public IReadOnlyCollection<RoleBusinessAccount> RoleBusinessAccounts { get; set; } =
            new List<RoleBusinessAccount>();
    }
}