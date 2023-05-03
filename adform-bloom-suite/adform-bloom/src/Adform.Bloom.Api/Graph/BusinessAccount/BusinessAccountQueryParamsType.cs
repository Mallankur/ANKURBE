using System;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Contracts.Input;
using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.BusinessAccount
{
    [Obsolete]
    public class BusinessAccountQueryParamsType : InputObjectType<QueryParamsBusinessAccountInput>
    {
        protected override void Configure(IInputObjectTypeDescriptor<QueryParamsBusinessAccountInput> descriptor)
        {
            descriptor.Name("BusinessAccountQueryParamsInput");

            descriptor.Field(t => t.FieldName)
                .Type<StringType>()
                .Description("OrderBy.");

            descriptor.Field(t => t.Order)
                .Type<SortOrderTypeEnum>()
                .Description("Sorting Order Ascending or descending.");

            descriptor.Field(t => t.Search)
                .Type<StringType>()
                .Description("Search.");

            descriptor.Field(t => t.BusinessAccountType)
                .Type<BusinessAccountTypeEnum>()
                .Description("BusinessAccount Type.");
        }
    }
}