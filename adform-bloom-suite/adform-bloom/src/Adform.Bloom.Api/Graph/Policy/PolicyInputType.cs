using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.Policy
{
    public class PolicyInputType : InputObjectType<Contracts.Output.Policy>
    {
        protected override void Configure(IInputObjectTypeDescriptor<Contracts.Output.Policy> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name("PolicyInput");

            descriptor.Field(t => t.Id).Ignore();

            descriptor.Field(t => t.Name)
                .Type<NonNullType<StringType>>().Description("Policy Name.");

            descriptor.Field(t => t.Description)
                .Type<NonNullType<StringType>>().Description("Policy Description.");

            descriptor.Field(t => t.Enabled)
                .Type<NonNullType<BooleanType>>().Description("Policy Enabled.");

            descriptor.Field(t => t.CreatedAt).Ignore();

            descriptor.Field(t => t.UpdatedAt).Ignore();
        }
    }
}