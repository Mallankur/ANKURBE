using Adform.Ciam.OngDb.Repository;
using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.Common
{
    public class SortOrderTypeEnum : EnumType<SortingOrder>
    {
        protected override void Configure(IEnumTypeDescriptor<SortingOrder> descriptor)
        {
            descriptor.Value(SortingOrder.Ascending)
                .Name("asc");
            
            descriptor.Value(SortingOrder.Descending)
                .Name("desc");
        }
    }
}