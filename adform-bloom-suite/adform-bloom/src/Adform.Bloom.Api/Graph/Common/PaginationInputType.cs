using Adform.Bloom.Contracts.Input;
using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.Common
{
    public class PaginationInputType : InputObjectType<PaginationInput>
    {
        protected override void Configure(IInputObjectTypeDescriptor<PaginationInput> descriptor)
        {
            descriptor.Name("PaginationInput");

            descriptor.Field(t => t.Offset)
                .Type<NonNullType<IntType>>().Description("Offset.");

            descriptor.Field(t => t.Limit)
                .Type<NonNullType<LimitType>>().Description("Limit.");
        }
    }
}