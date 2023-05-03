using Adform.Bloom.Application.Queries;
using HotChocolate.Types;

namespace Adform.Bloom.Runtime.Host.Graph.ExistenceCheck
{
    public class NodeDescriptorInputType : InputObjectType<NodeDescriptor>
    {
        protected override void Configure(IInputObjectTypeDescriptor<NodeDescriptor> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name(nameof(NodeDescriptor));

            descriptor.Field(t => t.Label)
                .Type<NonNullType<StringType>>().Description("Label.");

            descriptor.Field(t => t.Id)
                .Type<IdType>().Description("Id.");

            descriptor.Field(t => t.UniqueName)
                .Type<StringType>().Description("UniqueName.");
        }
    }
}