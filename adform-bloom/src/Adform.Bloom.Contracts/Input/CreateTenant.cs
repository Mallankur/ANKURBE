using System;

namespace Adform.Bloom.Contracts.Input
{
    public class CreateTenant : NamedNodeWriteDto
    {
        public Guid? ParentId { get; set; }
    }
}