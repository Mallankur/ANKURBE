using Adform.Bloom.Read.Contracts.BusinessAccount;

namespace Adform.Bloom.Contracts.Output
{
    public class BusinessAccount : NamedNodeDto
    {
        public int LegacyId { get; set; }
        public BusinessAccountStatus Status { get; set; }
        public BusinessAccountType Type { get; set; }
    }
}