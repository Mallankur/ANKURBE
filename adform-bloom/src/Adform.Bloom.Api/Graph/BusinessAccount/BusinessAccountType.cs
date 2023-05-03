using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.BusinessAccount
{
    public class BusinessAccountType : ObjectType<Contracts.Output.BusinessAccount>
    {
        protected override void Configure(IObjectTypeDescriptor<Contracts.Output.BusinessAccount> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name(nameof(Contracts.Output.BusinessAccount));

            descriptor.Field(t => t.Id)
                .Type<NonNullType<IdType>>().Description("Id of the Business Account.");

            descriptor.Field(t => t.Name)
                .Type<NonNullType<StringType>>().Description("Business Account Name.");

            descriptor.Field(t => t.Description)
                .Type<NonNullType<StringType>>().Description("Business Account Description.");

            descriptor.Field(t => t.Enabled)
                .Type<NonNullType<BooleanType>>().Description("Business Account Enabled.");

            descriptor.Field(t => t.LegacyId)
                .Type<NonNullType<IntType>>().Description("Business Account LegacyId.");

            descriptor.Field(t => t.Status)
                .Type<BusinessAccountStatusEnum>().Description("Business Account Status.");

            descriptor.Field(t => t.Type)
                .Type<BusinessAccountTypeEnum>().Description("Business Account Type.");

            descriptor.Field(t => t.CreatedAt).Ignore();

            descriptor.Field(t => t.UpdatedAt).Ignore();
        }
    }
}