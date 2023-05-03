using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.Role
{
    public class RoleType : ObjectType<Contracts.Output.Role>
    {
        protected override void Configure(IObjectTypeDescriptor<Contracts.Output.Role> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name(nameof(Contracts.Output.Role));

            descriptor.Field(t => t.Id)
                .Type<NonNullType<IdType>>().Description("Id of the Role.");

            descriptor.Field(t => t.Name)
                .Type<NonNullType<StringType>>().Description("Role Name.");

            descriptor.Field(t => t.Description)
                .Type<NonNullType<StringType>>().Description("Role Description.");
            
            descriptor.Field(t => t.Type)
                .Type<NonNullType<RoleTypeEnum>>().Description("Role Type.");

            descriptor.Field(t => t.Enabled)
                .Type<NonNullType<BooleanType>>().Description("Role Enabled.");
            
            descriptor.Field(t => t.CreatedAt)
                .Type<NonNullType<LongType>>().Description("Role Created Timestamp.");

            descriptor.Field(t => t.UpdatedAt)
                .Type<NonNullType<LongType>>().Description("Role Updated Timestamp.");
        }
    }
}