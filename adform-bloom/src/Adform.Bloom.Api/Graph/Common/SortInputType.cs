using Adform.Bloom.Contracts.Input;
using Adform.Ciam.OngDb.Repository;
using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.Common
{
    public class SortInputType: InputObjectType<SortingParamsInput>
    {
        protected override void Configure(IInputObjectTypeDescriptor<SortingParamsInput> descriptor)
        {
            descriptor.Name("SortInput");

            descriptor.Field(t => t.FieldName)
                .Type<NonNullType<StringType>>()
                .Description("OrderBy.")
                .DefaultValue("Id");

            descriptor.Field(t => t.Order)
                .Type<SortOrderTypeEnum>()
                .DefaultValue(SortingOrder.Ascending)
                .Description("SortingOrder.");
        }
    }
}