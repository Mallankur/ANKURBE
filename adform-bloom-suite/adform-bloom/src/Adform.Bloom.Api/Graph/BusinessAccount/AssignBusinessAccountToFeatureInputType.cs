using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Contracts.Input;
using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.BusinessAccount
{
    public class AssignBusinessAccountToFeatureInputType : InputObjectType<AssignBusinessAccountToFeature>
    {
        protected override void Configure(IInputObjectTypeDescriptor<AssignBusinessAccountToFeature> descriptor)
        {
            descriptor.Name("AssignBusinessAccountToFeatureInput");
            
            descriptor.Field(t => t.BusinessAccountId)
                .Type<NonNullType<IdType>>().Description("BusinessAccount Id.");
            
            descriptor.Field(t => t.FeatureId)
                .Type<NonNullType<IdType>>().Description("Feature Id.");
            
            descriptor.Field(t => t.Operation)
                .Type<NonNullType<LinkOperationTypeEnum>>().Description("Operation.");
        }
    }
}