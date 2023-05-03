using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.Permission
{
    public class PermissionInputType : InputObjectType<Contracts.Output.Permission>
    {

        protected override void Configure(IInputObjectTypeDescriptor<Contracts.Output.Permission> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name("PermissionInput");

            descriptor.Field(t => t.Id).Ignore();

            descriptor.Field(t => t.Name)
                .Type<NonNullType<StringType>>().Description("Permission Name.");

            descriptor.Field(t => t.Description)
                .Type<NonNullType<StringType>>().Description("Permission Description.");

            descriptor.Field(t => t.Enabled)
                .Type<NonNullType<BooleanType>>().Description("Permission Enabled.");

            descriptor.Field(t => t.CreatedAt).Ignore();

            descriptor.Field(t => t.UpdatedAt).Ignore();
        }
    }
}