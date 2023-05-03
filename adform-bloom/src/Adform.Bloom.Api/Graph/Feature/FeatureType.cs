using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.Feature
{
    public class FeatureType : ObjectType<Contracts.Output.Feature>
    {
        protected override void Configure(IObjectTypeDescriptor<Contracts.Output.Feature> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name(nameof(Contracts.Output.Feature));

            descriptor.Field(t => t.Id)
                .Type<NonNullType<IdType>>().Description("Id of the Feature.");

            descriptor.Field(t => t.Name)
                .Type<NonNullType<StringType>>().Description("Feature Name.");

            descriptor.Field(t => t.Description)
                .Type<NonNullType<StringType>>().Description("Feature Description.");

            descriptor.Field(t => t.Enabled)
                .Type<NonNullType<BooleanType>>().Description("Feature Enabled.");

            descriptor.Field(t => t.CreatedAt)
                .Type<NonNullType<LongType>>().Description("Feature Created Timestamp.");

            descriptor.Field(t => t.UpdatedAt)
                .Type<NonNullType<LongType>>().Description("Feature Updated Timestamp.");
        }
    }
}