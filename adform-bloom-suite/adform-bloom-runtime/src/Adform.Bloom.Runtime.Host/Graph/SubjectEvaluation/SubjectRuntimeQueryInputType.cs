using Adform.Bloom.Application.Queries;
using HotChocolate.Types;

namespace Adform.Bloom.Runtime.Host.Graph.SubjectEvaluation
{
    public class SubjectRuntimeQueryInputType : InputObjectType<SubjectRuntimeQuery>
    {
        protected override void Configure(IInputObjectTypeDescriptor<SubjectRuntimeQuery> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name("SubjectRuntimeQueryInput");

            descriptor.Field(t => t.SubjectId)
                .Type<NonNullType<IdType>>().Description("Subject Id.");

            descriptor.Field(t => t.TenantIds)
                .Type<ListType<IdType>>().Description("BusinessAccount Ids.");

            descriptor.Field(t => t.TenantType)
                .Type<StringType>().Description("BusinessAccount Type.");

            descriptor.Field(t => t.TenantIds)
                .Type<ListType<IntType>>().Description("BusinessAccount Legacy Ids.");

            descriptor.Field(t => t.PolicyNames)
                .Type<ListType<StringType>>().Description("Policy Ids.");

            descriptor.Field(t => t.PolicyNames)
                .Type<NonNullType<BooleanType>>().Description("Is Inheritance Enabled.");
            
        }
    }
}