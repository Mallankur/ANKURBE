using Adform.Bloom.Contracts.Input;
using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.User
{
    public class RoleBusinessAccountInputType : InputObjectType<RoleBusinessAccount>
    {
        protected override void Configure(IInputObjectTypeDescriptor<RoleBusinessAccount> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Name("RoleBusinessAccountInput");
            
            descriptor.Field(t => t.RoleId)
                .Type<NonNullType<IdType>>().Description("Role Id.");
            
            descriptor.Field(t => t.BusinessAccountId)
                .Type<NonNullType<IdType>>().Description("BusinessAccount Id.");
        }
    }
}