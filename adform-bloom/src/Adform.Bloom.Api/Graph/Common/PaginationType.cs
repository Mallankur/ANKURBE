using Adform.Ciam.SharedKernel.Entities;
using HotChocolate;
using HotChocolate.Types;
using Humanizer;

namespace Adform.Bloom.Api.Graph.Common
{
    public class PaginationType<T, U> : ObjectType<EntityPagination<U>> where T : ObjectType<U>
    {
        protected override void Configure(IObjectTypeDescriptor<EntityPagination<U>> descriptor)
        {
            descriptor.Name($"{typeof(U).Name}List");

            descriptor.Field(t => t.Offset)
                .Type<NonNullType<IntType>>()
                .Description("Offset.");

            descriptor.Field(t => t.Limit)
                .Type<NonNullType<IntType>>().Description("Limit.");

            descriptor.Field(t => t.TotalItems).Name(new NameString("totalCount"))
                .Type<NonNullType<IntType>>().Description("Total Count.");

            descriptor.Field(t => t.Data)
                .Type<NonNullType<ListType<T>>>()
                .Name(typeof(U).Name.Pluralize().ToCamelCase())
                .Description("Data.");
        }
    }
}