using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.Policy
{
    public class PolicyType : ObjectType<Contracts.Output.Policy>
    {
        protected override void Configure(IObjectTypeDescriptor<Contracts.Output.Policy> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name(nameof(Contracts.Output.Policy));

            descriptor.Field(t => t.Id)
                .Type<NonNullType<IdType>>().Description("Id of the Policy.");

            descriptor.Field(t => t.Name)
                .Type<NonNullType<StringType>>().Description("Policy Name.");

            descriptor.Field(t => t.Description)
                .Type<NonNullType<StringType>>().Description("Policy Description.");

            descriptor.Field(t => t.Enabled)
                .Type<NonNullType<BooleanType>>().Description("Policy Enabled.");

            descriptor.Field(t => t.CreatedAt)
                .Type<NonNullType<LongType>>().Description("Policy Created Timestamp.");

            descriptor.Field(t => t.UpdatedAt)
                .Type<NonNullType<LongType>>().Description("Policy Updated Timestamp.");
        }
    }
}