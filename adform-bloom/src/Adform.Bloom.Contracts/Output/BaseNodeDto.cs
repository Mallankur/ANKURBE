using System;

namespace Adform.Bloom.Contracts.Output
{
    public class BaseNodeDto
    {
        public Guid Id { get; set; }

        public bool Enabled { get; set; } = true;

        public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public long UpdatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}