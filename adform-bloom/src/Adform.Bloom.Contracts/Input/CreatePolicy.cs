using System;

namespace Adform.Bloom.Contracts.Input
{
    public class CreatePolicy : NamedNodeWriteDto
    {
        public Guid? ParentId { get; set; }
    }
}