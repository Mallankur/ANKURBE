using System.Collections.Generic;

namespace Adform.Bloom.Common.Test.Commons
{
    public class TestOAuth
    {
        public string TokenEndpointUri { get; set; } = "";
        public IEnumerable<TestOAuthClients> Clients { get; set; }
    }
}