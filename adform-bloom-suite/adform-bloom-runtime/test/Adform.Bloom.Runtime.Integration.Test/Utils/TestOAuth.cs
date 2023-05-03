using System.Collections.Generic;

namespace Adform.Bloom.Runtime.Integration.Test.Utils
{
    public class TestOAuth
    {
        public string TokenEndpointUri { get; set; } = "";
        public IEnumerable<TestOAuthClients> Clients { get; set; }
    }
}