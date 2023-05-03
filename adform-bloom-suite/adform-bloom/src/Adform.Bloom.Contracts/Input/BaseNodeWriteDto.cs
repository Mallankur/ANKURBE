using System;

namespace Adform.Bloom.Contracts.Input
{
    public class BaseNodeWriteDto
    {
        public bool IsEnabled { get; set; } = true;
        public virtual long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public virtual long UpdatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}