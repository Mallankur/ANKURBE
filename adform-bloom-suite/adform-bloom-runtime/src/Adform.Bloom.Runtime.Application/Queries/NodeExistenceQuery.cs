using FluentResults;
using MediatR;

namespace Adform.Bloom.Application.Queries
{
    public class NodeExistenceQuery : IRequest<Result<bool>>
    {
        public List<NodeDescriptor> NodeDescriptors { get; set; } = new();
    }

    public class NodeDescriptor
    {
        public string Label { get; set; } = "";
        public Guid? Id { get; set; }
        public string? UniqueName { get; set; }
    }
}
