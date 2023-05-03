using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.LicensedFeature
{
    public class LicensedFeatureType : ObjectType<Contracts.Output.LicensedFeature>
    {
        protected override void Configure(IObjectTypeDescriptor<Contracts.Output.LicensedFeature> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name(nameof(Contracts.Output.LicensedFeature));

            descriptor.Field(t => t.Id)
                .Type<NonNullType<IdType>>().Description("Id of the Licensed Feature.");

            descriptor.Field(t => t.Name)
                .Type<NonNullType<StringType>>().Description("Licensed Feature Name.");

            descriptor.Field(t => t.Description)
                .Type<NonNullType<StringType>>().Description("Licensed Feature Description.");

            descriptor.Field(t => t.Enabled)
                .Type<NonNullType<BooleanType>>().Description("Licensed Feature Enabled.");

            descriptor.Field(t => t.CreatedAt)
                .Type<NonNullType<LongType>>().Description("Licensed Feature Created Timestamp.");

            descriptor.Field(t => t.UpdatedAt)
                .Type<NonNullType<LongType>>().Description("Licensed Feature Updated Timestamp.");
        }
    }
}