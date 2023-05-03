using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.Role
{
    public class RoleInputType : InputObjectType<Contracts.Output.Role>
    {
        protected override void Configure(IInputObjectTypeDescriptor<Contracts.Output.Role> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name("RoleInput");

            descriptor.Field(t => t.Id).Ignore();

            descriptor.Field(t => t.Name)
                .Type<StringType>().Description("Role Name.");

            descriptor.Field(t => t.Description)
                .Type<StringType>()
                .Description("Role Description.")
                .DefaultValue(string.Empty);

            descriptor.Field(t => t.Enabled)
                .Type<BooleanType>()
                .Description("Role Enabled.")
                .DefaultValue(true);

            descriptor.Field(t => t.Type).Ignore();

            descriptor.Field(t => t.CreatedAt).Ignore();

            descriptor.Field(t => t.UpdatedAt).Ignore();
        }
    }
}