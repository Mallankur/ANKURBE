using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.Feature
{
    public class FeatureInputType : InputObjectType<Contracts.Output.Feature>
    {
        protected override void Configure(IInputObjectTypeDescriptor<Contracts.Output.Feature> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name("FeatureInput");

            descriptor.Field(t => t.Id).Ignore();

            descriptor.Field(t => t.Name)
                .Type<NonNullType<StringType>>().Description("Feature Name.");

            descriptor.Field(t => t.Description)
                .Type<NonNullType<StringType>>().Description("Feature Description.");

            descriptor.Field(t => t.Enabled)
                .Type<NonNullType<BooleanType>>().Description("Feature Enabled.");

            descriptor.Field(t => t.CreatedAt)
                .Type<NonNullType<LongType>>().Ignore();

            descriptor.Field(t => t.UpdatedAt)
                .Type<NonNullType<LongType>>().Ignore();
        }
    }
}