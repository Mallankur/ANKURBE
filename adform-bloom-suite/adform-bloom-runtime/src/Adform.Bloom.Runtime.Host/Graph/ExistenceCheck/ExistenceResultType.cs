using Adform.Bloom.Runtime.Read.Entities;
using HotChocolate.Types;

namespace Adform.Bloom.Runtime.Host.Graph.ExistenceCheck
{
    public class ExistenceResultType : ObjectType<ExistenceResult>
    {
        protected override void Configure(IObjectTypeDescriptor<ExistenceResult> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name(nameof(ExistenceResult));

            descriptor.Field(t => t.Exists)
                .Type<NonNullType<BooleanType>>().Description("Existence Flag.");
        }
    }
}