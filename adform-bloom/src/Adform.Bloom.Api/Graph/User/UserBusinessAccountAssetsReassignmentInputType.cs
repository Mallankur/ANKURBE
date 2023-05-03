using Adform.Bloom.Api.Graph.BusinessAccount;
using Adform.Bloom.Contracts.Input;
using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.User
{
    public class UserBusinessAccountAssetsReassignmentInputType : InputObjectType<AssetsReassignment>
    {
        protected override void Configure(IInputObjectTypeDescriptor<AssetsReassignment> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name("UserBusinessAccountAssetsReassignmentInput");

            descriptor.Field(t => t.BusinessAccountType)
                .Type<NonNullType<BusinessAccountTypeEnum>>().Description("BusinessAccount Type.");

            descriptor.Field(t => t.LegacyBusinessAccountId)
                .Type<NonNullType<IntType>>().Description("Legacy BusinessAccount Id.");

            descriptor.Field(t => t.NewUserId)
                .Type<NonNullType<IdType>>().Description("New Assignee User Id.");
        }
    }
}