using Adform.Bloom.Read.Integration.Test.Setup;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Read.Integration.Test.Collections;

[CollectionDefinition(nameof(GrpcCollection))]
[Order(TestsConstants.GrpcTestsOrderStartsAt)]
public class GrpcCollection
{
        
}