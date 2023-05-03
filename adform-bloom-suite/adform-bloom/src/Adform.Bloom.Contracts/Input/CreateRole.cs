using System;

namespace Adform.Bloom.Contracts.Input
{
    public class CreateRole : NamedNodeWriteDto
    {
        public Guid? PolicyId { get; set; }
        public Guid TenantId { get; set; }
    }
}