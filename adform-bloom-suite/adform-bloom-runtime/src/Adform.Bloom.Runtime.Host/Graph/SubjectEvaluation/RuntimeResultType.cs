using Adform.Bloom.Runtime.Read.Entities;
using HotChocolate.Types;

namespace Adform.Bloom.Runtime.Host.Graph.SubjectEvaluation
{
    public class RuntimeResultType : ObjectType<RuntimeResult>
    {
        protected override void Configure(IObjectTypeDescriptor<RuntimeResult> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name(nameof(RuntimeResult));

            descriptor.Field(t => t.TenantId)
                .Type<NonNullType<IdType>>().Description("Legacy Tenant Id.");

            descriptor.Field(t => t.TenantLegacyId)
                .Type<NonNullType<IntType>>().Description("Tenant Id.");

            descriptor.Field(t => t.TenantName)
                .Type<NonNullType<StringType>>().Description("Tenant Name.");

            descriptor.Field(t => t.Roles)
                .Type<NonNullType<ListType<StringType>>>().Description("Roles.");

            descriptor.Field(t => t.Permissions)
                .Type<NonNullType<ListType<StringType>>>().Description("Permissions.");
            
        }
    }
}