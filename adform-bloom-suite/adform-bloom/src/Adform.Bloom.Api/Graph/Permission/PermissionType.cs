using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.Permission
{
    public class PermissionType : ObjectType<Contracts.Output.Permission>
    {
        protected override void Configure(IObjectTypeDescriptor<Contracts.Output.Permission> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name(nameof(Contracts.Output.Permission));

            descriptor.Field(t => t.Id)
                .Type<NonNullType<IdType>>().Description("Id of the Permission.");

            descriptor.Field(t => t.Name)
                .Type<NonNullType<StringType>>().Description("Permission Name.");

            descriptor.Field(t => t.Description)
                .Type<NonNullType<StringType>>().Description("Permission Description.");

            descriptor.Field(t => t.Enabled)
                .Type<NonNullType<BooleanType>>().Description("Permission Enabled.");
            
            descriptor.Field(t => t.CreatedAt)
                .Type<NonNullType<LongType>>().Description("Permission Created Timestamp.");

            descriptor.Field(t => t.UpdatedAt)
                .Type<NonNullType<LongType>>().Description("Permission Updated Timestamp.");
        }
    }
}