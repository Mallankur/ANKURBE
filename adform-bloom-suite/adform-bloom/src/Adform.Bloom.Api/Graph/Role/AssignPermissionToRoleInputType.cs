using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Contracts.Input;
using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.Role
{
    public class AssignPermissionToRoleInputType : InputObjectType<AssignPermissionToRole>
    {
        protected override void Configure(IInputObjectTypeDescriptor<AssignPermissionToRole> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name("AssignPermissionToRoleInput");

            descriptor.Field(t => t.PermissionId)
                .Type<NonNullType<IdType>>().Description("Permission Id.");

            descriptor.Field(t => t.RoleId)
                .Type<NonNullType<IdType>>().Description("Role Id.");

            descriptor.Field(t => t.Operation)
                .Type<LinkOperationTypeEnum>().Description("Operation.");
        }
    }
}