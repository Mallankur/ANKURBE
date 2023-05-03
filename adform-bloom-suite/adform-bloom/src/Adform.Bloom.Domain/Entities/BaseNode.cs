using Adform.Ciam.SharedKernel.Entities;
using System;
using System.Text.Json;

namespace Adform.Bloom.Domain.Entities
{
    public abstract class NamedNode : BaseNode
    {
        protected NamedNode(string nodeName)
        {
            Name = nodeName;
        }

        public string Name { get; set; }

        public string Description { get; set; } = string.Empty;

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }


    public abstract class BaseNode : IEntity<Guid>
    {
        public bool IsEnabled { get; set; } = true;

        public Guid Id { get; set; } = Guid.NewGuid();

        public virtual long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        public virtual long UpdatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}