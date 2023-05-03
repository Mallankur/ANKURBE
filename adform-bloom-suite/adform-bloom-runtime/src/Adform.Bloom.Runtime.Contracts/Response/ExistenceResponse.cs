using System.Runtime.Serialization;

namespace Adform.Bloom.Client.Contracts.Response
{
    [DataContract]
    public class ExistenceResponse
    {
        [DataMember(Order = 1)]
        public bool Exists { get; set; }
    }
}
