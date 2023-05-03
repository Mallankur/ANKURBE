using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Contracts.Input;
using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.Feature
{
    public class AssignPermissionToFeatureInputType : InputObjectType<AssignPermissionToFeature>
    {
        protected override void Configure(IInputObjectTypeDescriptor<AssignPermissionToFeature> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Name("AssignPermissionToFeatureInput");
            
            descriptor.Field(t => t.PermissionId)
                .Type<NonNullType<IdType>>().Description("Permission Id.");
            
            descriptor.Field(t => t.FeatureId)
                .Type<NonNullType<IdType>>().Description("Feature Id.");
            
            descriptor.Field(t => t.Operation)
                .Type<NonNullType<LinkOperationTypeEnum>>().Description("Operation.");
        }
    }
}